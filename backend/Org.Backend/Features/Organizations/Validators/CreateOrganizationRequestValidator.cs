using FluentValidation;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Validators;

public class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.OrgName)
            .NotEmpty().WithMessage("Organization name is required")
            .Length(2, 200).WithMessage("Organization name must be between 2 and 200 characters");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Contact phone must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Location));
    }
}
