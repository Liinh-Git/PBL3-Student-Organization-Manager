using System.Text.Json;
using Org.Shared.Contracts;

namespace Org.Frontend.Services.Departments;

public sealed class DepartmentMockService(IWebHostEnvironment env) : IDepartmentService
{
    private readonly IWebHostEnvironment _env = env;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private List<DepartmentDto>? _cache;

    public async Task<List<DepartmentDto>> GetDepartments(Guid orgId)
    {
        var all = await LoadAll();
        return all.Where(x => x.OrgId == orgId).OrderBy(x => x.DeptName).ToList();
    }

    public async Task<DepartmentDto> CreateDepartment(CreateDepartmentRequest req)
    {
        var all = await LoadAll();
        var dto = new DepartmentDto
        {
            Id = Guid.NewGuid(),
            OrgId = req.OrgId,
            DeptName = req.DeptName.Trim(),
            ManagerId = req.ManagerId,
            Function = req.Function
        };

        all.Add(dto);
        return dto;
    }

    public async Task<DepartmentDto> UpdateDepartment(Guid id, UpdateDepartmentRequest req)
    {
        var all = await LoadAll();
        var current = all.FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Department {id} not found in mock data.");

        current.DeptName = req.DeptName.Trim();
        current.ManagerId = req.ManagerId;
        current.Function = req.Function;
        return current;
    }

    public async Task DeleteDepartment(Guid id)
    {
        var all = await LoadAll();
        var removed = all.RemoveAll(x => x.Id == id);
        if (removed == 0)
            throw new KeyNotFoundException($"Department {id} not found in mock data.");
    }

    private async Task<List<DepartmentDto>> LoadAll()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var path = Path.Combine(_env.ContentRootPath, "Services", "Mocks", "Data", "departments.mock.json");
        if (!File.Exists(path))
        {
            _cache = [];
            return _cache;
        }

        await using var stream = File.OpenRead(path);
        var data = await JsonSerializer.DeserializeAsync<List<DepartmentDto>>(stream, _jsonOptions);
        _cache = data ?? [];
        return _cache;
    }
}
