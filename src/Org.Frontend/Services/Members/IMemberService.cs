using Org.Shared.Contracts;
using FeatureCreateMemberRequest = Org.Shared.Features.Members.CreateMemberRequest;

namespace Org.Frontend.Services.Members;

public interface IMemberService
{
    Task<List<MemberDto>> GetMembers(Guid orgId);
    Task<MemberDto> CreateMember(Guid orgId, FeatureCreateMemberRequest req);
    Task AssignRole(Guid memberId, Guid roleId);
    Task AssignDepartment(Guid memberId, Guid departmentId);
    Task DeleteMember(Guid memberId);
}
