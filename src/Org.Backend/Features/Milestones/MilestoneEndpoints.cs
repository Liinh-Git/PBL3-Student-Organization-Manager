using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones;

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

        if (req.EndDate < req.StartDate)
            ThrowError("EndDate must be greater than or equal to StartDate.", StatusCodes.Status400BadRequest);

        var @event = await db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId, ct);
        if (@event is null)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        var milestone = new Milestone
        {
            EventId = eventId,
            Title = req.Name.Trim(),
            Description = req.Description?.Trim(),
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

        var exists = await db.Events.AsNoTracking().AnyAsync(x => x.Id == eventId, ct);
        if (!exists)
            ThrowError("Event not found.", StatusCodes.Status404NotFound);

        var items = await db.Milestones
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.OrderIndex)
            .Select(x => ContractMapping.ToMilestoneDto(x))
            .ToListAsync(ct);

        await Send.OkAsync(new GetMilestonesResponse(items), ct);
    }
}
