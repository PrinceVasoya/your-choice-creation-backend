using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EcommerceCA.Application.DTOs.Order;
using EcommerceCA.Application.DTOs.Payment;
using EcommerceCA.Application.DTOs.User;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Domain.Enums;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Stripe;
using System.Net.Http.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace EcommerceCA.Infrastructure.ExternalServices;

// ── Image Service ──────────────────────────────────────────────────────────────
public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;
    private readonly IConfiguration _config;
    public ImageService(IConfiguration config)
    {
        _config = config;
        var cloudName = config["Cloudinary:CloudName"];
        if (!string.IsNullOrEmpty(cloudName) && cloudName != "YOUR_CLOUD_NAME")
        {
            var acct = new CloudinaryDotNet.Account(cloudName, config["Cloudinary:ApiKey"], config["Cloudinary:ApiSecret"]);
            _cloudinary = new Cloudinary(acct);
        }
    }

    public async Task<(string Url, string PublicId)> UploadAsync(IFormFile file, string folder)
    {
        var cloudName = _config["Cloudinary:CloudName"];
        if (string.IsNullOrEmpty(cloudName) || cloudName == "YOUR_CLOUD_NAME")
        {
            try
            {
                var wwwroot = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                var uploads = System.IO.Path.Combine(wwwroot, "uploads", folder);
                if (!System.IO.Directory.Exists(uploads))
                {
                    System.IO.Directory.CreateDirectory(uploads);
                }
                var uniqueName = $"{System.Guid.NewGuid()}{System.IO.Path.GetExtension(file.FileName)}";
                var filePath = System.IO.Path.Combine(uploads, uniqueName);
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var url = $"/uploads/{folder}/{uniqueName}";
                return (url, uniqueName);
            }
            catch (System.Exception ex)
            {
                throw new BadRequestException($"Local image upload failed: {ex.Message}");
            }
        }
        else
        {
            if (_cloudinary == null)
            {
                throw new BadRequestException("Cloudinary is not initialized.");
            }
            using var stream = file.OpenReadStream();
            var result = await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File           = new FileDescription(file.FileName, stream),
                Folder         = $"ecommerce/{folder}",
                Transformation = new Transformation().Width(800).Height(800).Crop("limit").Quality("auto")
            });
            if (result.Error != null) throw new BadRequestException($"Image upload failed: {result.Error.Message}");
            return (result.SecureUrl.ToString(), result.PublicId);
        }
    }

    public async Task DeleteAsync(string publicId)
    {
        var cloudName = _config["Cloudinary:CloudName"];
        if (string.IsNullOrEmpty(cloudName) || cloudName == "YOUR_CLOUD_NAME")
        {
            try
            {
                var wwwroot = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                if (System.IO.Directory.Exists(wwwroot))
                {
                    var files = System.IO.Directory.GetFiles(wwwroot, publicId, System.IO.SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        if (System.IO.File.Exists(file))
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                }
            }
            catch
            {
                // Ignore fallback deletion failures
            }
            await Task.CompletedTask;
        }
        else
        {
            if (_cloudinary != null)
            {
                await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            }
        }
    }
}

