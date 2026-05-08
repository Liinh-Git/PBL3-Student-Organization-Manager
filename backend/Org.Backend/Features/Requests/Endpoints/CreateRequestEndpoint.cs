using FastEndpoints;
using FluentValidation;
using Org.Backend.Features.Requests.Services;
using Org.Backend.Features.Requests.Validators;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Endpoints;

public class CreateRequestEndpoint : Endpoint<CreateRequestEndpointRequest, ApiResponse<RequestDto>>
{
    private readonly IRequestService _requestService;

    public CreateRequestEndpoint(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/requests");
        Validator<CreateRequestEndpointRequestValidator>();
        Description(b => b
            .Produces<ApiResponse<RequestDto>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(CreateRequestEndpointRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _requestService.CreateRequestAsync(userId, req.OrgId, req.Request, ct);
            Response = ApiResponse<RequestDto>.SuccessResponse(result, "Request submitted successfully");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
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
            Response = ApiResponse<RequestDto>.ErrorResponse("Failed to create request", new List<string> { ex.Message });
        }
    }
}

public record CreateRequestEndpointRequest
{
    public Guid OrgId { get; init; }
    public CreateRequestRequest Request { get; init; } = null!;
}

public class CreateRequestEndpointRequestValidator : Validator<CreateRequestEndpointRequest>
{
    public CreateRequestEndpointRequestValidator()
    {
        RuleFor(x => x.OrgId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request data is required")
            .SetValidator(new CreateRequestRequestValidator());
    }
}

