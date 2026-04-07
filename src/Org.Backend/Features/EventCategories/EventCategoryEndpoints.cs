using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories;

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

        var milestoneExists = await db.Milestones.AsNoTracking().AnyAsync(x => x.Id == milestoneId, ct);
        if (!milestoneExists)
            ThrowError("Milestone not found.", StatusCodes.Status404NotFound);

        var entity = new EventCategory
        {
            MilestoneId = milestoneId,
            CategoryName = req.Name.Trim(),
            Description = req.Description?.Trim(),
            OrderIndex = req.SortOrder
        };

        db.EventCategories.Add(entity);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(ContractMapping.ToCategoryDto(entity, 0, 0), StatusCodes.Status201Created, cancellation: ct);
    }
}

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

        var milestoneExists = await db.Milestones.AsNoTracking().AnyAsync(x => x.Id == milestoneId, ct);
        if (!milestoneExists)
            ThrowError("Milestone not found.", StatusCodes.Status404NotFound);

        var items = await db.EventCategories
            .AsNoTracking()
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
