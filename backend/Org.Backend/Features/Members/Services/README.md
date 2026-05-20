# Members Services

## IMemberService / MemberService
**Methods**:
- `Task<List<MemberDto>> ListMembersAsync(Guid orgId, Guid userId)`
- `Task<MemberDto> AddMemberAsync(Guid orgId, AddMemberRequest request, Guid userId)`
- `Task<MemberDto> UpdateMemberDepartmentAsync(Guid memberId, Guid departmentId, Guid userId)`
- `Task RemoveMemberAsync(Guid memberId, Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
