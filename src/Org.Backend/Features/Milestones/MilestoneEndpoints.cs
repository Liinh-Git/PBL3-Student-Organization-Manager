// ---- Các endpoint CRUD cho module cột mốc (Milestone) trong sự kiện ----
// Milestone phải nằm trong khoảng ngày của Event cha, Cascade xóa mềm Category + Task con
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones;

// ---- POST /api/events/{eventId}/milestones — tạo cột mốc mới ----
public sealed class CreateMilestoneEndpoint(AppDbContext db) : Endpoint<CreateMilestoneRequest, MilestoneDto>
{
    public override void Configure()
    {
        Post("/api/events/{eventId:guid}/milestones");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateMilestoneRequest req, CancellationToken ct)
    {
        var eventId = Route<Guid>("eventId");

        if (eventId != req.EventId)
            ThrowError("Route eventId and body EventId must match.", StatusCodes.Status400BadRequest);

        var normalizedName = MilestoneValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Milestone name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        if (req.EndDate < req.StartDate)
            ThrowError("EndDate must be greater than or equal to StartDate.", StatusCodes.Status400BadRequest);

        var @event = await db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId, ct);
        if (@event is null)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, @event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var eventStart = DateOnly.FromDateTime(@event.StartDate);
        var eventEnd = DateOnly.FromDateTime(@event.EndDate);
        if (req.StartDate < eventStart || req.EndDate > eventEnd)
            ThrowError("Milestone dates must be within event date range.", StatusCodes.Status400BadRequest);

        var milestone = new Milestone
        {
            EventId = eventId,
            Title = normalizedName,
            Description = MilestoneValidation.NormalizeOptional(req.Description),
            StartDate = req.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = req.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            OrderIndex = req.SortOrder,
            Status = MilestoneStatus.NotStarted
        };

        db.Milestones.Add(milestone);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(ContractMapping.ToMilestoneDto(milestone), StatusCodes.Status201Created, cancellation: ct);
    }
}

// ---- GET /api/events/{eventId}/milestones — danh sách cột mốc sắp theo OrderIndex ----
public sealed class GetMilestonesEndpoint(AppDbContext db) : EndpointWithoutRequest<GetMilestonesResponse>
{
    public override void Configure()
    {
        Get("/api/events/{eventId:guid}/milestones");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var eventId = Route<Guid>("eventId");

        var @event = await db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId, ct);
        if (@event is null)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, @event!.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var items = await db.Milestones
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.OrderIndex)
            .Select(x => ContractMapping.ToMilestoneDto(x))
            .ToListAsync(ct);

        await Send.OkAsync(new GetMilestonesResponse(items), ct);
    }
}

// ---- GET /api/milestones/{id} — chi tiết một cột mốc ----
public sealed class GetMilestoneByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetMilestoneByIdResponse>
{
    public override void Configure()
    {
        Get("/api/milestones/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var milestone = await db.Milestones
            .AsNoTracking()
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (milestone is null)
            ThrowError("Milestone not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, milestone!.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await Send.OkAsync(new GetMilestoneByIdResponse(ContractMapping.ToMilestoneDto(milestone!)), ct);
    }
}

// ---- PUT /api/milestones/{id} — cập nhật thông tin và trạng thái cột mốc ----
public sealed class UpdateMilestoneEndpoint(AppDbContext db) : Endpoint<UpdateMilestoneRequest, MilestoneDto>
{
    public override void Configure()
    {
        Put("/api/milestones/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateMilestoneRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var normalizedName = MilestoneValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Milestone name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        if (req.EndDate < req.StartDate)
            ThrowError("EndDate must be greater than or equal to StartDate.", StatusCodes.Status400BadRequest);

        var milestone = await db.Milestones
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (milestone is null)
            ThrowError("Milestone not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, milestone!.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var eventStart = DateOnly.FromDateTime(milestone!.Event.StartDate);
        var eventEnd = DateOnly.FromDateTime(milestone.Event.EndDate);
        if (req.StartDate < eventStart || req.EndDate > eventEnd)
            ThrowError("Milestone dates must be within event date range.", StatusCodes.Status400BadRequest);

        milestone.Title = normalizedName;
        milestone.Description = MilestoneValidation.NormalizeOptional(req.Description);
        milestone.StartDate = req.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        milestone.EndDate = req.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        milestone.OrderIndex = req.SortOrder;
        milestone.Status = req.Status;

        await db.SaveChangesAsync(ct);
        await Send.OkAsync(ContractMapping.ToMilestoneDto(milestone), ct);
    }
}

// ---- DELETE /api/milestones/{id} — xóa mềm cột mốc và dữ liệu con ----
public sealed class DeleteMilestoneEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/milestones/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var milestone = await db.Milestones
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (milestone is null)
            ThrowError("Milestone not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, milestone!.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var categories = await db.EventCategories
            .IgnoreQueryFilters()
            .Where(x => x.MilestoneId == id)
            .ToListAsync(ct);

        var categoryIds = categories.Select(x => x.Id).ToList();

        var tasks = categoryIds.Count == 0
            ? new List<OrgTask>()
            : await db.Tasks
                .IgnoreQueryFilters()
                .Where(x => categoryIds.Contains(x.EventCategoryId))
                .ToListAsync(ct);

        foreach (var task in tasks)
            task.IsDeleted = true;

        foreach (var category in categories)
            category.IsDeleted = true;

        milestone!.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

// ---- POST /api/milestones/{id}/restore — khôi phục cột mốc và dữ liệu con ----
public sealed class RestoreMilestoneEndpoint(AppDbContext db) : EndpointWithoutRequest<MilestoneDto>
{
    public override void Configure()
    {
        Post("/api/milestones/{id:guid}/restore");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var milestone = await db.Milestones
            .IgnoreQueryFilters()
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (milestone is null)
            ThrowError("Milestone not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, milestone!.Event.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (milestone.Event.IsDeleted)
            ThrowError("Cannot restore milestone while event is deleted.", StatusCodes.Status409Conflict);

        var categories = await db.EventCategories
            .IgnoreQueryFilters()
            .Where(x => x.MilestoneId == id)
            .ToListAsync(ct);

        var categoryIds = categories.Select(x => x.Id).ToList();

        var tasks = categoryIds.Count == 0
            ? new List<OrgTask>()
            : await db.Tasks
                .IgnoreQueryFilters()
                .Where(x => categoryIds.Contains(x.EventCategoryId))
                .ToListAsync(ct);

        foreach (var task in tasks)
            task.IsDeleted = false;

        foreach (var category in categories)
            category.IsDeleted = false;

        milestone.IsDeleted = false;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToMilestoneDto(milestone), ct);
    }
}

// ---- Helper validate các tham số đầu vào cho module milestone ----
internal static class MilestoneValidation
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
