using FastEndpoints;
using Org.Backend.Features.Discover.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Discover;

namespace Org.Backend.Features.Discover.Endpoints;

public class DiscoverEventsEndpoint : EndpointWithoutRequest<ApiResponse<List<DiscoverEventDto>>>
{
    private readonly IDiscoverService _discoverService;

    public DiscoverEventsEndpoint(IDiscoverService discoverService)
    {
        _discoverService = discoverService;
    }

    public override void Configure()
    {
        Get("/users/me/discover/events");
        Description(b => b
            .Produces<ApiResponse<List<DiscoverEventDto>>>(200)
            .Produces<ApiResponse<object>>(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _discoverService.DiscoverEventsAsync(userId, ct);
            Response = ApiResponse<List<DiscoverEventDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<DiscoverEventDto>>.ErrorResponse("Failed to discover events", new List<string> { ex.Message });
        }
    }
}

