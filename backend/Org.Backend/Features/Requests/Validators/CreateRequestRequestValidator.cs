using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Validators;

public class CreateRequestRequestValidator : Validator<CreateRequestRequest>
{
    public CreateRequestRequestValidator()
    {
        RuleFor(x => x.RequestType)
            .NotEmpty().WithMessage("Request type is required")
            .Must(BeValidRequestType).WithMessage("Invalid request type. Must be one of: JoinOrganization, DepartmentChange, RoleChange, EventParticipation, Other");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(2000).WithMessage("Content must not exceed 2000 characters");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.DesiredPosition)
            .MaximumLength(100).WithMessage("Desired position must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.DesiredPosition));
    }

    private bool BeValidRequestType(string requestType)
    {
        var validTypes = new[] { "JoinOrganization", "DepartmentChange", "RoleChange", "EventParticipation", "Other" };
        return validTypes.Contains(requestType);
    }
}
