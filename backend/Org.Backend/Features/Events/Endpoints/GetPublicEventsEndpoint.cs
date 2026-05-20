using FastEndpoints;
using Org.Backend.Features.Events.Services;
using Org.Shared.Common;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Endpoints;

/// <summary>
/// Endpoint for getting public events (no auth required)
/// GET /api/events/public
/// </summary>
public class GetPublicEventsEndpoint : EndpointWithoutRequest<ApiResponse<List<EventPublicDto>>>
{
    private readonly IEventService _eventService;

    public GetPublicEventsEndpoint(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override void Configure()
    {
        Get("/events/public");
        AllowAnonymous();
        Description(b => b
            .Produces<ApiResponse<List<EventPublicDto>>>(200, "application/json")
            .WithTags("Events"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var result = await _eventService.GetPublicEventsAsync(ct);
            Response = ApiResponse<List<EventPublicDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<EventPublicDto>>.ErrorResponse("Failed to get public events", new List<string> { ex.Message });
        }
    }
}
