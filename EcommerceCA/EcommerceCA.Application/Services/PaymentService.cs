using EcommerceCA.Application.DTOs.Payment;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using Stripe;

namespace EcommerceCA.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IOrderRepository   _orderRepo;
    private readonly IConfiguration     _config;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IOrderRepository   orderRepo,
        IConfiguration     config)
    {
        _paymentRepo = paymentRepo;
        _orderRepo   = orderRepo;
        _config      = config;
    }

    public async Task<StripePaymentIntentResponseDto> CreateStripePaymentIntentAsync(int orderId)
    {
        var order = await _orderRepo.GetWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        // Stripe SDK call — StripeConfiguration.ApiKey set in Infrastructure
        var service = new Stripe.PaymentIntentService();
        var intent  = await service.CreateAsync(new Stripe.PaymentIntentCreateOptions
        {
            Amount   = (long)(order.TotalAmount * 100),
            Currency = "inr",
            Metadata = new Dictionary<string, string> { { "orderId", orderId.ToString() } }
        });

        var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
        if (payment != null)
        {
            payment.PaymentIntentId = intent.Id;
            await _paymentRepo.SaveChangesAsync();
        }

        return new StripePaymentIntentResponseDto
        {
            ClientSecret    = intent.ClientSecret,
            PaymentIntentId = intent.Id
        };
    }

    public async Task ConfirmStripePaymentAsync(ConfirmPaymentDto dto)
    {
        var payment = await _paymentRepo.GetByOrderIdAsync(dto.OrderId)
            ?? throw new NotFoundException("Payment for order", dto.OrderId);

        payment.TransactionId  = dto.TransactionId;
        payment.Status         = PaymentStatus.Success;
        payment.UpdatedAt      = DateTime.UtcNow;
        payment.Order.Status   = OrderStatus.Confirmed;
        payment.Order.UpdatedAt = DateTime.UtcNow;
        await _paymentRepo.SaveChangesAsync();
    }

    public async Task<object> CreateRazorpayOrderAsync(int orderId)
    {
        var order = await _orderRepo.GetWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        var keyId     = Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID") ?? _config["Razorpay:KeyId"];
        var keySecret = Environment.GetEnvironmentVariable("RAZORPAY_KEY_SECRET") ?? _config["Razorpay:KeySecret"];

        bool isMockMode = string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret) || keyId.Contains("dummy") || keySecret.Contains("dummy");

        try
        {
            var client = new Razorpay.Api.RazorpayClient(keyId, keySecret);
            
            var options = new Dictionary<string, object>
            {
                { "amount", (int)Math.Round(order.TotalAmount * 100) },
                { "currency", "INR" },
                { "receipt", order.OrderNumber }
            };

            var razorpayOrder = client.Order.Create(options);
            string razorpayOrderId = razorpayOrder["id"].ToString();

            var paymentRecord = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (paymentRecord != null)
            {
                paymentRecord.RazorpayOrderId = razorpayOrderId;
                await _paymentRepo.SaveChangesAsync();
            }

            return new
            {
                id = razorpayOrderId,
                amount = razorpayOrder["amount"],
                currency = razorpayOrder["currency"],
                receipt = razorpayOrder["receipt"],
                key = keyId,
                isMock = false
            };
        }
        catch (Exception)
        {
            if (isMockMode)
            {
                var mockOrderId = $"order_mock_{Guid.NewGuid().ToString()[..12]}";
                var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
                if (payment != null)
                {
                    payment.RazorpayOrderId = mockOrderId;
                    await _paymentRepo.SaveChangesAsync();
                }

                return new
                {
                    id = mockOrderId,
                    amount = (int)Math.Round(order.TotalAmount * 100),
                    currency = "INR",
                    receipt = order.OrderNumber,
                    key = "rzp_test_mockkeyid12345678",
                    isMock = true
                };
            }
            throw;
        }
    }

    public async Task ConfirmRazorpayPaymentAsync(ConfirmPaymentDto dto)
    {
        var payment = await _paymentRepo.GetByOrderIdAsync(dto.OrderId)
            ?? throw new NotFoundException("Payment for order", dto.OrderId);

        var order = payment.Order;

        var keyId     = Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID") ?? _config["Razorpay:KeyId"];
        var keySecret = Environment.GetEnvironmentVariable("RAZORPAY_KEY_SECRET") ?? _config["Razorpay:KeySecret"];

        bool isMockMode = string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret) || keyId.Contains("dummy") || keySecret.Contains("dummy") || (payment.RazorpayOrderId != null && payment.RazorpayOrderId.StartsWith("order_mock_"));

        if (!isMockMode)
        {
            var attributes = new Dictionary<string, string>
            {
                { "razorpay_payment_id", dto.TransactionId },
                { "razorpay_order_id", payment.RazorpayOrderId ?? string.Empty },
                { "razorpay_signature", dto.RazorpaySignature ?? string.Empty }
            };

            try
            {
                Razorpay.Api.Utils.verifyPaymentSignature(attributes);
            }
            catch (Exception ex)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = "Signature verification failed: " + ex.Message;
                await _paymentRepo.SaveChangesAsync();
                throw new BadRequestException("Razorpay payment signature verification failed.");
            }
        }

        order.RazorpayOrderId = payment.RazorpayOrderId;
        order.RazorpayPaymentId = dto.TransactionId;
        order.RazorpaySignature = dto.RazorpaySignature;
        order.Status = OrderStatus.Paid;
        order.UpdatedAt = DateTime.UtcNow;

        payment.TransactionId = dto.TransactionId;
        payment.Status = PaymentStatus.Success;
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepo.SaveChangesAsync();
        await _orderRepo.SaveChangesAsync();
    }
}
