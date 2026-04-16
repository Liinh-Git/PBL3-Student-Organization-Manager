using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Shared.Contracts;
using FeatureCreateDepartmentRequest = Org.Shared.Features.Departments.CreateDepartmentRequest;
using FeatureDepartmentDto = Org.Shared.Features.Departments.DepartmentDto;
using FeatureGetDepartmentsResponse = Org.Shared.Features.Departments.GetDepartmentsResponse;
using FeatureUpdateDepartmentRequest = Org.Shared.Features.Departments.UpdateDepartmentRequest;

namespace Org.Frontend.Services.Departments;

public sealed class DepartmentApiClient(
    HttpClient httpClient,
    ITokenStorage tokenStorage,
    IAccessTokenStore accessTokenStore) : IDepartmentService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ITokenStorage _tokenStorage = tokenStorage;
    private readonly IAccessTokenStore _accessTokenStore = accessTokenStore;

    public async Task<List<DepartmentDto>> GetDepartments(Guid orgId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/organizations/{orgId}/departments", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<FeatureGetDepartmentsResponse>(cancellationToken: CancellationToken.None)
            ?? new FeatureGetDepartmentsResponse([]);

        return payload.Items.Select(MapLegacyDto).ToList();
    }

    public async Task<DepartmentDto> CreateDepartment(CreateDepartmentRequest req)
    {
        var payload = new FeatureCreateDepartmentRequest(
            req.OrgId,
            BuildCode(req.DeptName),
            req.DeptName.Trim(),
            req.Function,
            req.ManagerId);

        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/departments", CancellationToken.None);
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<FeatureDepartmentDto>(cancellationToken: CancellationToken.None)
            ?? throw new InvalidOperationException("API returned no department payload.");

        return MapLegacyDto(created);
    }

    public async Task<DepartmentDto> UpdateDepartment(Guid id, UpdateDepartmentRequest req)
    {
        var payload = new FeatureUpdateDepartmentRequest(
            BuildCode(req.DeptName),
            req.DeptName.Trim(),
            req.Function,
            true,
            req.ManagerId);

        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/departments/{id}", CancellationToken.None);
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<FeatureDepartmentDto>(cancellationToken: CancellationToken.None)
            ?? throw new InvalidOperationException("API returned no department payload.");

        return MapLegacyDto(updated);
    }

    public async Task DeleteDepartment(Guid id)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/departments/{id}", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();
    }

    private static DepartmentDto MapLegacyDto(FeatureDepartmentDto source)
    {
        return new DepartmentDto
        {
            Id = source.Id,
            OrgId = source.OrganizationId,
            DeptName = source.Name,
            ManagerId = source.ManagerMemberId,
            Function = source.Description
        };
    }

    private static string BuildCode(string? departmentName)
    {
        if (string.IsNullOrWhiteSpace(departmentName))
            return "DEPT";

        var compact = new string(departmentName.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(compact))
            return "DEPT";

        var code = compact.Length <= 8 ? compact : compact[..8];
        return code.ToUpperInvariant();
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
}
