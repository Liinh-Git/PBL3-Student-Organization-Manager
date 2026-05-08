using Org.Backend.Domain.Entities;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Mappings;

public static class RequestMappings
{
    public static RequestDto ToRequestDto(this Request request)
    {
        return new RequestDto
        {
            Id = request.Id,
            SenderId = request.SenderId,
            SenderName = request.Sender?.FullName ?? string.Empty,
            SenderEmail = request.Sender?.Email,
            OrganizationId = request.OrgId,
            OrganizationName = request.Organization?.OrgName ?? string.Empty,
            RequestType = request.RequestType.ToString(),
            Title = request.Title,
            Content = request.Content,
            DesiredDepartmentId = request.DesiredDepartmentId,
            DesiredDepartmentName = request.DesiredDepartment?.DeptName,
            DesiredPosition = request.DesiredPosition,
            Status = request.Status.ToString(),
            ReviewNote = request.ReviewNote,
            ReviewedByMemberId = request.ReviewedByMemberId,
            ReviewedByMemberName = request.ReviewedByMember?.User?.FullName,
            ReviewedAt = request.ReviewedAt,
            CreatedAtUtc = request.CreatedAt,
            UpdatedAtUtc = request.UpdatedAt
        };
    }
}
