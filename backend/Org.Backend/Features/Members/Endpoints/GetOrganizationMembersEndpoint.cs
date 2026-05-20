using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Members.Services;
using Org.Shared.Common;
using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members.Endpoints;

/// <summary>
/// Endpoint for getting organization members
/// GET /api/organizations/{orgId}/members
/// </summary>
public class GetOrganizationMembersEndpoint : EndpointWithoutRequest<ApiResponse<List<MemberDto>>>
{
    private readonly IMemberService _memberService;

    public GetOrganizationMembersEndpoint(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public override void Configure()
    {
        Get("/organizations/{orgId}/members");
        Description(b => b
            .Produces<ApiResponse<List<MemberDto>>>(200, "application/json")
            .Produces<ApiResponse<List<MemberDto>>>(401, "application/json")
            .Produces<ApiResponse<List<MemberDto>>>(403, "application/json")
            .WithTags("Members"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<List<MemberDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Get orgId from route
            var orgIdStr = Route<string>("orgId");
            if (string.IsNullOrEmpty(orgIdStr) || !Guid.TryParse(orgIdStr, out var orgId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<List<MemberDto>>.ErrorResponse("Invalid organization ID");
                return;
            }

            var result = await _memberService.GetOrganizationMembersAsync(orgId, userId, ct);
            Response = ApiResponse<List<MemberDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<MemberDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<MemberDto>>.ErrorResponse("Failed to get members", new List<string> { ex.Message });
        }
    }
}
