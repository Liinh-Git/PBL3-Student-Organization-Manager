using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members.Services;

public interface IMemberService
{
    // Read operations
    Task<List<MemberDto>> GetOrganizationMembersAsync(Guid orgId, Guid userId, CancellationToken ct = default);
    
    // Write operations
    Task<MemberDto> AddMemberAsync(Guid orgId, Guid userId, AddMemberRequest request, CancellationToken ct = default);
    Task<MemberDto> UpdateMemberDepartmentAsync(Guid memberId, Guid userId, UpdateMemberDepartmentRequest request, CancellationToken ct = default);
    Task<bool> RemoveMemberAsync(Guid memberId, Guid userId, RemoveMemberRequest? request, CancellationToken ct = default);
}
