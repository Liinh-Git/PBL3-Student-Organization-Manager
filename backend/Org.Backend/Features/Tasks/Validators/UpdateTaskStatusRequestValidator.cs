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
            .Must(s => new[] { "Todo", "InProgress", "Done" }.Contains(s))
            .WithMessage("Status must be one of: Todo, InProgress, Done");
    }
}
