using EcommerceCA.Application.DTOs.Payment;
using FluentValidation;

namespace EcommerceCA.Application.Validators.Payment;

public class ConfirmPaymentValidator : AbstractValidator<ConfirmPaymentDto>
{
    public ConfirmPaymentValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("A valid order ID is required.");
        RuleFor(x => x.TransactionId).NotEmpty().WithMessage("Transaction ID is required.");
    }
}
