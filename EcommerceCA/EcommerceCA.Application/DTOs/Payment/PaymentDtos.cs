namespace EcommerceCA.Application.DTOs.Payment;

// ── Requests ──────────────────────────────────────────────────────────────────
public class ConfirmPaymentDto
{
    public int     OrderId            { get; set; }
    public string  TransactionId      { get; set; } = string.Empty;
    public string? RazorpaySignature  { get; set; }
}




public class PaymentResponseDto
{
    public int     Id            { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount        { get; set; }
    public string  Currency      { get; set; } = string.Empty;
    public string  Status        { get; set; } = string.Empty;
    public string  Method        { get; set; } = string.Empty;
    public DateTime CreatedAt    { get; set; }
}
