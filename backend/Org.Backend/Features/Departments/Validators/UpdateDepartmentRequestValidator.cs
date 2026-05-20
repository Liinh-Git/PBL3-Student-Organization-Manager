using FluentValidation;
using Org.Shared.Features.Departments;

namespace Org.Backend.Features.Departments.Validators;

public class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.DepartmentName)
            .NotEmpty().WithMessage("Department name is required")
            .Length(2, 100).WithMessage("Department name must be between 2 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ManagerId)
            .NotEmpty().WithMessage("Manager ID must not be empty")
            .When(x => x.ManagerId.HasValue);
    }
}
