using FastEndpoints;
using Org.Backend.Features.Organizations.Services;
using Org.Shared.Common;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Endpoints;

/// <summary>
/// Request for getting organization public overview
/// </summary>
public class GetPublicOverviewRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// Endpoint for getting organization public overview (no auth required)
/// GET /api/organizations/{id}/public-overview
/// </summary>
public class GetPublicOverviewEndpoint : Endpoint<GetPublicOverviewRequest, ApiResponse<OrganizationPublicOverviewDto>>
{
    private readonly IOrganizationService _organizationService;

    public GetPublicOverviewEndpoint(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public override void Configure()
    {
        Get("/organizations/{id}/public-overview");
        AllowAnonymous();
        Description(b => b
            .Produces<ApiResponse<OrganizationPublicOverviewDto>>(200, "application/json")
            .Produces<ApiResponse<OrganizationPublicOverviewDto>>(404, "application/json")
            .WithTags("Organizations"));
    }

    public override async Task HandleAsync(GetPublicOverviewRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _organizationService.GetPublicOverviewAsync(req.Id, ct);
            Response = ApiResponse<OrganizationPublicOverviewDto>.SuccessResponse(result);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<OrganizationPublicOverviewDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<OrganizationPublicOverviewDto>.ErrorResponse("Failed to get public overview", new List<string> { ex.Message });
        }
    }
}
