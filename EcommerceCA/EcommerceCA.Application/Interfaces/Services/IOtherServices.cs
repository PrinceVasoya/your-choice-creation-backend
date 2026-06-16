using EcommerceCA.Application.DTOs.Order;
using EcommerceCA.Application.DTOs.Payment;
using EcommerceCA.Application.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace EcommerceCA.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<StripePaymentIntentResponseDto> CreateStripePaymentIntentAsync(int orderId);
    Task                                 ConfirmStripePaymentAsync(ConfirmPaymentDto dto);
    Task<object>                         CreateRazorpayOrderAsync(int orderId);
    Task                                 ConfirmRazorpayPaymentAsync(ConfirmPaymentDto dto);
}

public interface IUserService
{
    Task<UserProfileResponseDto>       GetProfileAsync(string userId);
    Task<UserProfileResponseDto>       UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<List<AddressResponseDto>>     GetAddressesAsync(string userId);
    Task<AddressResponseDto>           AddAddressAsync(string userId, CreateAddressDto dto);
    Task                               DeleteAddressAsync(string userId, int addressId);
    Task<List<AdminUserResponseDto>>   GetAllUsersAsync();
    Task                               AssignRoleAsync(AssignRoleDto dto);
    Task<bool>                         ToggleUserStatusAsync(string userId);
}

public interface IImageService
{
    Task<(string Url, string PublicId)> UploadAsync(IFormFile file, string folder);
    Task                                DeleteAsync(string publicId);
}

public interface IEmailService
{
    Task SendPasswordResetAsync(string email, string token);
    Task SendOrderConfirmationAsync(string email, OrderResponseDto order);
}

public interface IWhatsAppService
{
    Task SendOrderNotificationAsync(string adminPhone, string customerName, OrderResponseDto order);
}
