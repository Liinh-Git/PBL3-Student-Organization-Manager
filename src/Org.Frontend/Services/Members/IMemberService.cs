using Org.Shared.Contracts;

namespace Org.Frontend.Services.Members;

public interface IMemberService
{
    Task<List<MemberDto>> GetMembers(Guid orgId);
    Task AssignRole(Guid memberId, Guid roleId);
    Task AssignDepartment(Guid memberId, Guid departmentId);
}
