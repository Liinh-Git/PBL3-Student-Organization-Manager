using FluentValidation;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Validators;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required")
            .Length(2, 100).WithMessage("Role name must be between 2 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PermissionKeys)
            .Must(keys => keys == null || keys.All(k => !string.IsNullOrWhiteSpace(k)))
            .WithMessage("Permission keys must not contain empty values")
            .When(x => x.PermissionKeys != null);
    }
}
