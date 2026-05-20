using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Validators;

public class ReviewRequestRequestValidator : Validator<ReviewRequestRequest>
{
    public ReviewRequestRequestValidator()
    {
        RuleFor(x => x.Decision)
            .NotEmpty().WithMessage("Decision is required")
            .Must(BeValidDecision).WithMessage("Decision must be 'Approved' or 'Rejected'");

        RuleFor(x => x.ReviewNote)
            .MaximumLength(1000).WithMessage("Review note must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ReviewNote));
    }

    private bool BeValidDecision(string decision)
    {
        return decision == "Approved" || decision == "Rejected";
    }
}
