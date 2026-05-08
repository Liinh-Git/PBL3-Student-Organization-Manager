using FastEndpoints;
using Org.Backend.Features.Requests.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Endpoints;

public class GetRequestByIdEndpoint : Endpoint<GetRequestByIdRequest, ApiResponse<RequestDto>>
{
    private readonly IRequestService _requestService;

    public GetRequestByIdEndpoint(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public override void Configure()
    {
        Get("/requests/{requestId}");
        Description(b => b
            .Produces<ApiResponse<RequestDto>>(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(GetRequestByIdRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _requestService.GetRequestByIdAsync(userId, req.RequestId, ct);
            Response = ApiResponse<RequestDto>.SuccessResponse(result);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<RequestDto>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<RequestDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<RequestDto>.ErrorResponse("Failed to get request", new List<string> { ex.Message });
        }
    }
}

public record GetRequestByIdRequest
{
    public Guid RequestId { get; init; }
}

