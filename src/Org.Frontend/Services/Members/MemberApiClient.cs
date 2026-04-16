using System.Net.Http.Json;
using Org.Shared;
using Org.Shared.Contracts;
using FeatureCreateMemberRequest = Org.Shared.Features.Members.CreateMemberRequest;
using FeatureGetMembersResponse = Org.Shared.Features.Members.GetMembersResponse;
using FeatureMemberDto = Org.Shared.Features.Members.MemberDto;
using FeatureUpdateMemberDepartmentRequest = Org.Shared.Features.Members.UpdateMemberDepartmentRequest;
using FeatureUpdateMemberRoleRequest = Org.Shared.Features.Members.UpdateMemberRoleRequest;

namespace Org.Frontend.Services.Members;

public sealed class MemberApiClient(
    HttpClient httpClient) : IMemberService
{
    private static readonly Guid LeaderRoleId = Guid.Parse("7e9e6f36-0508-4da8-adf1-c15b0cbe2f3a");
    private static readonly Guid CoreMemberRoleId = Guid.Parse("f80bb5dc-ab54-48fa-9453-869a12a81dc8");
    private static readonly Guid CollaboratorRoleId = Guid.Parse("1a96a5f4-6dfc-42db-936d-f86ee4e35ef5");

    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<MemberDto>> GetMembers(Guid orgId)
    {
        var payload = await _httpClient.GetFromJsonAsync<FeatureGetMembersResponse>($"api/organizations/{orgId}/members", CancellationToken.None)
            ?? new FeatureGetMembersResponse([]);

        return payload.Items.Select(MapLegacyDto).ToList();
    }

    public async Task<MemberDto> CreateMember(Guid orgId, FeatureCreateMemberRequest req)
    {
        using var response = await _httpClient.PostAsJsonAsync($"api/organizations/{orgId}/members", req, CancellationToken.None);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<FeatureMemberDto>(cancellationToken: CancellationToken.None)
            ?? throw new InvalidOperationException("API returned no member payload.");

        return MapLegacyDto(created);
    }

    public async Task AssignRole(Guid memberId, Guid roleId)
    {
        var payload = new FeatureUpdateMemberRoleRequest(MapLegacyRoleId(roleId));
        using var response = await _httpClient.PutAsJsonAsync($"api/members/{memberId}/role", payload, CancellationToken.None);
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignDepartment(Guid memberId, Guid departmentId)
    {
        var payload = new FeatureUpdateMemberDepartmentRequest(departmentId);
        using var response = await _httpClient.PutAsJsonAsync($"api/members/{memberId}/department", payload, CancellationToken.None);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMember(Guid memberId)
    {
        using var response = await _httpClient.DeleteAsync($"api/members/{memberId}", CancellationToken.None);
        response.EnsureSuccessStatusCode();
    }

    private static MemberDto MapLegacyDto(FeatureMemberDto source)
    {
        return new MemberDto
        {
            Id = source.Id,
            OrgId = source.OrganizationId,
            UserId = Guid.Empty,
            DisplayName = source.FullName,
            DepartmentId = source.DepartmentId,
            RoleId = MapFeatureRole(source.Role),
            JoinDate = source.JoinedAtUtc.UtcDateTime
        };
    }

    private static Guid MapFeatureRole(MemberRole role)
    {
        return role switch
        {
            MemberRole.President => LeaderRoleId,
            MemberRole.VicePresident => CoreMemberRoleId,
            MemberRole.Manager => CoreMemberRoleId,
            _ => CollaboratorRoleId
        };
    }

    private static MemberRole MapLegacyRoleId(Guid roleId)
    {
        if (roleId == LeaderRoleId)
            return MemberRole.President;

        if (roleId == CoreMemberRoleId)
            return MemberRole.Manager;

        return MemberRole.Member;
    }
}
