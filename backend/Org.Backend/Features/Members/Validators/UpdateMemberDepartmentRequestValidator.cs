using FluentValidation;
using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members.Validators;

public class UpdateMemberDepartmentRequestValidator : AbstractValidator<UpdateMemberDepartmentRequest>
{
    public UpdateMemberDepartmentRequestValidator()
    {
        // DepartmentId can be null (to clear department) or a valid Guid
        // No specific validation needed beyond type checking
    }
}
