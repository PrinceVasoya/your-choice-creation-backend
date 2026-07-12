using Microsoft.AspNetCore.Http;

namespace EcommerceCA.Application.DTOs.Product;

// ── Requests ──────────────────────────────────────────────────────────────────
public class CreateProductDto
{
    public string     Name           { get; set; } = string.Empty;
    public string?    ProductCode    { get; set; }
    public string?    Description    { get; set; }
    public decimal    Price          { get; set; }
    public decimal?   DiscountPrice  { get; set; }
    public int        Stock          { get; set; }
    public int        CategoryId     { get; set; }
    public bool       IsCustomizable { get; set; } = false;
    public IFormFile? Image          { get; set; }
    public string?    ImageUrl       { get; set; }
    public List<CreateProductVariantDto>? Variants { get; set; }
}

public class UpdateProductDto
{
    public string?    Name           { get; set; }
    public string?    ProductCode    { get; set; }
    public string?    Description    { get; set; }
    public decimal?   Price          { get; set; }
    public decimal?   DiscountPrice  { get; set; }
    public int?       Stock          { get; set; }
    public int?       CategoryId     { get; set; }
    public bool?      IsActive       { get; set; }
    public bool?      IsCustomizable { get; set; }
    public IFormFile? Image          { get; set; }
    public string?    ImageUrl       { get; set; }
}

public class CreateProductVariantDto
{
    public string?  Size            { get; set; }
    public string?  Color           { get; set; }
    public string?  ColorHex        { get; set; }
    public decimal? PriceAdjustment { get; set; } = 0;
    public int      Stock           { get; set; }
    public string?  SKU             { get; set; }
}

public class ProductQueryParams
{
    public int     Page           { get; set; } = 1;
    public int     PageSize       { get; set; } = 10;
    public string? Search         { get; set; }
    public int?    CategoryId     { get; set; }
    public decimal? MinPrice      { get; set; }
    public decimal? MaxPrice      { get; set; }
    public bool?   IsCustomizable { get; set; }
    public bool?   InStock        { get; set; }
    public string  SortBy         { get; set; } = "createdAt";
    public string  SortOrder      { get; set; } = "desc";
}

// ── Responses ─────────────────────────────────────────────────────────────────
public class ProductResponseDto
{
    public int      Id             { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public string   ProductCode    { get; set; } = string.Empty;
    public string?  Description    { get; set; }
    public decimal  Price          { get; set; }
    public decimal? DiscountPrice  { get; set; }
    public decimal  EffectivePrice => DiscountPrice.HasValue ? Math.Min(Price, DiscountPrice.Value) : Price;
    public string?  ImageUrl       { get; set; }
    public int      Stock          { get; set; }
    public bool     IsActive       { get; set; }
    public bool     IsCustomizable { get; set; }
    public int      CategoryId     { get; set; }
    public string   CategoryName   { get; set; } = string.Empty;
    public DateTime CreatedAt      { get; set; }
    public List<ProductVariantResponseDto> Variants { get; set; } = new();
}

public class ProductVariantResponseDto
{
    public int      Id              { get; set; }
    public string?  Size            { get; set; }
    public string?  Color           { get; set; }
    public string?  ColorHex        { get; set; }
    public decimal? PriceAdjustment { get; set; }
    public int      Stock           { get; set; }
    public string?  SKU             { get; set; }
    public bool     IsActive        { get; set; }
}

public class PaginatedProductResult
{
    public List<ProductResponseDto> Items      { get; set; } = new();
    public int                      TotalCount { get; set; }
    public int                      Page       { get; set; }
    public int                      PageSize   { get; set; }
    public int TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext     => Page < TotalPages;
}
