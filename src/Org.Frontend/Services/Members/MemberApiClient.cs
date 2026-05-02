// ---- API client thực cho service thành viên — CRUD qua BE endpoint /api/members ----
// MapLegacyDto: ánh xạ FeatureMemberDto sang MemberDto cũ; giữ Guid role củ thông qua MapFeatureRole.
// MapLegacyRoleId / MapFeatureRole: bridge giữa MemberRole enum và Guid role củ (Contracts).
using System.Net;
using System.Net.Http.Headers;
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
    HttpClient httpClient,
    ITokenStorage tokenStorage,
    IAccessTokenStore accessTokenStore) : IMemberService
{
    private static readonly Guid LeaderRoleId = Guid.Parse("7e9e6f36-0508-4da8-adf1-c15b0cbe2f3a");
    private static readonly Guid CoreMemberRoleId = Guid.Parse("f80bb5dc-ab54-48fa-9453-869a12a81dc8");
    private static readonly Guid CollaboratorRoleId = Guid.Parse("1a96a5f4-6dfc-42db-936d-f86ee4e35ef5");

    private readonly HttpClient _httpClient = httpClient;
    private readonly ITokenStorage _tokenStorage = tokenStorage;
    private readonly IAccessTokenStore _accessTokenStore = accessTokenStore;

    public async Task<List<MemberDto>> GetMembers(Guid orgId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/organizations/{orgId}/members", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<FeatureGetMembersResponse>(cancellationToken: CancellationToken.None)
            ?? new FeatureGetMembersResponse([]);

        return payload.Items.Select(MapLegacyDto).ToList();
    }

    public async Task<MemberDto> CreateMember(Guid orgId, FeatureCreateMemberRequest req)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, $"api/organizations/{orgId}/members", CancellationToken.None);
        request.Content = JsonContent.Create(req);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<FeatureMemberDto>(cancellationToken: CancellationToken.None)
            ?? throw new InvalidOperationException("API returned no member payload.");

        return MapLegacyDto(created);
    }

    public async Task AssignRole(Guid memberId, Guid roleId)
    {
        var payload = new FeatureUpdateMemberRoleRequest(MapLegacyRoleId(roleId));
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/members/{memberId}/role", CancellationToken.None);
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();
    }

    public async Task AssignDepartment(Guid memberId, Guid departmentId)
    {
        var payload = new FeatureUpdateMemberDepartmentRequest(departmentId);
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/members/{memberId}/department", CancellationToken.None);
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMember(Guid memberId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/members/{memberId}", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> CanManageOrganizationMembersAsync(Guid orgId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/users/me/organizations", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<FeatureGetMyOrganizationsResponse>(cancellationToken: CancellationToken.None)
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
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, $"api/organizations/{orgId}/leave", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotSupportedException("Leave organization endpoint is not available in current backend.");
        }

        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string uri, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, uri);

        var token = await GetAccessTokenAsync(ct);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_accessTokenStore.AccessToken))
            return _accessTokenStore.AccessToken;

        try
        {
            var token = await _tokenStorage.GetTokenAsync(ct);
            if (!string.IsNullOrWhiteSpace(token))
                _accessTokenStore.AccessToken = token;

            return token;
        }
        catch (InvalidOperationException)
        {
            return null;
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
