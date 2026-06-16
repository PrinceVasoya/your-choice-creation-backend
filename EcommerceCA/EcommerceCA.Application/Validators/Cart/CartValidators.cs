using EcommerceCA.Application.DTOs.Cart;
using FluentValidation;

namespace EcommerceCA.Application.Validators.Cart;

public class AddToCartValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("A valid product ID is required.");
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
    }
}

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemDto>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
    }
}
