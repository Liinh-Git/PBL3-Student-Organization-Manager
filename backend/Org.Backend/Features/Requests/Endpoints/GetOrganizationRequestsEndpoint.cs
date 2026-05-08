using FastEndpoints;
using Org.Backend.Features.Requests.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Endpoints;

public class GetOrganizationRequestsEndpoint : Endpoint<GetOrganizationRequestsRequest, ApiResponse<List<RequestDto>>>
{
    private readonly IRequestService _requestService;

    public GetOrganizationRequestsEndpoint(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public override void Configure()
    {
        Get("/organizations/{orgId}/requests");
        Description(b => b
            .Produces<ApiResponse<List<RequestDto>>>(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(GetOrganizationRequestsRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _requestService.GetOrganizationRequestsAsync(userId, req.OrgId, ct);
            Response = ApiResponse<List<RequestDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<RequestDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<RequestDto>>.ErrorResponse("Failed to get requests", new List<string> { ex.Message });
        }
    }
}

public record GetOrganizationRequestsRequest
{
    public Guid OrgId { get; init; }
}

