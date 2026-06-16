using Microsoft.AspNetCore.Http;

namespace EcommerceCA.Application.DTOs.Category;

// ── Requests ──────────────────────────────────────────────────────────────────
public class CreateCategoryDto
{
    public string     Name        { get; set; } = string.Empty;
    public string?    Description { get; set; }
    public IFormFile? Image       { get; set; }
}

public class UpdateCategoryDto
{
    public string?    Name        { get; set; }
    public string?    Description { get; set; }
    public IFormFile? Image       { get; set; }
    public bool?      IsActive    { get; set; }
}

// ── Response ──────────────────────────────────────────────────────────────────
public class CategoryResponseDto
{
    public int      Id           { get; set; }
    public string   Name         { get; set; } = string.Empty;
    public string?  Description  { get; set; }
    public string?  ImageUrl     { get; set; }
    public bool     IsActive     { get; set; }
    public int      ProductCount { get; set; }
    public DateTime CreatedAt    { get; set; }
}
