using System.Net.Http.Json;
using Org.Shared.Contracts;
using FeatureCreateDepartmentRequest = Org.Shared.Features.Departments.CreateDepartmentRequest;
using FeatureDepartmentDto = Org.Shared.Features.Departments.DepartmentDto;
using FeatureGetDepartmentsResponse = Org.Shared.Features.Departments.GetDepartmentsResponse;
using FeatureUpdateDepartmentRequest = Org.Shared.Features.Departments.UpdateDepartmentRequest;

namespace Org.Frontend.Services.Departments;

public sealed class DepartmentApiClient(
    HttpClient httpClient) : IDepartmentService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<DepartmentDto>> GetDepartments(Guid orgId)
    {
        var payload = await _httpClient.GetFromJsonAsync<FeatureGetDepartmentsResponse>($"api/organizations/{orgId}/departments", CancellationToken.None)
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

        using var response = await _httpClient.PostAsJsonAsync("api/departments", payload, CancellationToken.None);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<FeatureDepartmentDto>()
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

        using var response = await _httpClient.PutAsJsonAsync($"api/departments/{id}", payload, CancellationToken.None);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<FeatureDepartmentDto>()
            ?? throw new InvalidOperationException("API returned no department payload.");

        return MapLegacyDto(updated);
    }

    public async Task DeleteDepartment(Guid id)
    {
        using var response = await _httpClient.DeleteAsync($"api/departments/{id}", CancellationToken.None);
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
}
