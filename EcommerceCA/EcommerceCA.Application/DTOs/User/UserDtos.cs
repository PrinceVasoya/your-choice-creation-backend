using Microsoft.AspNetCore.Http;

namespace EcommerceCA.Application.DTOs.User;

// ── Requests ──────────────────────────────────────────────────────────────────
public class UpdateProfileDto
{
    public string?    FirstName   { get; set; }
    public string?    LastName    { get; set; }
    public string?    PhoneNumber { get; set; }
    public IFormFile? ProfileImage{ get; set; }
}

public class CreateAddressDto
{
    public string  FullName     { get; set; } = string.Empty;
    public string  Phone        { get; set; } = string.Empty;
    public string  AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string  City         { get; set; } = string.Empty;
    public string  State        { get; set; } = string.Empty;
    public string  PostalCode   { get; set; } = string.Empty;
    public string  Country      { get; set; } = "India";
    public bool    IsDefault    { get; set; } = false;
}

public class AssignRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string Role   { get; set; } = string.Empty;
}

// ── Responses ─────────────────────────────────────────────────────────────────
public class UserProfileResponseDto
{
    public string  Id             { get; set; } = string.Empty;
    public string  FirstName      { get; set; } = string.Empty;
    public string  LastName       { get; set; } = string.Empty;
    public string  FullName       => $"{FirstName} {LastName}".Trim();
    public string  Email          { get; set; } = string.Empty;
    public string? PhoneNumber    { get; set; }
    public string? ProfileImageUrl{ get; set; }
    public IList<string> Roles    { get; set; } = new List<string>();
}

public class AddressResponseDto
{
    public int     Id           { get; set; }
    public string  FullName     { get; set; } = string.Empty;
    public string  Phone        { get; set; } = string.Empty;
    public string  AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string  City         { get; set; } = string.Empty;
    public string  State        { get; set; } = string.Empty;
    public string  PostalCode   { get; set; } = string.Empty;
    public string  Country      { get; set; } = string.Empty;
    public bool    IsDefault    { get; set; }
}

public class AdminUserResponseDto
{
    public string  Id          { get; set; } = string.Empty;
    public string  FirstName   { get; set; } = string.Empty;
    public string  LastName    { get; set; } = string.Empty;
    public string  Email       { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool    IsActive    { get; set; }
    public DateTime CreatedAt  { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
