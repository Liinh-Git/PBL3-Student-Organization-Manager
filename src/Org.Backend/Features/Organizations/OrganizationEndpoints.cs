using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Organizations;
using System.Security.Claims;

namespace Org.Backend.Features.Organizations;

public sealed class GetDefaultOrganizationEndpoint(AppDbContext db) : EndpointWithoutRequest<GetDefaultOrganizationResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/default");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        OrganizationSummaryDto? organization = null;

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdText, out var userId))
        {
            organization = await db.Members
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.JoinDate)
                .Select(x => new OrganizationSummaryDto(
                    x.Organization.Id,
                    x.Organization.OrgName,
                    x.Organization.Description))
                .FirstOrDefaultAsync(ct);
        }

        organization ??= await db.Organizations
            .AsNoTracking()
            .OrderBy(x => x.OrgName)
            .Select(x => new OrganizationSummaryDto(
                x.Id,
                x.OrgName,
                x.Description))
            .FirstOrDefaultAsync(ct);

        if (organization is null)
            ThrowError("No organization available.", StatusCodes.Status404NotFound);

        await Send.OkAsync(new GetDefaultOrganizationResponse(organization!), ct);
    }
}
