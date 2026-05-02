// ---- API client thực cho service phòng ban — gọn request qua HttpClient có Bearer token ----
// Dùng alias import để tách biệt DTO củ (Contracts/) và DTO từ Features/ của API thật.
// MapLegacyDto: chuyển FeatureDepartmentDto sang DepartmentDto cũ mà UI đang dùng.
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Frontend.ViewModels;
using Org.Shared.Contracts;
using FeatureCreateDepartmentRequest = Org.Shared.Features.Departments.CreateDepartmentRequest;
using FeatureDepartmentDto = Org.Shared.Features.Departments.DepartmentDto;
using FeatureGetDepartmentMembersResponse = Org.Shared.Features.Departments.GetDepartmentMembersResponse;
using FeatureGetDepartmentsResponse = Org.Shared.Features.Departments.GetDepartmentsResponse;
using FeatureGetDepartmentTasksOverviewResponse = Org.Shared.Features.Departments.GetDepartmentTasksOverviewResponse;
using FeatureMemberDto = Org.Shared.Features.Members.MemberDto;
using FeatureUpdateDepartmentManagerRequest = Org.Shared.Features.Departments.UpdateDepartmentManagerRequest;
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

    public async Task<List<MemberDto>> GetDepartmentMembersAsync(Guid departmentId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/departments/{departmentId}/members", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<FeatureGetDepartmentMembersResponse>(cancellationToken: CancellationToken.None)
            ?? new FeatureGetDepartmentMembersResponse([]);

        return payload.Items.Select(MapLegacyMemberDto).ToList();
    }

    public async Task<DepartmentDto> AssignManagerAsync(Guid departmentId, Guid? managerMemberId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/departments/{departmentId}/manager", CancellationToken.None);
        request.Content = JsonContent.Create(new FeatureUpdateDepartmentManagerRequest(managerMemberId));

        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<FeatureDepartmentDto>(cancellationToken: CancellationToken.None)
            ?? throw new InvalidOperationException("API returned no department payload.");

        return MapLegacyDto(updated);
    }

    public async Task AssignMemberAsync(Guid departmentId, Guid memberId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, $"api/departments/{departmentId}/members/{memberId}", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveMemberAsync(Guid departmentId, Guid memberId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Delete, $"api/departments/{departmentId}/members/{memberId}", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();
    }

    public Task<List<DepartmentTaskDto>> GetDepartmentTasksAsync(Guid departmentId)
    {
        throw new NotSupportedException(
            "Live/API mode does not yet expose dedicated organization-department task endpoints. Department task list is only fully supported in mock mode.");
    }

    public Task<DepartmentTaskDto> CreateDepartmentTaskAsync(Guid departmentId, CreateDepartmentTaskRequest request)
    {
        throw new NotSupportedException(
            "Live/API mode does not yet expose dedicated organization-department task create endpoint.");
    }

    public Task<DepartmentTaskDto> UpdateDepartmentTaskAsync(Guid taskId, UpdateDepartmentTaskRequest request)
    {
        throw new NotSupportedException(
            "Live/API mode does not yet expose dedicated organization-department task update endpoint.");
    }

    public Task DeleteDepartmentTaskAsync(Guid taskId)
    {
        throw new NotSupportedException(
            "Live/API mode does not yet expose dedicated organization-department task delete endpoint.");
    }

    public Task<DepartmentTaskDto> CompleteDepartmentTaskAsync(Guid taskId)
    {
        throw new NotSupportedException(
            "Live/API mode does not yet expose dedicated organization-department task completion endpoint.");
    }

    public async Task<DepartmentTasksOverviewViewModel> GetTasksOverviewAsync(Guid departmentId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/departments/{departmentId}/tasks/overview", CancellationToken.None);
        using var response = await _httpClient.SendAsync(request, CancellationToken.None);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<FeatureGetDepartmentTasksOverviewResponse>(cancellationToken: CancellationToken.None)
            ?? new FeatureGetDepartmentTasksOverviewResponse(departmentId, 0, 0, 0, []);

        return new DepartmentTasksOverviewViewModel
        {
            DepartmentId = payload.DepartmentId,
            TotalTasks = payload.TotalTasks,
            OpenTaskCount = payload.OpenTaskCount,
            CompletedTaskCount = payload.CompletedTaskCount,
            Items = payload.Items
                .Select(x => new DepartmentTaskItemViewModel
                {
                    TaskId = x.TaskId,
                    Title = x.Title,
                    Status = ToStatusText(x.Status),
                    Priority = ToPriorityText(x.Priority),
                    DueDate = x.DueDate?.ToDateTime(TimeOnly.MinValue),
                    AssigneeName = x.AssigneeName
                })
                .ToList()
        };
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

    private static MemberDto MapLegacyMemberDto(FeatureMemberDto source)
    {
        return new MemberDto
        {
            Id = source.Id,
            OrgId = source.OrganizationId,
            UserId = Guid.Empty,
            DisplayName = source.FullName,
            Email = source.Email,
            DepartmentId = source.DepartmentId,
            RoleId = null,
            RoleName = source.Role.ToString(),
            JoinDate = source.JoinedAtUtc.UtcDateTime
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

    private static string ToStatusText(Org.Shared.TaskStatus status)
    {
        return status switch
        {
            Org.Shared.TaskStatus.InProgress => "IN_PROGRESS",
            Org.Shared.TaskStatus.Done => "DONE",
            _ => "TODO"
        };
    }

    private static string ToPriorityText(Org.Shared.TaskPriority priority)
    {
        return priority switch
        {
            Org.Shared.TaskPriority.High => "High",
            Org.Shared.TaskPriority.Low => "Low",
            _ => "Medium"
        };
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
