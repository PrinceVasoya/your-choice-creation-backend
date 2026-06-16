using EcommerceCA.Domain.Enums;

namespace EcommerceCA.Domain.Entities;

public class Order
{
    public int          Id                { get; set; }
    public string       OrderNumber       { get; set; } = string.Empty;
    public string       UserId            { get; set; } = string.Empty;
    public int          ShippingAddressId { get; set; }
    public decimal      SubTotal          { get; set; }
    public decimal      ShippingCost      { get; set; }
    public decimal      TaxAmount         { get; set; }
    public decimal      TotalAmount       { get; set; }
    public decimal      GrandTotal        { get; set; }
    public OrderStatus  Status            { get; set; } = OrderStatus.Pending;
    public string?      Notes             { get; set; }
    public string?      TrackingUrl       { get; set; }
    public string?      RazorpayOrderId   { get; set; }
    public string?      RazorpayPaymentId { get; set; }
    public string?      RazorpaySignature { get; set; }
    public DateTime     CreatedAt         { get; set; } = DateTime.UtcNow;
    public DateTime     UpdatedAt         { get; set; } = DateTime.UtcNow;

    public ApplicationUser         User            { get; set; } = null!;
    public Address                 ShippingAddress { get; set; } = null!;
    public ICollection<OrderItem>  OrderItems      { get; set; } = new List<OrderItem>();
    public Payment?                Payment         { get; set; }
}

public class OrderItem
{
    public int     Id               { get; set; }
    public int     OrderId          { get; set; }
    public int     ProductId        { get; set; }
    public int?    ProductVariantId { get; set; }
    public int     Quantity         { get; set; }
    public decimal UnitPrice        { get; set; }
    public decimal TotalPrice       { get; set; }
    public string? ProductName      { get; set; }
    public string? VariantInfo      { get; set; }
    public string? CustomizationNote{ get; set; }

    public Order          Order          { get; set; } = null!;
    public Product        Product        { get; set; } = null!;
    public ProductVariant? ProductVariant{ get; set; }
}
