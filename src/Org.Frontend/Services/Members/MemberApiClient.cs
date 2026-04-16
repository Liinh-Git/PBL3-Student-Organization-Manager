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

    public async Task<MemberDto> CreateMember(CreateMemberRequest req)
    {
        var payload = new FeatureCreateMemberRequest(
            req.DisplayName.Trim(),
            req.Email.Trim(),
            req.DepartmentId);

        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, $"api/organizations/{req.OrgId}/members", CancellationToken.None);
        request.Content = JsonContent.Create(payload);

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

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string uri, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, uri);

        var token = await GetAccessTokenAsync(ct);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

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
