namespace Org.Backend.Features.Members;

internal static class MembersFeatureTodos
{
    // TODO(BE-DAY1): GET /api/organizations/{orgId}/members
    //  - Return 200 and { items: [...] } sorted by FullName.
    //  - Include role + department for FE filter UI.
    //
    // TODO(BE-DAY1): PUT /api/members/{id}/role
    //  - Validate requested role belongs to org role policy.
    //  - Prevent unauthorized role escalation.
    //  - Return updated MemberDto.
    //
    // TODO(BE-DAY1): PUT /api/members/{id}/department
    //  - Validate department belongs to same organization.
    //  - Allow null department for unassigned members.
    //  - Return updated MemberDto.
}
