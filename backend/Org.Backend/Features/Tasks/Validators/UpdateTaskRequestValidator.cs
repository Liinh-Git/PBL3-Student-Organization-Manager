using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Validators;

public class UpdateTaskRequestValidator : Validator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("TaskName is required")
            .MinimumLength(2).WithMessage("TaskName must be at least 2 characters")
            .MaximumLength(200).WithMessage("TaskName must not exceed 200 characters");

        RuleFor(x => x.Priority)
            .Must(p => p == null || new[] { "Low", "Medium", "High", "Urgent" }.Contains(p))
            .WithMessage("Priority must be one of: Low, Medium, High, Urgent");

        RuleFor(x => x.Status)
            .Must(s => s == null || new[] { "Todo", "InProgress", "Blocked", "Done", "Cancelled" }.Contains(s))
            .WithMessage("Status must be one of: Todo, InProgress, Blocked, Done, Cancelled");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be non-negative")
            .When(x => x.OrderIndex.HasValue);
    }
}
