using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events;

public sealed class GetOrganizationEventsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetOrganizationEventsResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{orgId:guid}/events");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");

        var items = await db.Events
            .AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new EventTreeNodeDto(
                x.Id,
                x.EventName,
                x.Status,
                DateOnly.FromDateTime(x.StartDate),
                DateOnly.FromDateTime(x.EndDate),
                x.Milestones.Count,
                x.Milestones.SelectMany(m => m.Categories).Count(),
                x.Milestones.SelectMany(m => m.Categories).SelectMany(c => c.Tasks).Count(),
                x.Milestones.SelectMany(m => m.Categories).SelectMany(c => c.Tasks).Count(t => t.Status == Org.Shared.TaskStatus.Done)))
            .ToListAsync(ct);

        await Send.OkAsync(new GetOrganizationEventsResponse(items), ct);
    }
}

public sealed class CreateEventEndpoint(AppDbContext db) : Endpoint<CreateEventRequest, EventDto>
{
    public override void Configure()
    {
        Post("/api/events");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateEventRequest req, CancellationToken ct)
    {
        if (req.EndDate < req.StartDate)
            ThrowError("EndDate must be greater than or equal to StartDate.", StatusCodes.Status400BadRequest);

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == req.OrganizationId, ct);
        if (!orgExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var entity = new Event
        {
            OrgId = req.OrganizationId,
            EventName = req.Name.Trim(),
            Description = req.Description?.Trim(),
            StartDate = req.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = req.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            Status = EventStatus.Draft
        };

        db.Events.Add(entity);
        await db.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        await Send.OkAsync(ContractMapping.ToEventDto(entity), ct);
    }
}

public sealed class GetEventByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetEventByIdResponse>
{
    public override void Configure()
    {
        Get("/api/events/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var entity = await db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        await Send.OkAsync(new GetEventByIdResponse(ContractMapping.ToEventDto(entity!)), ct);
    }
}
