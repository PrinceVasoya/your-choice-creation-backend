namespace EcommerceCA.Domain.Enums;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Refunded
}

public enum PaymentMethod
{
    Razorpay,
    CashOnDelivery
}
