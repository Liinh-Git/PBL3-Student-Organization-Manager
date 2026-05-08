using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Members.Services;
using Org.Backend.Features.Members.Validators;
using Org.Shared.Common;
using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members.Endpoints;

public class UpdateMemberDepartmentEndpoint : Endpoint<UpdateMemberDepartmentRequest, ApiResponse<MemberDto>>
{
    private readonly IMemberService _memberService;

    public UpdateMemberDepartmentEndpoint(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public override void Configure()
    {
        Put("/members/{id}/department");
        Validator<UpdateMemberDepartmentRequestValidator>();
    }

    public override async Task HandleAsync(UpdateMemberDepartmentRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<MemberDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var memberId = Route<Guid>("id");

            var member = await _memberService.UpdateMemberDepartmentAsync(memberId, userId, req, ct);

            Response = ApiResponse<MemberDto>.SuccessResponse(member, "Member department updated successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<MemberDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<MemberDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<MemberDto>.ErrorResponse($"An error occurred: {ex.Message}");
        }
    }
}
