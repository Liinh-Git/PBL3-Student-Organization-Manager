using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Members.Services;
using Org.Shared.Common;
using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members.Endpoints;

public class RemoveMemberEndpoint : Endpoint<RemoveMemberRequest, ApiResponse<bool>>
{
    private readonly IMemberService _memberService;

    public RemoveMemberEndpoint(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public override void Configure()
    {
        Delete("/members/{id}");
    }

    public override async Task HandleAsync(RemoveMemberRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var memberId = Route<Guid>("id");

            var result = await _memberService.RemoveMemberAsync(memberId, userId, req, ct);

            Response = ApiResponse<bool>.SuccessResponse(result, "Member removed successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}");
        }
    }
}
