using System.Text.Json;
using Org.Shared.Contracts;
using FeatureCreateMemberRequest = Org.Shared.Features.Members.CreateMemberRequest;

namespace Org.Frontend.Services.Members;

public sealed class MemberMockService(IWebHostEnvironment env) : IMemberService
{
    private readonly IWebHostEnvironment _env = env;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private List<MemberDto>? _cache;

    public async Task<List<MemberDto>> GetMembers(Guid orgId)
    {
        var all = await LoadAll();
        return all.Where(x => x.OrgId == orgId).OrderBy(x => x.DisplayName).ToList();
    }

    public async Task<MemberDto> CreateMember(Guid orgId, FeatureCreateMemberRequest req)
    {
        var all = await LoadAll();

        var dto = new MemberDto
        {
            Id = Guid.NewGuid(),
            OrgId = orgId,
            UserId = Guid.NewGuid(),
            DisplayName = req.FullName.Trim(),
            DepartmentId = req.DepartmentId,
            RoleId = null,
            JoinDate = DateTime.UtcNow
        };

        all.Add(dto);
        return dto;
    }

    public async Task AssignRole(Guid memberId, Guid roleId)
    {
        var all = await LoadAll();
        var current = all.FirstOrDefault(x => x.Id == memberId)
            ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");

        current.RoleId = roleId;
    }

    public async Task AssignDepartment(Guid memberId, Guid departmentId)
    {
        var all = await LoadAll();
        var current = all.FirstOrDefault(x => x.Id == memberId)
            ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");

        current.DepartmentId = departmentId;
    }

    public async Task DeleteMember(Guid memberId)
    {
        var all = await LoadAll();
        var removed = all.RemoveAll(x => x.Id == memberId);
        if (removed == 0)
            throw new KeyNotFoundException($"Member {memberId} not found in mock data.");
    }

    private async Task<List<MemberDto>> LoadAll()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var path = Path.Combine(_env.ContentRootPath, "Services", "Mocks", "Data", "members.mock.json");
        if (!File.Exists(path))
        {
            _cache = [];
            return _cache;
        }

        await using var stream = File.OpenRead(path);
        var data = await JsonSerializer.DeserializeAsync<List<MemberDto>>(stream, _jsonOptions);
        _cache = data ?? [];
        return _cache;
    }
}
