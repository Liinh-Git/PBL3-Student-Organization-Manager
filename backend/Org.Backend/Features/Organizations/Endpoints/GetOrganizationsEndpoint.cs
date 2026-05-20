using FastEndpoints;
using Org.Backend.Features.Organizations.Services;
using Org.Shared.Common;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Endpoints;

/// <summary>
/// Endpoint for listing organizations
/// GET /api/organizations
/// </summary>
public class GetOrganizationsEndpoint : EndpointWithoutRequest<ApiResponse<List<OrganizationSummaryDto>>>
{
    private readonly IOrganizationService _organizationService;

    public GetOrganizationsEndpoint(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public override void Configure()
    {
        Get("/organizations");
        Description(b => b
            .Produces<ApiResponse<List<OrganizationSummaryDto>>>(200, "application/json")
            .Produces<ApiResponse<List<OrganizationSummaryDto>>>(401, "application/json")
            .WithTags("Organizations"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var result = await _organizationService.GetOrganizationsAsync(ct);
            Response = ApiResponse<List<OrganizationSummaryDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<OrganizationSummaryDto>>.ErrorResponse("Failed to get organizations", new List<string> { ex.Message });
        }
    }
}
