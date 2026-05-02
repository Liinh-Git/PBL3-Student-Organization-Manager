// ---- Các endpoint CRUD cho module nhiệm vụ (OrgTask) trong hạng mục sự kiện ----
// Ghi chú: TaskName (entity) → Title (DTO), Note (entity) → Description (DTO)
// Luồng chuyển trạng thái: Todo → InProgress → Done (chỉ 1 chiều, không quay lại)
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Backend.Services;
using Org.Shared.Features.Tasks;
using System.Security.Claims;

namespace Org.Backend.Features.Tasks;

// ---- POST /api/categories/{categoryId}/tasks — tạo nhiệm vụ mới ----
public sealed class CreateTaskEndpoint(AppDbContext db) : Endpoint<CreateTaskRequest, TaskDto>
{
    public override void Configure()
    {
        Post("/api/categories/{categoryId:guid}/tasks");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateTaskRequest req, CancellationToken ct)
    {
        var categoryId = Route<Guid>("categoryId");
        if (categoryId != req.CategoryId)
            ThrowError("Route categoryId and body CategoryId must match.", StatusCodes.Status400BadRequest);

        var normalizedTitle = TaskValidation.NormalizeTitle(req.Title);
        if (normalizedTitle is null)
            ThrowError("Task title must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var category = await db.EventCategories
            .AsNoTracking()
            .Include(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == categoryId, ct);

        if (category is null)
            ThrowError("Category not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, category!.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (req.DueDate is not null)
        {
            var milestoneStart = DateOnly.FromDateTime(category.Milestone.StartDate);
            var milestoneEnd = DateOnly.FromDateTime(category.Milestone.EndDate);
            if (req.DueDate.Value < milestoneStart || req.DueDate.Value > milestoneEnd)
                ThrowError("Task due date must be within milestone date range.", StatusCodes.Status400BadRequest);
        }

        Guid? departmentId = category.OwnerDepartmentId;

        if (req.AssigneeMemberId is not null)
        {
            var assignee = await db.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.AssigneeMemberId.Value && x.OrgId == category.Milestone.Event.OrgId, ct);

            if (assignee is null)
                ThrowError("Assignee is invalid for this category.", StatusCodes.Status400BadRequest);

            departmentId = assignee.DepartmentId;
        }

        var entity = new OrgTask
        {
            EventCategoryId = categoryId,
            TaskName = normalizedTitle,
            Note = TaskValidation.NormalizeOptional(req.Description),
            AssigneeId = req.AssigneeMemberId,
            DeptId = departmentId,
            Deadline = req.DueDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Priority = req.Priority,
            Status = Org.Shared.TaskStatus.Todo
        };

        db.Tasks.Add(entity);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(ContractMapping.ToTaskDto(entity), StatusCodes.Status201Created, cancellation: ct);
    }
}

// ---- GET /api/categories/{categoryId}/tasks — danh sách task trong category ----
public sealed class GetTasksEndpoint(AppDbContext db) : EndpointWithoutRequest<GetTasksResponse>
{
    public override void Configure()
    {
        Get("/api/categories/{categoryId:guid}/tasks");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categoryId = Route<Guid>("categoryId");

        var category = await db.EventCategories
            .AsNoTracking()
            .Include(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == categoryId, ct);

        if (category is null)
            ThrowError("Category not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, category!.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var items = await db.Tasks
            .AsNoTracking()
            .Where(x => x.EventCategoryId == categoryId)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Deadline)
            .Select(x => ContractMapping.ToTaskDto(x))
            .ToListAsync(ct);

        await Send.OkAsync(new GetTasksResponse(items), ct);
    }
}

// ---- GET /api/tasks/{taskId} — chi tiết một nhiệm vụ ----
public sealed class GetTaskByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetTaskByIdResponse>
{
    public override void Configure()
    {
        Get("/api/tasks/{taskId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");

        var task = await db.Tasks
            .AsNoTracking()
            .Include(x => x.EventCategory)
            .ThenInclude(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == taskId, ct);

        if (task is null)
            ThrowError("Task not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, task!.EventCategory.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await Send.OkAsync(new GetTaskByIdResponse(ContractMapping.ToTaskDto(task!)), ct);
    }
}

// ---- PUT /api/tasks/{taskId} — cập nhật thông tin nhiệm vụ (yêu cầu Manager+) ----
public sealed class UpdateTaskEndpoint(AppDbContext db) : Endpoint<UpdateTaskRequest, TaskDto>
{
    public override void Configure()
    {
        Put("/api/tasks/{taskId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");

        var normalizedTitle = TaskValidation.NormalizeTitle(req.Title);
        if (normalizedTitle is null)
            ThrowError("Task title must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var task = await db.Tasks
            .Include(x => x.EventCategory)
            .ThenInclude(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == taskId, ct);

        if (task is null)
            ThrowError("Task not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, task!.EventCategory.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (req.DueDate is not null)
        {
            var milestoneStart = DateOnly.FromDateTime(task.EventCategory.Milestone.StartDate);
            var milestoneEnd = DateOnly.FromDateTime(task.EventCategory.Milestone.EndDate);
            if (req.DueDate.Value < milestoneStart || req.DueDate.Value > milestoneEnd)
                ThrowError("Task due date must be within milestone date range.", StatusCodes.Status400BadRequest);
        }

        Guid? departmentId = task.EventCategory.OwnerDepartmentId;
        if (req.AssigneeMemberId is not null)
        {
            var assignee = await db.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.AssigneeMemberId.Value && x.OrgId == task.EventCategory.Milestone.Event.OrgId, ct);

            if (assignee is null)
                ThrowError("Assignee is invalid for this task.", StatusCodes.Status400BadRequest);

            departmentId = assignee.DepartmentId;
        }

        task.TaskName = normalizedTitle;
        task.Note = TaskValidation.NormalizeOptional(req.Description);
        task.AssigneeId = req.AssigneeMemberId;
        task.Deadline = req.DueDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        task.Priority = req.Priority;
        task.DeptId = departmentId;

        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToTaskDto(task), ct);
    }
}

// ---- PUT /api/tasks/{taskId}/status — chuyển trạng thái (assignee cũng có thể cập nhật) ----
public sealed class UpdateTaskStatusEndpoint(AppDbContext db, INotificationService notificationService) : Endpoint<UpdateTaskStatusRequest, TaskDto>
{
    public override void Configure()
    {
        Put("/api/tasks/{taskId:guid}/status");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateTaskStatusRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");

        var task = await db.Tasks
            .Include(x => x.EventCategory)
            .ThenInclude(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .Include(x => x.Assignee)
            .FirstOrDefaultAsync(x => x.Id == taskId, ct);

        if (task is null)
            ThrowError("Task not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, task!.EventCategory.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (!OrganizationAuthorization.CanPlan(callerContext.Value.Role) && task!.AssigneeId != callerContext.Value.MemberId)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (!TaskValidation.CanTransition(task!.Status, req.Status))
            ThrowError($"Invalid status transition: {task.Status} -> {req.Status}.", StatusCodes.Status400BadRequest);

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var changerId = Guid.TryParse(userIdText, out var uid) ? uid : (Guid?)null;
        if (changerId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        task!.Status = req.Status;
        await db.SaveChangesAsync(ct);

        // Notify assignee about task status change (if assignee exists and is not the changer)
        if (task.AssigneeId is not null && task.Assignee?.UserId is not null && task.Assignee.UserId != changerId.Value)
        {
            try
            {
                await notificationService.NotifyTaskStatusChanged(
                    task.Assignee.UserId, 
                    changerId.Value, 
                    taskId, 
                    req.Status.ToString());
            }
            catch (Exception ex)
            {
                // Log error but don't throw - notification failure should not block business logic
                Console.WriteLine($"Failed to send task status change notification: {ex.Message}");
            }
        }

        await Send.OkAsync(ContractMapping.ToTaskDto(task), ct);
    }
}

// ---- PUT /api/tasks/{taskId}/assign — gán thành viên cho nhiệm vụ (yêu cầu Manager+) ----
public sealed class UpdateTaskAssignEndpoint(AppDbContext db, INotificationService notificationService) : Endpoint<AssignTaskRequest, TaskDto>
{
    public override void Configure()
    {
        Put("/api/tasks/{taskId:guid}/assign");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AssignTaskRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");

        var task = await db.Tasks
            .Include(x => x.EventCategory)
            .ThenInclude(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == taskId, ct);

        if (task is null)
            ThrowError("Task not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, task!.EventCategory.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var assignerId = Guid.TryParse(userIdText, out var uid) ? uid : (Guid?)null;
        if (assignerId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        Guid? assigneeUserId = null;
        if (req.AssigneeMemberId is not null)
        {
            var assignee = await db.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.AssigneeMemberId.Value && x.OrgId == task!.EventCategory.Milestone.Event.OrgId, ct);

            if (assignee is null)
                ThrowError("Assignee is invalid for this task.", StatusCodes.Status400BadRequest);

            task!.DeptId = assignee.DepartmentId;
            assigneeUserId = assignee.UserId;
        }
        else
        {
            task!.DeptId = task.EventCategory.OwnerDepartmentId;
        }

        task!.AssigneeId = req.AssigneeMemberId;
        await db.SaveChangesAsync(ct);

        // Notify assignee about task assignment
        if (assigneeUserId is not null)
        {
            try
            {
                await notificationService.NotifyTaskAssigned(assigneeUserId.Value, assignerId.Value, taskId);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - notification failure should not block business logic
                Console.WriteLine($"Failed to send task assignment notification: {ex.Message}");
            }
        }

        await Send.OkAsync(ContractMapping.ToTaskDto(task), ct);
    }
}

// ---- DELETE /api/tasks/{taskId} — xóa mềm nhiệm vụ ----
public sealed class DeleteTaskEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/tasks/{taskId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");

        var task = await db.Tasks
            .Include(x => x.EventCategory)
            .ThenInclude(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == taskId, ct);

        if (task is null)
            ThrowError("Task not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, task!.EventCategory.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        task!.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

// ---- POST /api/tasks/{taskId}/restore — khôi phục task (chức năng cha không được xóa) ----
public sealed class RestoreTaskEndpoint(AppDbContext db) : EndpointWithoutRequest<TaskDto>
{
    public override void Configure()
    {
        Post("/api/tasks/{taskId:guid}/restore");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");

        var task = await db.Tasks
            .IgnoreQueryFilters()
            .Include(x => x.EventCategory)
            .ThenInclude(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == taskId, ct);

        if (task is null)
            ThrowError("Task not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, task!.EventCategory.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (task.EventCategory.IsDeleted || task.EventCategory.Milestone.IsDeleted || task.EventCategory.Milestone.Event.IsDeleted)
            ThrowError("Cannot restore task while parent entities are deleted.", StatusCodes.Status409Conflict);

        task.IsDeleted = false;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToTaskDto(task), ct);
    }
}

// ---- Helper validate tên và luồng chuyển trạng thái nhiệm vụ ----
internal static class TaskValidation
{
    public static string? NormalizeTitle(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        return normalized.Length >= 2 ? normalized : null;
    }

    public static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static bool CanTransition(Org.Shared.TaskStatus current, Org.Shared.TaskStatus next)
    {
        if (current == next)
            return true;

        return current switch
        {
            Org.Shared.TaskStatus.Todo => next == Org.Shared.TaskStatus.InProgress,
            Org.Shared.TaskStatus.InProgress => next == Org.Shared.TaskStatus.Done,
            Org.Shared.TaskStatus.Done => false,
            _ => false
        };
    }
}