// ── Payment Service ────────────────────────────────────────────────────────────
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IOrderRepository   _orderRepo;
    private readonly IConfiguration     _config;

    public PaymentService(IPaymentRepository paymentRepo, IOrderRepository orderRepo, IConfiguration config)
    {
        _paymentRepo = paymentRepo; _orderRepo = orderRepo; _config = config;
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
    }

    public async Task<StripePaymentIntentResponseDto> CreateStripePaymentIntentAsync(int orderId)
    {
        var order  = await _orderRepo.GetWithDetailsAsync(orderId) ?? throw new NotFoundException("Order", orderId);
        var intent = await new PaymentIntentService().CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)(order.TotalAmount * 100), Currency = "inr",
            Metadata = new Dictionary<string, string> { ["orderId"] = orderId.ToString() }
        });
        var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
        if (payment != null) { payment.PaymentIntentId = intent.Id; await _paymentRepo.SaveChangesAsync(); }
        return new StripePaymentIntentResponseDto { ClientSecret = intent.ClientSecret, PaymentIntentId = intent.Id };
    }

    public async Task ConfirmStripePaymentAsync(ConfirmPaymentDto dto)
    {
        var payment = await _paymentRepo.GetByOrderIdAsync(dto.OrderId) ?? throw new NotFoundException("Payment", dto.OrderId);
        payment.TransactionId = dto.TransactionId; payment.Status = PaymentStatus.Success;
        payment.UpdatedAt = DateTime.UtcNow; payment.Order.Status = OrderStatus.Confirmed; payment.Order.UpdatedAt = DateTime.UtcNow;
        await _paymentRepo.SaveChangesAsync();
    }

    public async Task<object> CreateRazorpayOrderAsync(int orderId)
    {
        var order = await _orderRepo.GetWithDetailsAsync(orderId) ?? throw new NotFoundException("Order", orderId);
        using var client = new HttpClient();
        var creds = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_config["Razorpay:KeyId"]}:{_config["Razorpay:KeySecret"]}"));
        client.DefaultRequestHeaders.Add("Authorization", $"Basic {creds}");
        var resp = await client.PostAsJsonAsync("https://api.razorpay.com/v1/orders",
            new { amount = (int)(order.TotalAmount * 100), currency = "INR", receipt = order.OrderNumber });
        return await resp.Content.ReadFromJsonAsync<object>() ?? new { };
    }

    public async Task ConfirmRazorpayPaymentAsync(ConfirmPaymentDto dto)
    {
        var payment = await _paymentRepo.GetByOrderIdAsync(dto.OrderId) ?? throw new NotFoundException("Payment", dto.OrderId);
        payment.TransactionId = dto.TransactionId; payment.RazorpayOrderId = dto.RazorpaySignature;
        payment.Status = PaymentStatus.Success; payment.UpdatedAt = DateTime.UtcNow;
        payment.Order.Status = OrderStatus.Confirmed; payment.Order.UpdatedAt = DateTime.UtcNow;
        await _paymentRepo.SaveChangesAsync();
    }
}

// ── WhatsApp Service ───────────────────────────────────────────────────────────
public class WhatsAppService : IWhatsAppService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(IConfiguration config, ILogger<WhatsAppService> logger)
    {
        _config = config; _logger = logger;
        TwilioClient.Init(config["Twilio:AccountSid"], config["Twilio:AuthToken"]);
    }

    public async Task SendOrderNotificationAsync(string adminPhone, string customerName, OrderResponseDto order)
    {
        try
        {
            var lines = string.Join("\n", order.Items.Select(i => $"  • {i.ProductName} x{i.Quantity} = ₹{i.TotalPrice:F2}"));
            var msg = $"🛒 *New Order!*\n👤 {customerName}\n📦 {order.OrderNumber}\n{lines}\n✅ *Total: ₹{order.TotalAmount:F2}*\n📍 {order.ShippingAddress?.City}, {order.ShippingAddress?.State}";
            await MessageResource.CreateAsync(body: msg,
                from: new Twilio.Types.PhoneNumber($"whatsapp:{_config["Twilio:FromNumber"]}"),
                to:   new Twilio.Types.PhoneNumber($"whatsapp:{adminPhone}"));
        }
        catch (Exception ex) { _logger.LogError(ex, "WhatsApp failed for {Order}", order.OrderNumber); }
    }
}

// ── Email Service ──────────────────────────────────────────────────────────────
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger) { _config = config; _logger = logger; }

    public async Task SendPasswordResetAsync(string email, string token)
    {
        var link = $"{_config["Frontend:BaseUrl"]}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
        await SendAsync(email, "Reset Your Password",
            $"<h2>Password Reset</h2><p>Click below to reset your password (expires in 1 hour):</p><a href='{link}' style='background:#4F46E5;color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;'>Reset Password</a>");
    }

    public async Task SendOrderConfirmationAsync(string email, OrderResponseDto order) =>
        await SendAsync(email, $"Order Confirmed — {order.OrderNumber}",
            $"<h2>Order Confirmed 🎉</h2><p>Order: <strong>{order.OrderNumber}</strong></p><p>Total: <strong>₹{order.TotalAmount:F2}</strong></p>");

    private async Task SendAsync(string to, string subject, string html)
    {
        try
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_config["Email:FromAddress"]));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = html };
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await smtp.SendAsync(msg); await smtp.DisconnectAsync(true);
        }
        catch (Exception ex) { _logger.LogError(ex, "Email failed to {Email}", to); }
    }
}

