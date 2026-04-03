namespace Org.Backend.Features.Tasks;

internal static class TasksFeatureTodos
{
    // TODO(BE-DAY1): POST /api/categories/{categoryId}/tasks
    //  - Validate category exists and categoryId matches body.CategoryId.
    //  - Validate assignee belongs to same organization.
    //  - Return 201 with created TaskDto.
    //
    // TODO(BE-DAY1): GET /api/categories/{categoryId}/tasks
    //  - Return 200 with { items: [...] }.
    //  - Support sorting by Priority then DueDate.
    //
    // TODO(BE-DAY1): PUT /api/tasks/{taskId}/status
    //  - Enforce status transition rules.
    //  - Return 404 when task not found.
    //
    // TODO(BE-DAY1): PUT /api/tasks/{taskId}/assign
    //  - Validate assignee exists and has member role in event organization.
    //  - Allow null to unassign.
}
