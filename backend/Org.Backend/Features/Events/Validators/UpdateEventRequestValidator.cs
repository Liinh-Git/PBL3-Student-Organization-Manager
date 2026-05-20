using FluentValidation;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Validators;

public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.EventName)
            .NotEmpty().WithMessage("Event name is required")
            .Length(2, 200).WithMessage("Event name must be between 2 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.BannerUrl)
            .MaximumLength(1000).WithMessage("Banner URL must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.BannerUrl));

        RuleFor(x => x.Visibility)
            .Must(v => string.IsNullOrEmpty(v) || v == "Public" || v == "OrganizationOnly" || v == "Private")
            .WithMessage("Visibility must be 'Public', 'OrganizationOnly', or 'Private'")
            .When(x => !string.IsNullOrEmpty(x.Visibility));
    }
}
