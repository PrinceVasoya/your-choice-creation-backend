using EcommerceCA.Application.DTOs.Auth;

namespace EcommerceCA.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task                  ForgotPasswordAsync(string email);
    Task                  ResetPasswordAsync(ResetPasswordRequestDto dto);
    Task                  ChangePasswordAsync(string userId, ChangePasswordRequestDto dto);
    Task                  LogoutAsync(string refreshToken);
}
