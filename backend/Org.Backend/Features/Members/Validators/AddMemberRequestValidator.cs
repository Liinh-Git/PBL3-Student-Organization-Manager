using FluentValidation;
using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members.Validators;

public class AddMemberRequestValidator : AbstractValidator<AddMemberRequest>
{
    public AddMemberRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.StudentCode)
            .MaximumLength(50).WithMessage("Student code must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.StudentCode));
    }
}
