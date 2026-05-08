using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Validators;

public class UpdateTaskStatusRequestValidator : Validator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => new[] { "Todo", "InProgress", "Blocked", "Done", "Cancelled" }.Contains(s))
            .WithMessage("Status must be one of: Todo, InProgress, Blocked, Done, Cancelled");
    }
}
