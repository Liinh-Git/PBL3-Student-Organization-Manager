// ---- Các endpoint CRUD cho module sự kiện (Event) ----
// Luồng xóa/khôi phục: cascade soft-delete toàn bộ Milestone → Category → Task con
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using Org.Shared.Features.Events;
using Org.Shared.Features.Users;
using System.Security.Claims;

namespace Org.Backend.Features.Events;

// ---- GET /api/organizations/{orgId}/events — danh sách sự kiện kèm số liệu tổng hợp ----
public sealed class GetOrganizationEventsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetOrganizationEventsResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{orgId:guid}/events");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!orgExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var items = await db.Events
            .AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new
            {
                x.Id,
                x.EventName,
                x.Status,
                x.StartDate,
                x.EndDate,
                MilestoneCount = x.Milestones.Count,
                CategoryCount = x.Milestones.SelectMany(m => m.Categories).Count(),
                TaskCounts = x.Milestones
                    .SelectMany(m => m.Categories)
                    .SelectMany(c => c.Tasks)
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Done = g.Count(t => t.Status == Org.Shared.TaskStatus.Done)
                    })
                    .FirstOrDefault()
            })
            .Select(x => new EventTreeNodeDto(
                x.Id,
                x.EventName,
                x.Status,
                DateOnly.FromDateTime(x.StartDate),
                DateOnly.FromDateTime(x.EndDate),
                x.MilestoneCount,
                x.CategoryCount,
                x.TaskCounts == null ? 0 : x.TaskCounts.Total,
                x.TaskCounts == null ? 0 : x.TaskCounts.Done))
            .ToListAsync(ct);

        await Send.OkAsync(new GetOrganizationEventsResponse(items), ct);
    }
}

// ---- GET /api/users/me/events — danh sách sự kiện user đã ghi danh (theo Attendee) ----
public sealed class GetMyRegisteredEventsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetMyRegisteredEventsResponse>
{
    private const string DefaultRegisteredEventImageUrl = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=1200&q=80";

    public override void Configure()
    {
        Get("/api/users/me/events");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var rows = await db.Attendees
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status != AttendeeStatus.Cancelled)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.EventId,
                x.Event.OrgId,
                OrganizationName = x.Event.Organization.OrgName,
                EventName = x.Event.EventName,
                EventDescription = x.Event.Description,
                x.Event.StartDate,
                x.Event.EndDate,
                x.Event.Status,
                x.Event.Location,
                RegistrationStatus = x.Status,
                RegisteredAtUtc = x.CreatedAt,
                EventImageUrl = x.Event.DigitalAssets
                    .Where(a => a.FileType == FileType.Image && !string.IsNullOrWhiteSpace(a.FileUrl))
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.FileUrl)
                    .FirstOrDefault(),
                OrganizationCoverUrl = x.Event.Organization.CoverUrl,
                OrganizationAvatarUrl = x.Event.Organization.AvatarUrl
            })
            .ToListAsync(ct);

        var items = rows
            .GroupBy(x => x.EventId)
            .Select(g => g.First())
            .OrderBy(x => x.StartDate)
            .Select(x => new MyRegisteredEventDto(
                x.EventId,
                x.OrgId,
                x.OrganizationName,
                x.EventName,
                x.EventDescription,
                DateOnly.FromDateTime(x.StartDate),
                DateOnly.FromDateTime(x.EndDate),
                x.Status,
                x.RegistrationStatus.ToString(),
                new DateTimeOffset(DateTime.SpecifyKind(x.RegisteredAtUtc, DateTimeKind.Utc)),
                x.Location,
                ResolveRegisteredEventImageUrl(x.EventImageUrl, x.OrganizationCoverUrl, x.OrganizationAvatarUrl)))
            .ToList();

        await Send.OkAsync(new GetMyRegisteredEventsResponse(items), ct);
    }

    private static string ResolveRegisteredEventImageUrl(string? eventImageUrl, string? organizationCoverUrl, string? organizationAvatarUrl)
    {
        if (!string.IsNullOrWhiteSpace(eventImageUrl))
            return eventImageUrl.Trim();

        if (!string.IsNullOrWhiteSpace(organizationCoverUrl))
            return organizationCoverUrl.Trim();

        if (!string.IsNullOrWhiteSpace(organizationAvatarUrl))
            return organizationAvatarUrl.Trim();

        return DefaultRegisteredEventImageUrl;
    }
}

