using EcommerceCA.Application.DTOs.User;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository              _userRepo;
    private readonly IImageService                _imageService;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IUserRepository              userRepo,
        IImageService                imageService)
    {
        _userManager  = userManager;
        _userRepo     = userRepo;
        _imageService = imageService;
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(string userId)
    {
        var user  = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);
        var roles = await _userManager.GetRolesAsync(user);
        return MapProfile(user, roles);
    }

    public async Task<UserProfileResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (dto.FirstName   != null) user.FirstName   = dto.FirstName;
        if (dto.LastName    != null) user.LastName     = dto.LastName;
        if (dto.PhoneNumber != null) user.PhoneNumber  = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        if (dto.ProfileImage != null)
        {
            var (url, _)         = await _imageService.UploadAsync(dto.ProfileImage, "avatars");
            user.ProfileImageUrl = url;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roles = await _userManager.GetRolesAsync(user);
        return MapProfile(user, roles);
    }

    public async Task<List<AddressResponseDto>> GetAddressesAsync(string userId)
    {
        var addresses = await _userRepo.GetAddressesAsync(userId);
        return addresses.Select(MapAddress).ToList();
    }

    public async Task<AddressResponseDto> AddAddressAsync(string userId, CreateAddressDto dto)
    {
        if (dto.IsDefault)
        {
            var existing = await _userRepo.GetAddressesAsync(userId);
            foreach (var a in existing.Where(a => a.IsDefault))
                a.IsDefault = false;
        }

        var address = new Address
        {
            UserId       = userId,
            FullName     = dto.FullName,
            Phone        = dto.Phone,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City         = dto.City,
            State        = dto.State,
            PostalCode   = dto.PostalCode,
            Country      = dto.Country,
            IsDefault    = dto.IsDefault
        };

        await _userRepo.AddAddressAsync(address);
        await _userRepo.SaveChangesAsync();
        return MapAddress(address);
    }

    public async Task DeleteAddressAsync(string userId, int addressId)
    {
        var address = await _userRepo.GetAddressAsync(addressId, userId)
            ?? throw new NotFoundException("Address", addressId);
        await _userRepo.RemoveAddressAsync(address);
        await _userRepo.SaveChangesAsync();
    }

    public async Task<List<AdminUserResponseDto>> GetAllUsersAsync()
    {
        var users  = await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        var result = new List<AdminUserResponseDto>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new AdminUserResponseDto
            {
                Id          = u.Id,
                FirstName   = u.FirstName,
                LastName    = u.LastName,
                Email       = u.Email!,
                PhoneNumber = u.PhoneNumber,
                IsActive    = u.IsActive,
                CreatedAt   = u.CreatedAt,
                Roles       = roles
            });
        }
        return result;
    }

    public async Task AssignRoleAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId)
            ?? throw new NotFoundException("User", dto.UserId);

        if (!new[] { "Admin", "User" }.Contains(dto.Role))
            throw new BadRequestException($"Invalid role '{dto.Role}'. Valid roles: Admin, User.");

        var current = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, current);
        await _userManager.AddToRoleAsync(user, dto.Role);
    }

    public async Task<bool> ToggleUserStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return user.IsActive;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static UserProfileResponseDto MapProfile(ApplicationUser u, IList<string> roles) => new()
    {
        Id              = u.Id,
        FirstName       = u.FirstName,
        LastName        = u.LastName,
        Email           = u.Email!,
        PhoneNumber     = u.PhoneNumber,
        ProfileImageUrl = u.ProfileImageUrl,
        Roles           = roles
    };

    private static AddressResponseDto MapAddress(Address a) => new()
    {
        Id           = a.Id,
        FullName     = a.FullName,
        Phone        = a.Phone,
        AddressLine1 = a.AddressLine1,
        AddressLine2 = a.AddressLine2,
        City         = a.City,
        State        = a.State,
        PostalCode   = a.PostalCode,
        Country      = a.Country,
        IsDefault    = a.IsDefault
    };
}
