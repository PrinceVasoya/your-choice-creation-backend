namespace EcommerceCA.Application.DTOs.Cart;

// ── Requests ──────────────────────────────────────────────────────────────────
public class AddToCartDto
{
    public int     ProductId         { get; set; }
    public int?    ProductVariantId  { get; set; }
    public int     Quantity          { get; set; } = 1;
    public string? CustomizationNote { get; set; }
}

public class UpdateCartItemDto
{
    public int Quantity { get; set; }
}

// ── Responses ─────────────────────────────────────────────────────────────────
public class CartResponseDto
{
    public int                    Id        { get; set; }
    public List<CartItemResponseDto> Items  { get; set; } = new();
    public decimal                SubTotal  { get; set; }
    public int                    ItemCount { get; set; }
}

public class CartItemResponseDto
{
    public int     Id               { get; set; }
    public int     ProductId        { get; set; }
    public string  ProductName      { get; set; } = string.Empty;
    public string? ProductImageUrl  { get; set; }
    public decimal UnitPrice        { get; set; }
    public int?    ProductVariantId { get; set; }
    public string? VariantInfo      { get; set; }
    public int     Quantity         { get; set; }
    public decimal TotalPrice       { get; set; }
    public string? CustomizationNote{ get; set; }
}
