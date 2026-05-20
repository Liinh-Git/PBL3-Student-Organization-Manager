using FluentValidation;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Validators;

public class AssignRoleToMemberRequestValidator : AbstractValidator<AssignRoleToMemberRequest>
{
    public AssignRoleToMemberRequestValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");
    }
}
