using EcommerceCA.Application.DTOs.Product;
using FluentValidation;

namespace EcommerceCA.Application.Validators.Product;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
        RuleFor(x => x.DiscountPrice)
            .LessThan(x => x.Price).WithMessage("Discount price must be less than the regular price.")
            .When(x => x.DiscountPrice.HasValue);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("A valid category is required.");
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name != null);
        RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).When(x => x.Stock.HasValue);
    }
}

public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantDto>
{
    public CreateProductVariantValidator()
    {
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Size).MaximumLength(50).When(x => x.Size != null);
        RuleFor(x => x.Color).MaximumLength(100).When(x => x.Color != null);
        RuleFor(x => x.SKU).MaximumLength(100).When(x => x.SKU != null);
    }
}
