using Org.Backend.Domain.Entities;
using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members.Mappings;

public static class MemberMappings
{
    public static MemberDto ToMemberDto(this Member member)
    {
        return new MemberDto
        {
            Id = member.Id,
            OrganizationId = member.OrgId,
            UserId = member.UserId,
            DepartmentId = member.DepartmentId,
            DepartmentName = member.Department?.DeptName,
            RoleId = member.RoleId,
            RoleName = member.Role?.RoleName,
            StudentCode = member.StudentCode,
            FullName = member.User.FullName,
            Email = member.User.Email,
            AvatarUrl = member.User.AvatarUrl,
            Status = member.Status.ToString(),
            JoinedAtUtc = member.JoinDate
        };
    }
}
