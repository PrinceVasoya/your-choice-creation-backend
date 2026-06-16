namespace EcommerceCA.Domain.Entities;

public class Cart
{
    public int      Id        { get; set; }
    public string   UserId    { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser         User      { get; set; } = null!;
    public ICollection<CartItem>   CartItems { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int     Id                { get; set; }
    public int     CartId            { get; set; }
    public int     ProductId         { get; set; }
    public int?    ProductVariantId  { get; set; }
    public int     Quantity          { get; set; }
    public string? CustomizationNote { get; set; }
    public DateTime CreatedAt        { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt        { get; set; } = DateTime.UtcNow;

    public Cart            Cart           { get; set; } = null!;
    public Product         Product        { get; set; } = null!;
    public ProductVariant? ProductVariant { get; set; }
}
