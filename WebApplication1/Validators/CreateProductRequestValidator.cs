using FluentValidation;
using WebApplication1.Controllers.N8n;

namespace WebApplication1.Validators;
/// <summary>
/// Create CreateProductRequest Fluent Validator
/// https://docs.fluentvalidation.net/en/latest/di.html
/// </summary>
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.ProductTitle)
            .NotEmpty().WithMessage("Product Title is required")
            .MinimumLength(3).WithMessage("Product Title must more than 3 characters")
            .MaximumLength(100).WithMessage("Product Title cannot exceed 100 characters");
    }
}