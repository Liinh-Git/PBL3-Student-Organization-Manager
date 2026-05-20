using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones.Validators;

public class UpdateMilestoneRequestValidator : Validator<UpdateMilestoneRequest>
{
    public UpdateMilestoneRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(2).WithMessage("Title must be at least 2 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => new[] { "Planned", "InProgress", "Completed", "Archived" }.Contains(s))
            .WithMessage("Status must be one of: Planned, InProgress, Completed, Archived");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be non-negative")
            .When(x => x.OrderIndex.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
