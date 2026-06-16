using EcommerceCA.Domain.Enums;

namespace EcommerceCA.Domain.Entities;

public class Payment
{
    public int           Id               { get; set; }
    public int           OrderId          { get; set; }
    public string?       TransactionId    { get; set; }
    public string?       PaymentIntentId  { get; set; }
    public string?       RazorpayOrderId  { get; set; }
    public decimal       Amount           { get; set; }
    public string        Currency         { get; set; } = "INR";
    public PaymentStatus Status           { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method           { get; set; }
    public string?       FailureReason    { get; set; }
    public DateTime      CreatedAt        { get; set; } = DateTime.UtcNow;
    public DateTime      UpdatedAt        { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
}