// ---- GET /api/users/me/discover/events — danh sách sự kiện đề xuất ----
public sealed class GetSuggestedEventsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetSuggestedEventsResponse>
{
    private const string DefaultEventImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=1200&q=80";

    public override void Configure()
    {
        Get("/api/users/me/discover/events");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var joinedEventIds = await db.Attendees
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status != AttendeeStatus.Cancelled)
            .Select(x => x.EventId)
            .Distinct()
            .ToListAsync(ct);

        var today = DateTime.UtcNow.Date;

        var rows = await db.Events
            .AsNoTracking()
            .Where(x => !joinedEventIds.Contains(x.Id) && x.EndDate >= today)
            .Select(x => new
            {
                x.Id,
                x.OrgId,
                OrganizationName = x.Organization.OrgName,
                EventName = x.EventName,
                EventDescription = x.Description,
                x.StartDate,
                x.EndDate,
                x.Status,
                x.Location,
                EventImageUrl = x.DigitalAssets
                    .Where(a => a.FileType == FileType.Image && !string.IsNullOrWhiteSpace(a.FileUrl))
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.FileUrl)
                    .FirstOrDefault(),
                OrganizationCoverUrl = x.Organization.CoverUrl,
                OrganizationAvatarUrl = x.Organization.AvatarUrl,
                RegisteredCount = x.Attendees.Count(a => a.Status != AttendeeStatus.Cancelled)
            })
            .OrderBy(x => x.StartDate < today ? 1 : 0)
            .ThenBy(x => x.StartDate)
            .ThenBy(x => x.EventName)
            .Take(12)
            .ToListAsync(ct);

        var items = rows
            .Select(x => new SuggestedEventDto(
                x.Id,
                x.OrgId,
                x.OrganizationName,
                x.EventName,
                x.EventDescription,
                DateOnly.FromDateTime(x.StartDate),
                DateOnly.FromDateTime(x.EndDate),
                x.Status,
                x.Location,
                ResolveEventImageUrl(x.EventImageUrl, x.OrganizationCoverUrl, x.OrganizationAvatarUrl),
                x.RegisteredCount))
            .ToList();

        await Send.OkAsync(new GetSuggestedEventsResponse(items), ct);
    }

    private static string ResolveEventImageUrl(string? eventImageUrl, string? organizationCoverUrl, string? organizationAvatarUrl)
    {
        if (!string.IsNullOrWhiteSpace(eventImageUrl))
            return eventImageUrl.Trim();

        if (!string.IsNullOrWhiteSpace(organizationCoverUrl))
            return organizationCoverUrl.Trim();

        if (!string.IsNullOrWhiteSpace(organizationAvatarUrl))
            return organizationAvatarUrl.Trim();

        return DefaultEventImageUrl;
    }
}

// ---- POST /api/events — tạo sự kiện mới (yêu cầu vai trò Manager trở lên) ----
public sealed class CreateEventEndpoint(AppDbContext db) : Endpoint<CreateEventRequest, EventDto>
{
    public override void Configure()
    {
        Post("/api/events");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateEventRequest req, CancellationToken ct)
    {
        var normalizedName = EventValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Event name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        if (req.EndDate < req.StartDate)
            ThrowError("EndDate must be greater than or equal to StartDate.", StatusCodes.Status400BadRequest);

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == req.OrganizationId, ct);
        if (!orgExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, req.OrganizationId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var entity = new Event
        {
            OrgId = req.OrganizationId,
            EventName = normalizedName,
            Description = EventValidation.NormalizeOptional(req.Description),
            StartDate = req.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = req.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Status = EventStatus.Draft
        };

        db.Events.Add(entity);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(ContractMapping.ToEventDto(entity), StatusCodes.Status201Created, cancellation: ct);
    }
}

