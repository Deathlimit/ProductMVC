using FluentValidation;
using ProductMVC.BLL.DTO;

namespace ProductMVC.BLL.Validation;

public class ProductDtoValidator : AbstractValidator<ProductDTO>
{
    public ProductDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100).WithMessage("Product name must not be more than 100 chars");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Product price must be greater than 0");

        RuleFor(p => p.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Product quantity cannot be negative");

        RuleFor(p => p.Category)
            .MaximumLength(50).WithMessage("Category must not be more than 50 chars")
            .When(p => !string.IsNullOrEmpty(p.Category));

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("Description must not be more than 500 chars")
            .When(p => !string.IsNullOrEmpty(p.Description));
    }
}
