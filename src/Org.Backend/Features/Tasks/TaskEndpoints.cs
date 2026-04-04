using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks;

public sealed class CreateTaskEndpoint(AppDbContext db) : Endpoint<CreateTaskRequest, TaskDto>
{
    public override void Configure()
    {
        Post("/api/categories/{categoryId:guid}/tasks");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateTaskRequest req, CancellationToken ct)
    {
        var categoryId = Route<Guid>("categoryId");
        if (categoryId != req.CategoryId)
            ThrowError("Route categoryId and body CategoryId must match.", StatusCodes.Status400BadRequest);

        var category = await db.EventCategories
            .AsNoTracking()
            .Include(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == categoryId, ct);

        if (category is null)
            ThrowError("Category not found.", StatusCodes.Status404NotFound);

        if (req.AssigneeMemberId is not null)
        {
            var assigneeValid = await db.Members
                .AsNoTracking()
                .AnyAsync(x => x.Id == req.AssigneeMemberId.Value && x.OrgId == category.Milestone.Event.OrgId, ct);

            if (!assigneeValid)
                ThrowError("Assignee is invalid for this category.", StatusCodes.Status400BadRequest);
        }

        var entity = new OrgTask
        {
            EventCategoryId = categoryId,
            TaskName = req.Title.Trim(),
            Note = req.Description?.Trim(),
            AssigneeId = req.AssigneeMemberId,
            Deadline = req.DueDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Priority = req.Priority,
            Status = Org.Shared.TaskStatus.Todo
        };

        db.Tasks.Add(entity);
        await db.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        await Send.OkAsync(ContractMapping.ToTaskDto(entity), ct);
    }
}

public sealed class GetTasksEndpoint(AppDbContext db) : EndpointWithoutRequest<GetTasksResponse>
{
    public override void Configure()
    {
        Get("/api/categories/{categoryId:guid}/tasks");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categoryId = Route<Guid>("categoryId");

        var categoryExists = await db.EventCategories.AsNoTracking().AnyAsync(x => x.Id == categoryId, ct);
        if (!categoryExists)
            ThrowError("Category not found.", StatusCodes.Status404NotFound);

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

public sealed class UpdateTaskStatusEndpoint(AppDbContext db) : Endpoint<UpdateTaskStatusRequest, TaskDto>
{
    public override void Configure()
    {
        Put("/api/tasks/{taskId:guid}/status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTaskStatusRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");

        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId, ct);
        if (task is null)
            ThrowError("Task not found.", StatusCodes.Status404NotFound);

        task!.Status = req.Status;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToTaskDto(task), ct);
    }
}

public sealed class UpdateTaskAssignEndpoint(AppDbContext db) : Endpoint<AssignTaskRequest, TaskDto>
{
    public override void Configure()
    {
        Put("/api/tasks/{taskId:guid}/assign");
        AllowAnonymous();
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

        if (req.AssigneeMemberId is not null)
        {
            var isValidAssignee = await db.Members
                .AnyAsync(x => x.Id == req.AssigneeMemberId.Value && x.OrgId == task!.EventCategory.Milestone.Event.OrgId, ct);

            if (!isValidAssignee)
                ThrowError("Assignee is invalid for this task.", StatusCodes.Status400BadRequest);
        }

        task!.AssigneeId = req.AssigneeMemberId;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToTaskDto(task), ct);
    }
}
