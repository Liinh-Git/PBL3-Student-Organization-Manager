using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth.Validators;

/// <summary>
/// Validator for LoginRequest
/// </summary>
public class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email is too long");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}
