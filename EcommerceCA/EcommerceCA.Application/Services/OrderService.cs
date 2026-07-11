using AutoMapper;
using EcommerceCA.Application.DTOs.Order;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Constants;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Domain.Enums;

namespace EcommerceCA.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository   _orderRepo;
    private readonly ICartRepository    _cartRepo;
    private readonly IUserRepository    _userRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IWhatsAppService   _whatsApp;
    private readonly IMapper            _mapper;
    private readonly IProductRepository _productRepo; // Added (Fix 6, Part A)

    public OrderService(
        IOrderRepository   orderRepo,
        ICartRepository    cartRepo,
        IUserRepository    userRepo,
        IPaymentRepository paymentRepo,
        IWhatsAppService   whatsApp,
        IMapper            mapper,
        IProductRepository productRepo) // Added
    {
        _orderRepo   = orderRepo;
        _cartRepo    = cartRepo;
        _userRepo    = userRepo;
        _paymentRepo = paymentRepo;
        _whatsApp    = whatsApp;
        _mapper      = mapper;
        _productRepo = productRepo; // Added
    }

    public async Task<OrderResponseDto> PlaceOrderAsync(string userId, PlaceOrderDto dto)
    {
        var cart = await _cartRepo.GetWithItemsAsync(userId);
        if (cart == null || !cart.CartItems.Any())
            throw new BadRequestException("Your cart is empty. Add items before placing an order.");

        // Enforce inventory check for all items first (Fix 1, Stock Validation Everywhere)
        var stockErrors = new List<StockErrorDto>();
        foreach (var item in cart.CartItems)
        {
            if (item.Product.Stock < item.Quantity)
            {
                stockErrors.Add(new StockErrorDto
                {
                    Name = item.Product.Name,
                    Error = $"Only {item.Product.Stock} units of '{item.Product.Name}' are available.",
                    Available = item.Product.Stock,
                    Requested = item.Quantity
                });
            }
        }

        if (stockErrors.Any())
        {
            throw new StockValidationException(stockErrors, "Some items in your cart have stock issues.");
        }

        var address = await _userRepo.GetAddressAsync(dto.ShippingAddressId, userId)
            ?? throw new NotFoundException("ShippingAddress", dto.ShippingAddressId);

        decimal subTotal   = 0;
        var     orderItems = new List<OrderItem>();

        foreach (var item in cart.CartItems)
        {
            if (!item.Product.IsActive)
                throw new BadRequestException($"Product '{item.Product.Name}' is no longer available.");

            // Always charge the lower of Price vs DiscountPrice (matches frontend Math.min logic).
            // DiscountPrice ?? Price would pick DiscountPrice even if it is higher — causing mismatch.
            var unitPrice = item.Product.DiscountPrice.HasValue
                ? Math.Min(item.Product.Price, item.Product.DiscountPrice.Value)
                : item.Product.Price;
            // Note: ProductVariant.PriceAdjustment is not used — variants in this project
            // are label-only (Size/Color names) and do not carry extra price deltas.

            var variantParts = new List<string>();
            if (item.ProductVariant?.Size  != null) variantParts.Add($"Size: {item.ProductVariant.Size}");
            if (item.ProductVariant?.Color != null) variantParts.Add($"Color: {item.ProductVariant.Color}");

            orderItems.Add(new OrderItem
            {
                ProductId         = item.ProductId,
                ProductVariantId  = item.ProductVariantId,
                Quantity          = item.Quantity,
                UnitPrice         = unitPrice,
                TotalPrice        = unitPrice * item.Quantity,
                ProductName       = item.Product.Name,
                VariantInfo       = variantParts.Any() ? string.Join(", ", variantParts) : null,
                CustomizationNote = item.CustomizationNote
            });

            subTotal += unitPrice * item.Quantity;
        }

        var shippingCost = 0m;
        var taxAmount    = 0m;
        var totalAmount  = subTotal;

        var order = new Order
        {
            OrderNumber       = GenerateOrderNumber(),
            UserId            = userId,
            ShippingAddressId = dto.ShippingAddressId,
            SubTotal          = subTotal,
            ShippingCost      = shippingCost,
            TaxAmount         = taxAmount,
            TotalAmount       = totalAmount,
            GrandTotal        = totalAmount,
            Notes             = dto.Notes,
            Status            = OrderStatus.Pending,
            OrderItems        = orderItems
        };

        await _orderRepo.AddAsync(order);

        await _paymentRepo.AddAsync(new Payment
        {
            Order    = order,
            Amount   = totalAmount,
            Method   = dto.PaymentMethod,
            Status   = PaymentStatus.Pending,
            Currency = "INR"
        });

        await _paymentRepo.SaveChangesAsync();

        // Reduce stock in DB for each item in the order (Fix 6, Part A)
        foreach (var item in cart.CartItems)
        {
            try
            {
                var prod = await _productRepo.GetByIdAsync(item.ProductId);
                if (prod != null)
                {
                    prod.Stock = Math.Max(0, prod.Stock - item.Quantity);
                    await _productRepo.UpdateAsync(prod);
                }
            }
            catch (Exception ex)
            {
                // If any stock update fails, log the error but do not fail the order
                Console.WriteLine($"[STOCK REDUCTION ERROR] Failed to reduce stock for product {item.ProductId}: {ex.Message}");
            }
        }
        await _productRepo.SaveChangesAsync();

        // Clear cart after successful order
        await _cartRepo.ClearAsync(userId);
        await _cartRepo.SaveChangesAsync();

        var saved = await _orderRepo.GetWithDetailsAsync(order.Id, userId);
        var result = _mapper.Map<OrderResponseDto>(saved);

        // Fire-and-forget WhatsApp notification to admin
        var adminPhone = Environment.GetEnvironmentVariable("ADMIN_WHATSAPP_NUMBER") ?? string.Empty;
        if (!string.IsNullOrEmpty(adminPhone))
        {
            var customerName = $"{cart.User?.FirstName} {cart.User?.LastName}".Trim();
            _ = _whatsApp.SendOrderNotificationAsync(adminPhone, customerName, result);
        }

        return result;
    }

    public async Task<PaginatedOrderResult> GetUserOrdersAsync(string userId, int page, int pageSize)
    {
        var (items, total) = await _orderRepo.GetUserOrdersPagedAsync(userId, page, pageSize);
        return new PaginatedOrderResult
        {
            Items      = _mapper.Map<List<OrderResponseDto>>(items),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(string userId, int orderId)
    {
        var order = await _orderRepo.GetWithDetailsAsync(orderId, userId)
            ?? throw new NotFoundException("Order", orderId);
        return _mapper.Map<OrderResponseDto>(order);
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId)
    {
        var order = await _orderRepo.GetWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);
        return _mapper.Map<OrderResponseDto>(order);
    }

    public async Task<PaginatedOrderResult> GetAllOrdersAsync(int page, int pageSize, string? status)
    {
        OrderStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var s))
            parsed = s;

        var (items, total) = await _orderRepo.GetAllOrdersPagedAsync(page, pageSize, parsed);
        return new PaginatedOrderResult
        {
            Items      = _mapper.Map<List<OrderResponseDto>>(items),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
    {
        if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var status))
            throw new BadRequestException($"'{dto.Status}' is not a valid order status.");

        var order = await _orderRepo.GetWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        order.Status    = status;
        if (dto.TrackingUrl != null)
        {
            order.TrackingUrl = dto.TrackingUrl;
        }
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepo.UpdateAsync(order);
        await _orderRepo.SaveChangesAsync();

        return _mapper.Map<OrderResponseDto>(order);
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
}
