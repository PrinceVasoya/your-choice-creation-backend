using EcommerceCA.Domain.Enums;

namespace EcommerceCA.Application.DTOs.Order;

// ── Requests ──────────────────────────────────────────────────────────────────
public class PlaceOrderDto
{
    public int           ShippingAddressId { get; set; }
    public string?       Notes            { get; set; }
    public PaymentMethod PaymentMethod    { get; set; }
}

public class UpdateOrderStatusDto
{
    public string  Status      { get; set; } = string.Empty;
    public string? TrackingUrl { get; set; }
}

// ── Responses ─────────────────────────────────────────────────────────────────
public class OrderResponseDto
{
    public int     Id          { get; set; }
    public string  OrderNumber { get; set; } = string.Empty;
    public decimal SubTotal    { get; set; }
    public decimal ShippingCost{ get; set; }
    public decimal TaxAmount   { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal GrandTotal  { get; set; }
    public string  Status      { get; set; } = string.Empty;
    public string? Notes       { get; set; }
    public string? TrackingUrl { get; set; }
    public string? RazorpayOrderId   { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySignature { get; set; }
    public DateTime CreatedAt  { get; set; }
    public AddressInOrderDto?          ShippingAddress { get; set; }
    public List<OrderItemResponseDto>  Items           { get; set; } = new();
    public OrderPaymentResponseDto?    Payment         { get; set; }
}

public class OrderItemResponseDto
{
    public int     Id                { get; set; }
    public int     ProductId         { get; set; }
    public string  ProductName       { get; set; } = string.Empty;
    public string? ProductImageUrl   { get; set; }
    public string? VariantInfo       { get; set; }
    public int     Quantity          { get; set; }
    public decimal UnitPrice         { get; set; }
    public decimal TotalPrice        { get; set; }
    public string? CustomizationNote { get; set; }
}

public class AddressInOrderDto
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
}

public class OrderPaymentResponseDto
{
    public int     Id            { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount        { get; set; }
    public string  Currency      { get; set; } = string.Empty;
    public string  Status        { get; set; } = string.Empty;
    public string  Method        { get; set; } = string.Empty;
    public DateTime CreatedAt    { get; set; }
}

public class PaginatedOrderResult
{
    public List<OrderResponseDto> Items      { get; set; } = new();
    public int                    TotalCount { get; set; }
    public int                    Page       { get; set; }
    public int                    PageSize   { get; set; }
    public int  TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext     => Page < TotalPages;
}
