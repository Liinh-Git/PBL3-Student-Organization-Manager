using FastEndpoints;
using FluentValidation;
using Org.Backend.Features.Requests.Services;
using Org.Backend.Features.Requests.Validators;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Endpoints;

public class ReviewRequestEndpoint : Endpoint<ReviewRequestEndpointRequest, ApiResponse<RequestDto>>
{
    private readonly IRequestService _requestService;

    public ReviewRequestEndpoint(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public override void Configure()
    {
        Post("/organizations/requests/{requestId}/review");
        Validator<ReviewRequestEndpointRequestValidator>();
        Description(b => b
            .Produces<ApiResponse<RequestDto>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(ReviewRequestEndpointRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _requestService.ReviewRequestAsync(userId, req.RequestId, req.Review, ct);
            Response = ApiResponse<RequestDto>.SuccessResponse(result, "Request reviewed successfully");
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
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<RequestDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<RequestDto>.ErrorResponse("Failed to review request", new List<string> { ex.Message });
        }
    }
}

public record ReviewRequestEndpointRequest
{
    public Guid RequestId { get; init; }
    public ReviewRequestRequest Review { get; init; } = null!;
}

public class ReviewRequestEndpointRequestValidator : Validator<ReviewRequestEndpointRequest>
{
    public ReviewRequestEndpointRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Request ID is required");

        RuleFor(x => x.Review)
            .NotNull().WithMessage("Review data is required")
            .SetValidator(new ReviewRequestRequestValidator());
    }
}

