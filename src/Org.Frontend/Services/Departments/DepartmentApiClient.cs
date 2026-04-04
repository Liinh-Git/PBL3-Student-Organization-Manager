using System.Net.Http.Json;
using Org.Shared.Contracts;

namespace Org.Frontend.Services.Departments;

public sealed class DepartmentApiClient(HttpClient httpClient) : IDepartmentService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<DepartmentDto>> GetDepartments(Guid orgId)
    {
        var data = await _httpClient.GetFromJsonAsync<List<DepartmentDto>>($"api/organizations/{orgId}/departments");
        return data ?? [];
    }

    public async Task<DepartmentDto> CreateDepartment(CreateDepartmentRequest req)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/departments", req);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DepartmentDto>())
            ?? throw new InvalidOperationException("API returned no department payload.");
    }

    public async Task<DepartmentDto> UpdateDepartment(Guid id, UpdateDepartmentRequest req)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/departments/{id}", req);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DepartmentDto>())
            ?? throw new InvalidOperationException("API returned no department payload.");
    }
}
