namespace Org.Backend.Features.Departments;

internal static class DepartmentsFeatureTodos
{
    // TODO(BE-DAY1): GET /api/organizations/{orgId}/departments
    //  - Validate orgId exists and caller has access.
    //  - Return 200 with payload shape: { items: [...] }.
    //  - Include member count for each department.
    //
    // TODO(BE-DAY1): POST /api/departments
    //  - Validate unique department code inside organization.
    //  - Return 201 with created DepartmentDto.
    //  - Return 400 for invalid code/name; 404 if organization not found.
    //
    // TODO(BE-DAY1): PUT /api/departments/{id}
    //  - Validate department belongs to organization scope.
    //  - Return 404 when not found, 403 when forbidden.
    //  - Keep idempotent behavior for repeated same payload.
}
