using EcommerceCA.Application.DTOs.User;
using FluentValidation;

namespace EcommerceCA.Application.Validators.User;

public class CreateAddressValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\d{10}$").WithMessage("Phone number must be exactly 10 digits.");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code/pincode is required.")
            .Matches(@"^\d{6}$").WithMessage("Postal code/pincode must be exactly 6 digits.");
    }
}
