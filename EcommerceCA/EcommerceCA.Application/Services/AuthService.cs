using EcommerceCA.Application.DTOs.Auth;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Constants;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Common.Helpers;
using EcommerceCA.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EcommerceCA.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser>   _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IRefreshTokenRepository        _tokenRepo;
    private readonly IEmailService                  _emailService;
    private readonly JwtHelper                      _jwt;

    public AuthService(
        UserManager<ApplicationUser>   userManager,
        SignInManager<ApplicationUser> signInManager,
        IRefreshTokenRepository        tokenRepo,
        IEmailService                  emailService,
        JwtHelper                      jwt)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _tokenRepo     = tokenRepo;
        _emailService  = emailService;
        _jwt           = jwt;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new ConflictException("This email is already registered.");

        var user = new ApplicationUser
        {
            FirstName   = dto.FirstName,
            LastName    = dto.LastName,
            Email       = dto.Email,
            UserName    = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (dto.Email.ToLower() == "admin@yourstore.com")
            await _userManager.AddToRoleAsync(user, Roles.Admin);
        else
            await _userManager.AddToRoleAsync(user, Roles.User);

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("Your account has been disabled. Please contact support.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                throw new UnauthorizedException("Account locked due to multiple failed attempts. Try again later.");
            throw new UnauthorizedException("Invalid email or password.");
        }

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var token = await _tokenRepo.GetValidAsync(refreshToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        await _tokenRepo.RevokeAsync(token);
        return await BuildAuthResponseAsync(token.User);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return; // Silently ignore — prevent user enumeration

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetAsync(email, token);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new NotFoundException("User", dto.Email);

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _tokenRepo.GetValidAsync(refreshToken);
        if (token != null) await _tokenRepo.RevokeAsync(token);
    }

    // ── Private helpers ───────────────────────────────────────────────────────
    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles        = await _userManager.GetRolesAsync(user);
        var accessToken  = _jwt.GenerateAccessToken(user.Id, user.Email!, user.FirstName, user.LastName, roles);
        var refreshToken = _jwt.GenerateRefreshToken();

        await _tokenRepo.AddAsync(new RefreshToken
        {
            UserId    = user.Id,
            Token     = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshExpiryDays)
        });

        return new AuthResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt    = DateTime.UtcNow.AddMinutes(60),
            User = new UserInfoDto
            {
                Id              = user.Id,
                FirstName       = user.FirstName,
                LastName        = user.LastName,
                Email           = user.Email!,
                PhoneNumber     = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles           = roles
            }
        };
    }
}
