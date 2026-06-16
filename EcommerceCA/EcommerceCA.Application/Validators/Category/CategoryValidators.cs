using EcommerceCA.Application.DTOs.Category;
using FluentValidation;

namespace EcommerceCA.Application.Validators.Category;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Category name is required (max 150 chars).");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Image)
            .Must(f => f == null || f.Length <= 5 * 1024 * 1024).WithMessage("Image must be under 5 MB.")
            .Must(f => f == null || new[] { "image/jpeg", "image/png", "image/webp" }.Contains(f.ContentType))
            .WithMessage("Only JPEG, PNG, or WebP images are allowed.");
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name).MaximumLength(150).When(x => x.Name != null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}