// ── User Service ───────────────────────────────────────────────────────────────
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepo;
    private readonly IImageService _imageService;

    public UserService(UserManager<ApplicationUser> userManager, IUserRepository userRepo, IImageService imageService)
    { _userManager = userManager; _userRepo = userRepo; _imageService = imageService; }

    public async Task<UserProfileResponseDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User", userId);
        return await ToDto(user);
    }

    public async Task<UserProfileResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User", userId);
        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
        if (dto.ProfileImage != null) { var (url, _) = await _imageService.UploadAsync(dto.ProfileImage, "avatars"); user.ProfileImageUrl = url; }
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        return await ToDto(user);
    }

    public async Task<List<AddressResponseDto>> GetAddressesAsync(string userId)
        => (await _userRepo.GetAddressesAsync(userId)).Select(ToAddressDto).ToList();

    public async Task<AddressResponseDto> AddAddressAsync(string userId, CreateAddressDto dto)
    {
        if (dto.IsDefault) { var ex = await _userRepo.GetAddressesAsync(userId); foreach (var a in ex.Where(a => a.IsDefault)) a.IsDefault = false; }
        var address = new EcommerceCA.Domain.Entities.Address { UserId = userId, FullName = dto.FullName, Phone = dto.Phone, AddressLine1 = dto.AddressLine1, AddressLine2 = dto.AddressLine2, City = dto.City, State = dto.State, PostalCode = dto.PostalCode, Country = dto.Country, IsDefault = dto.IsDefault };
        await _userRepo.AddAddressAsync(address); await _userRepo.SaveChangesAsync();
        return ToAddressDto(address);
    }

    public async Task DeleteAddressAsync(string userId, int addressId)
    {
        var addr = await _userRepo.GetAddressAsync(addressId, userId) ?? throw new NotFoundException("Address", addressId);
        await _userRepo.RemoveAddressAsync(addr); await _userRepo.SaveChangesAsync();
    }

    public async Task<List<AdminUserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        var result = new List<AdminUserResponseDto>();
        foreach (var u in users) { var r = await _userManager.GetRolesAsync(u); result.Add(new AdminUserResponseDto { Id = u.Id, FirstName = u.FirstName, LastName = u.LastName, Email = u.Email!, PhoneNumber = u.PhoneNumber, IsActive = u.IsActive, CreatedAt = u.CreatedAt, Roles = r }); }
        return result;
    }

    public async Task AssignRoleAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId) ?? throw new NotFoundException("User", dto.UserId);
        if (!new[] { "Admin", "User" }.Contains(dto.Role)) throw new BadRequestException($"Invalid role '{dto.Role}'.");
        await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
        await _userManager.AddToRoleAsync(user, dto.Role);
    }

    public async Task<bool> ToggleUserStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User", userId);
        user.IsActive = !user.IsActive; await _userManager.UpdateAsync(user); return user.IsActive;
    }

    private async Task<UserProfileResponseDto> ToDto(ApplicationUser u)
    {
        var roles = await _userManager.GetRolesAsync(u);
        return new UserProfileResponseDto { Id = u.Id, FirstName = u.FirstName, LastName = u.LastName, Email = u.Email!, PhoneNumber = u.PhoneNumber, ProfileImageUrl = u.ProfileImageUrl, Roles = roles };
    }

    private static AddressResponseDto ToAddressDto(EcommerceCA.Domain.Entities.Address a) => new() { Id = a.Id, FullName = a.FullName, Phone = a.Phone, AddressLine1 = a.AddressLine1, AddressLine2 = a.AddressLine2, City = a.City, State = a.State, PostalCode = a.PostalCode, Country = a.Country, IsDefault = a.IsDefault };
}
