// ---- Các endpoint CRUD cho module hạng mục sự kiện (EventCategory) trong milestone ----
// Hạng mục là nhóm công việc (ví dụ: Hậu cần, Kỹ thuật) phân cấp trong Milestone
// Cascade xóa mềm Task con khi xóa Category
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories;

// ---- POST /api/milestones/{milestoneId}/categories — tạo hạng mục mới ----
public sealed class CreateEventCategoryEndpoint(AppDbContext db) : Endpoint<CreateEventCategoryRequest, EventCategoryDto>
{
    public override void Configure()
    {
        Post("/api/milestones/{milestoneId:guid}/categories");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateEventCategoryRequest req, CancellationToken ct)
    {
        var milestoneId = Route<Guid>("milestoneId");
        if (milestoneId != req.MilestoneId)
            ThrowError("Route milestoneId and body MilestoneId must match.", StatusCodes.Status400BadRequest);

        var normalizedName = EventCategoryValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Category name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var milestone = await db.Milestones
            .AsNoTracking()
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == milestoneId, ct);

        if (milestone is null)
            ThrowError("Milestone not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, milestone!.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var entity = new EventCategory
        {
            MilestoneId = milestoneId,
            CategoryName = normalizedName,
            Description = EventCategoryValidation.NormalizeOptional(req.Description),
            OrderIndex = req.SortOrder
        };

        db.EventCategories.Add(entity);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(ContractMapping.ToCategoryDto(entity, 0, 0), StatusCodes.Status201Created, cancellation: ct);
    }
}

// ---- GET /api/milestones/{milestoneId}/categories — danh sách hạng mục kèm số task ----
public sealed class GetEventCategoriesEndpoint(AppDbContext db) : EndpointWithoutRequest<GetEventCategoriesResponse>
{
    public override void Configure()
    {
        Get("/api/milestones/{milestoneId:guid}/categories");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var milestoneId = Route<Guid>("milestoneId");

        var milestone = await db.Milestones
            .AsNoTracking()
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == milestoneId, ct);

        if (milestone is null)
            ThrowError("Milestone not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, milestone!.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var items = await db.EventCategories
            .AsNoTracking()
            .Include(x => x.OwnerDepartment)
            .ThenInclude(x => x!.Manager)
            .ThenInclude(x => x!.User)
            .Where(x => x.MilestoneId == milestoneId)
            .OrderBy(x => x.OrderIndex)
            .Select(x => ContractMapping.ToCategoryDto(
                x,
                x.Tasks.Count,
                x.Tasks.Count(t => t.Status == Org.Shared.TaskStatus.Done)))
            .ToListAsync(ct);

        await Send.OkAsync(new GetEventCategoriesResponse(items), ct);
    }
}

// ---- GET /api/categories/{id} — chi tiết hạng mục kèm số task/done ----
public sealed class GetEventCategoryByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetEventCategoryByIdResponse>
{
    public override void Configure()
    {
        Get("/api/categories/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var category = await db.EventCategories
            .AsNoTracking()
            .Include(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .Include(x => x.OwnerDepartment)
            .ThenInclude(x => x!.Manager)
            .ThenInclude(x => x!.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (category is null)
            ThrowError("Category not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, category!.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var taskCount = await db.Tasks.CountAsync(x => x.EventCategoryId == id, ct);
        var completedTaskCount = await db.Tasks.CountAsync(x => x.EventCategoryId == id && x.Status == Org.Shared.TaskStatus.Done, ct);

        await Send.OkAsync(new GetEventCategoryByIdResponse(ContractMapping.ToCategoryDto(category!, taskCount, completedTaskCount)), ct);
    }
}

// ---- PUT /api/categories/{id} — cập nhật tên và thứ tự hiển thị ----
public sealed class UpdateEventCategoryEndpoint(AppDbContext db) : Endpoint<UpdateEventCategoryRequest, EventCategoryDto>
{
    public override void Configure()
    {
        Put("/api/categories/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateEventCategoryRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var normalizedName = EventCategoryValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Category name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var category = await db.EventCategories
            .Include(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .Include(x => x.OwnerDepartment)
            .ThenInclude(x => x!.Manager)
            .ThenInclude(x => x!.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (category is null)
            ThrowError("Category not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, category!.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        category!.CategoryName = normalizedName;
        category.Description = EventCategoryValidation.NormalizeOptional(req.Description);
        category.OrderIndex = req.SortOrder;

        await db.SaveChangesAsync(ct);

        var taskCount = await db.Tasks.CountAsync(x => x.EventCategoryId == id, ct);
        var completedTaskCount = await db.Tasks.CountAsync(x => x.EventCategoryId == id && x.Status == Org.Shared.TaskStatus.Done, ct);
        await Send.OkAsync(ContractMapping.ToCategoryDto(category, taskCount, completedTaskCount), ct);
    }
}

// ---- DELETE /api/categories/{id} — xóa mềm hạng mục và task con ----
public sealed class DeleteEventCategoryEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/categories/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var category = await db.EventCategories
            .Include(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (category is null)
            ThrowError("Category not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, category!.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var tasks = await db.Tasks
            .IgnoreQueryFilters()
            .Where(x => x.EventCategoryId == id)
            .ToListAsync(ct);

        foreach (var task in tasks)
            task.IsDeleted = true;

        category!.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

// ---- POST /api/categories/{id}/restore — khôi phục hạng mục (milestone/event cha phải còn) ----
public sealed class RestoreEventCategoryEndpoint(AppDbContext db) : EndpointWithoutRequest<EventCategoryDto>
{
    public override void Configure()
    {
        Post("/api/categories/{id:guid}/restore");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var category = await db.EventCategories
            .IgnoreQueryFilters()
            .Include(x => x.Milestone)
            .ThenInclude(x => x.Event)
            .Include(x => x.OwnerDepartment)
            .ThenInclude(x => x!.Manager)
            .ThenInclude(x => x!.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (category is null)
            ThrowError("Category not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, category!.Milestone.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (category.Milestone.IsDeleted || category.Milestone.Event.IsDeleted)
            ThrowError("Cannot restore category while parent milestone/event is deleted.", StatusCodes.Status409Conflict);

        var tasks = await db.Tasks
            .IgnoreQueryFilters()
            .Where(x => x.EventCategoryId == id)
            .ToListAsync(ct);

        foreach (var task in tasks)
            task.IsDeleted = false;

        category.IsDeleted = false;
        await db.SaveChangesAsync(ct);

        var taskCount = await db.Tasks.CountAsync(x => x.EventCategoryId == id, ct);
        var completedTaskCount = await db.Tasks.CountAsync(x => x.EventCategoryId == id && x.Status == Org.Shared.TaskStatus.Done, ct);
        await Send.OkAsync(ContractMapping.ToCategoryDto(category, taskCount, completedTaskCount), ct);
    }
}

// ---- Helper validate các tham số đầu vào cho module event category ----
internal static class EventCategoryValidation
{
    public static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        return normalized.Length >= 2 ? normalized : null;
    }

    public static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
