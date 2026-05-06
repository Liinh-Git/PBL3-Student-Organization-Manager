using System.Net;
using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Shared;
using Org.Shared.Contracts;
using FeatureCreateMemberRequest = Org.Shared.Features.Members.CreateMemberRequest;
using FeatureGetMembersResponse = Org.Shared.Features.Members.GetMembersResponse;
using FeatureMemberDto = Org.Shared.Features.Members.MemberDto;
using FeatureUpdateMemberDepartmentRequest = Org.Shared.Features.Members.UpdateMemberDepartmentRequest;
using FeatureUpdateMemberRoleRequest = Org.Shared.Features.Members.UpdateMemberRoleRequest;
using FeatureGetMyOrganizationsResponse = Org.Shared.Features.Users.GetMyOrganizationsResponse;

namespace Org.Frontend.Services.Members;

public sealed class MemberApiClient(
    IAuthenticatedBackendClient backendClient) : IMemberService
{
    private static readonly Guid LeaderRoleId = Guid.Parse("7e9e6f36-0508-4da8-adf1-c15b0cbe2f3a");
    private static readonly Guid CoreMemberRoleId = Guid.Parse("f80bb5dc-ab54-48fa-9453-869a12a81dc8");
    private static readonly Guid CollaboratorRoleId = Guid.Parse("1a96a5f4-6dfc-42db-936d-f86ee4e35ef5");

    private readonly IAuthenticatedBackendClient _backendClient = backendClient;

    public async Task<List<MemberDto>> GetMembers(Guid orgId)
    {
        var payload = await _backendClient.GetFromJsonAsync<FeatureGetMembersResponse>($"api/organizations/{orgId}/members")
            ?? new FeatureGetMembersResponse([]);

        return payload.Items.Select(MapLegacyDto).ToList();
    }

    public async Task<MemberDto> CreateMember(Guid orgId, FeatureCreateMemberRequest req)
    {
        var created = await _backendClient.PostAsJsonAsync<FeatureCreateMemberRequest, FeatureMemberDto>(
            $"api/organizations/{orgId}/members",
            req) ?? throw new InvalidOperationException("API returned no member payload.");

        return MapLegacyDto(created);
    }

    public async Task AssignRole(Guid memberId, Guid roleId)
    {
        var payload = new FeatureUpdateMemberRoleRequest(MapLegacyRoleId(roleId));
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/members/{memberId}/role")
        {
            Content = JsonContent.Create(payload)
        };

        using var _ = await _backendClient.SendAsync(request, CancellationToken.None);
    }

    public async Task AssignDepartment(Guid memberId, Guid departmentId)
    {
        var payload = new FeatureUpdateMemberDepartmentRequest(departmentId);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/members/{memberId}/department")
        {
            Content = JsonContent.Create(payload)
        };

        using var _ = await _backendClient.SendAsync(request, CancellationToken.None);
    }

    public async Task DeleteMember(Guid memberId)
    {
        await _backendClient.DeleteAsync($"api/members/{memberId}", CancellationToken.None);
    }

    public async Task<bool> CanManageOrganizationMembersAsync(Guid orgId)
    {
        var payload = await _backendClient.GetFromJsonAsync<FeatureGetMyOrganizationsResponse>("api/users/me/organizations")
            ?? new FeatureGetMyOrganizationsResponse([]);

        var membership = payload.Items.FirstOrDefault(x => x.OrganizationId == orgId);
        if (membership is null)
        {
            return false;
        }

        return string.Equals(membership.MemberRole, "President", StringComparison.OrdinalIgnoreCase)
            || string.Equals(membership.MemberRole, "VicePresident", StringComparison.OrdinalIgnoreCase)
            || string.Equals(membership.MemberRole, "Manager", StringComparison.OrdinalIgnoreCase)
            || string.Equals(membership.MemberRole, "Owner", StringComparison.OrdinalIgnoreCase)
            || string.Equals(membership.MemberRole, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task LeaveOrganizationAsync(Guid orgId)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/organizations/{orgId}/leave");
            using var _ = await _backendClient.SendAsync(request, CancellationToken.None);
        }
        catch (AuthApiException ex) when (ex.StatusCode == (int)HttpStatusCode.NotFound)
        {
            throw new NotSupportedException("Leave organization endpoint is not available in current backend.");
        }
    }

    private static MemberDto MapLegacyDto(FeatureMemberDto source)
    {
        return new MemberDto
        {
            Id = source.Id,
            OrgId = source.OrganizationId,
            UserId = Guid.Empty,
            DisplayName = source.FullName,
            Email = source.Email,
            DepartmentId = source.DepartmentId,
            RoleId = MapFeatureRole(source.Role),
            RoleName = source.Role.ToString(),
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
