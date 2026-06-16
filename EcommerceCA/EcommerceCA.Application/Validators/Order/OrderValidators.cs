using EcommerceCA.Application.DTOs.Order;
using FluentValidation;

namespace EcommerceCA.Application.Validators.Order;

public class PlaceOrderValidator : AbstractValidator<PlaceOrderDto>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.ShippingAddressId).GreaterThan(0).WithMessage("A valid shipping address is required.");
        RuleFor(x => x.PaymentMethod).IsInEnum().WithMessage("Invalid payment method.");
    }
}

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDto>
{
    private static readonly string[] ValidStatuses =
        { "Pending", "Confirmed", "Paid", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded" };

    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }
}
