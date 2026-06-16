using EcommerceCA.Application.DTOs.Auth;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceCA.API.Controllers;

/// <summary>Authentication — register, login, token refresh, password management</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Register a new user account</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return StatusCode(201, ApiResponse<AuthResponseDto>.Created(result, "Registration successful."));
    }

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    /// <summary>Get a new access token using a refresh token</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    /// <summary>Send a password reset link to the given email</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(ApiResponse<object>.Ok(null!, "If that email exists, a reset link has been sent."));
    }

    /// <summary>Reset password using the token received by email</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(ApiResponse<object>.Ok(null!, "Password has been reset successfully."));
    }

    /// <summary>Change password for the currently logged-in user</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
    {
        await _authService.ChangePasswordAsync(CurrentUserId, dto);
        return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully."));
    }

    /// <summary>Logout and revoke the refresh token</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully."));
    }
}
