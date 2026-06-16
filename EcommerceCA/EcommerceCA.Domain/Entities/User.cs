using Microsoft.AspNetCore.Identity;

namespace EcommerceCA.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName       { get; set; } = string.Empty;
    public string LastName        { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool   IsActive        { get; set; } = true;
    public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;

    // Navigation
    public Cart?                     Cart          { get; set; }
    public ICollection<Order>        Orders        { get; set; } = new List<Order>();
    public ICollection<Address>      Addresses     { get; set; } = new List<Address>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}

public class RefreshToken
{
    public int      Id        { get; set; }
    public string   UserId    { get; set; } = string.Empty;
    public string   Token     { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool     IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}

public class Address
{
    public int     Id           { get; set; }
    public string  UserId       { get; set; } = string.Empty;
    public string  FullName     { get; set; } = string.Empty;
    public string  Phone        { get; set; } = string.Empty;
    public string  AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string  City         { get; set; } = string.Empty;
    public string  State        { get; set; } = string.Empty;
    public string  PostalCode   { get; set; } = string.Empty;
    public string  Country      { get; set; } = "India";
    public bool    IsDefault    { get; set; } = false;
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

    public ApplicationUser       User   { get; set; } = null!;
    public ICollection<Order> Orders    { get; set; } = new List<Order>();
}
