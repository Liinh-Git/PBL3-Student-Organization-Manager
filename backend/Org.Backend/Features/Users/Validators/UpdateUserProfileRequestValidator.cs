using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Validators;

/// <summary>
/// Validator for UpdateUserProfileRequest
/// </summary>
public class UpdateUserProfileRequestValidator : Validator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleFor(x => x.ProfileVisibility)
            .Must(v => string.IsNullOrWhiteSpace(v) || 
                      v == "Public" || v == "FriendsOnly" || v == "Private")
            .WithMessage("Profile visibility must be Public, FriendsOnly, or Private")
            .When(x => !string.IsNullOrWhiteSpace(x.ProfileVisibility));
    }
}
