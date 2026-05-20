using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories.Validators;

public class UpdateEventCategoryRequestValidator : Validator<UpdateEventCategoryRequest>
{
    public UpdateEventCategoryRequestValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("CategoryName is required")
            .MinimumLength(2).WithMessage("CategoryName must be at least 2 characters")
            .MaximumLength(200).WithMessage("CategoryName must not exceed 200 characters");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be non-negative")
            .When(x => x.OrderIndex.HasValue);
    }
}
