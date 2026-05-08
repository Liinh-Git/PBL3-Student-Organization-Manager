using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Validators;

public class AssignTaskRequestValidator : Validator<AssignTaskRequest>
{
    public AssignTaskRequestValidator()
    {
        // At least one of AssigneeId or DeptId should be provided (optional validation)
        // Both can be null to unassign
        // Both can be provided (will be validated in service layer for same org)
    }
}
