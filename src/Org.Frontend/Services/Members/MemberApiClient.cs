using System.Net.Http.Json;
using Org.Shared.Contracts;

namespace Org.Frontend.Services.Members;

public sealed class MemberApiClient(HttpClient httpClient) : IMemberService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<MemberDto>> GetMembers(Guid orgId)
    {
        var data = await _httpClient.GetFromJsonAsync<List<MemberDto>>($"api/organizations/{orgId}/members");
        return data ?? [];
    }

    public async Task AssignRole(Guid memberId, Guid roleId)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/members/{memberId}/role", new AssignRoleRequest
        {
            RoleId = roleId
        });

        response.EnsureSuccessStatusCode();
    }

    public async Task AssignDepartment(Guid memberId, Guid departmentId)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/members/{memberId}/department", new AssignDepartmentRequest
        {
            DepartmentId = departmentId
        });

        response.EnsureSuccessStatusCode();
    }
}
