using EcommerceCA.Application.DTOs.User;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Constants;
using EcommerceCA.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceCA.API.Controllers;

/// <summary>User profile, addresses, and admin user management</summary>
[ApiController]
[Route("api/users")]
[Authorize]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService) => _userService = userService;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ── Profile ────────────────────────────────────────────────────────────────

    /// <summary>Get the current user's profile</summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponseDto>), 200)]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(CurrentUserId);
        return Ok(ApiResponse<UserProfileResponseDto>.Ok(result));
    }

    /// <summary>Update the current user's profile (name, phone, avatar)</summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponseDto>), 200)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
    {
        var result = await _userService.UpdateProfileAsync(CurrentUserId, dto);
        return Ok(ApiResponse<UserProfileResponseDto>.Ok(result, "Profile updated successfully."));
    }

    // ── Addresses ──────────────────────────────────────────────────────────────

    /// <summary>Get all saved addresses for the current user</summary>
    [HttpGet("addresses")]
    [ProducesResponseType(typeof(ApiResponse<List<AddressResponseDto>>), 200)]
    public async Task<IActionResult> GetAddresses()
    {
        var result = await _userService.GetAddressesAsync(CurrentUserId);
        return Ok(ApiResponse<List<AddressResponseDto>>.Ok(result));
    }

    /// <summary>Add a new shipping address</summary>
    [HttpPost("addresses")]
    [ProducesResponseType(typeof(ApiResponse<AddressResponseDto>), 201)]
    public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto dto)
    {
        var result = await _userService.AddAddressAsync(CurrentUserId, dto);
        return StatusCode(201, ApiResponse<AddressResponseDto>.Created(result, "Address added."));
    }

    /// <summary>Delete a saved address</summary>
    [HttpDelete("addresses/{addressId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteAddress(int addressId)
    {
        await _userService.DeleteAddressAsync(CurrentUserId, addressId);
        return Ok(ApiResponse<object>.Ok(null!, "Address deleted."));
    }

    // ── Admin ──────────────────────────────────────────────────────────────────

    /// <summary>Get all registered users — Admin only</summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<List<AdminUserResponseDto>>), 200)]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<List<AdminUserResponseDto>>.Ok(result));
    }

    /// <summary>Assign a role (Admin | User) to a user — Admin only</summary>
    [HttpPost("admin/assign-role")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        await _userService.AssignRoleAsync(dto);
        return Ok(ApiResponse<object>.Ok(null!, $"Role '{dto.Role}' assigned to user."));
    }

    /// <summary>Activate or deactivate a user account — Admin only</summary>
    [HttpPut("admin/{userId}/toggle-status")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ToggleStatus(string userId)
    {
        var isActive = await _userService.ToggleUserStatusAsync(userId);
        var msg      = isActive ? "User account activated." : "User account deactivated.";
        return Ok(ApiResponse<object>.Ok(null!, msg));
    }
}
