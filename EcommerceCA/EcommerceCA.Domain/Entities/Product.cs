namespace EcommerceCA.Domain.Entities;

public class Product
{
    public int      Id             { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public string   ProductCode    { get; set; } = string.Empty;
    public string?  Description    { get; set; }
    public decimal  Price          { get; set; }
    public decimal? DiscountPrice  { get; set; }
    public string?  ImageUrl       { get; set; }
    public string?  ImagePublicId  { get; set; }
    public int      Stock          { get; set; }
    public bool     IsActive       { get; set; } = true;
    public bool     IsCustomizable { get; set; } = false;
    public int      CategoryId     { get; set; }
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt      { get; set; } = DateTime.UtcNow;

    public Category                      Category  { get; set; } = null!;
    public ICollection<ProductVariant>   Variants  { get; set; } = new List<ProductVariant>();
    public ICollection<CartItem>         CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem>        OrderItems{ get; set; } = new List<OrderItem>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}

public class ProductVariant
{
    public int      Id              { get; set; }
    public int      ProductId       { get; set; }
    public string?  Size            { get; set; }
    public string?  Color           { get; set; }
    public string?  ColorHex        { get; set; }
    public decimal? PriceAdjustment { get; set; } = 0;
    public int      Stock           { get; set; }
    public string?  SKU             { get; set; }
    public bool     IsActive        { get; set; } = true;
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}