// ---- GET /api/events/{id} — chi tiết một sự kiện ----
public sealed class GetEventByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetEventByIdResponse>
{
    public override void Configure()
    {
        Get("/api/events/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var entity = await db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, entity!.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await Send.OkAsync(new GetEventByIdResponse(ContractMapping.ToEventDto(entity!)), ct);
    }
}

// ---- PUT /api/events/{id} — cập nhật thông tin và trạng thái sự kiện ----
public sealed class UpdateEventEndpoint(AppDbContext db) : Endpoint<UpdateEventRequest, EventDto>
{
    public override void Configure()
    {
        Put("/api/events/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateEventRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var normalizedName = EventValidation.NormalizeName(req.Name);
        if (normalizedName is null)
            ThrowError("Event name must be at least 2 characters.", StatusCodes.Status400BadRequest);

        if (req.EndDate < req.StartDate)
            ThrowError("EndDate must be greater than or equal to StartDate.", StatusCodes.Status400BadRequest);

        var entity = await db.Events.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, entity!.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        entity!.EventName = normalizedName;
        entity.Description = EventValidation.NormalizeOptional(req.Description);
        entity.StartDate = req.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        entity.EndDate = req.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        entity.Status = req.Status;

        await db.SaveChangesAsync(ct);
        await Send.OkAsync(ContractMapping.ToEventDto(entity), ct);
    }
}

// ---- DELETE /api/events/{id} — xóa mềm sự kiện và toàn bộ dữ liệu con ----
public sealed class DeleteEventEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/events/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var entity = await db.Events.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, entity!.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var milestones = await db.Milestones
            .IgnoreQueryFilters()
            .Where(x => x.EventId == id)
            .ToListAsync(ct);

        var milestoneIds = milestones.Select(x => x.Id).ToList();

        var categories = milestoneIds.Count == 0
            ? []
            : await db.EventCategories
                .IgnoreQueryFilters()
                .Where(x => milestoneIds.Contains(x.MilestoneId))
                .ToListAsync(ct);

        var categoryIds = categories.Select(x => x.Id).ToList();

        var tasks = categoryIds.Count == 0
            ? []
            : await db.Tasks
                .IgnoreQueryFilters()
                .Where(x => categoryIds.Contains(x.EventCategoryId))
                .ToListAsync(ct);

        foreach (var task in tasks)
            task.IsDeleted = true;

        foreach (var category in categories)
            category.IsDeleted = true;

        foreach (var milestone in milestones)
            milestone.IsDeleted = true;

        entity!.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

// ---- POST /api/events/{id}/restore — khôi phục sự kiện và dữ liệu con đã xóa mềm ----
public sealed class RestoreEventEndpoint(AppDbContext db) : EndpointWithoutRequest<EventDto>
{
    public override void Configure()
    {
        Post("/api/events/{id:guid}/restore");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var entity = await db.Events
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        var organization = await db.Organizations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == entity!.OrgId, ct);

        if (organization is null)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        if (organization.IsDeleted)
            ThrowError("Cannot restore event while organization is deleted.", StatusCodes.Status409Conflict);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, entity!.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var milestones = await db.Milestones
            .IgnoreQueryFilters()
            .Where(x => x.EventId == id)
            .ToListAsync(ct);

        var milestoneIds = milestones.Select(x => x.Id).ToList();

        var categories = milestoneIds.Count == 0
            ? []
            : await db.EventCategories
                .IgnoreQueryFilters()
                .Where(x => milestoneIds.Contains(x.MilestoneId))
                .ToListAsync(ct);

        var categoryIds = categories.Select(x => x.Id).ToList();

        var tasks = categoryIds.Count == 0
            ? []
            : await db.Tasks
                .IgnoreQueryFilters()
                .Where(x => categoryIds.Contains(x.EventCategoryId))
                .ToListAsync(ct);

        foreach (var task in tasks)
            task.IsDeleted = false;

        foreach (var category in categories)
            category.IsDeleted = false;

        foreach (var milestone in milestones)
            milestone.IsDeleted = false;

        entity.IsDeleted = false;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToEventDto(entity), ct);
    }
}


