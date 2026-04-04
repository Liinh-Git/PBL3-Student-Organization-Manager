using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");

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

public sealed class CreateEventEndpoint(AppDbContext db) : Endpoint<CreateEventRequest, EventDto>
{
    public override void Configure()
    {
        Post("/api/events");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
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

        await HttpContext.Response.SendAsync(ContractMapping.ToEventDto(entity), StatusCodes.Status201Created, cancellation: ct);
    }
}

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

        await Send.OkAsync(new GetEventByIdResponse(ContractMapping.ToEventDto(entity!)), ct);
    }
}
