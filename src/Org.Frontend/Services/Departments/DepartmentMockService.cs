// ---- Mock service phòng ban — CRUD trên in-memory MockDataStore ----
// Validate quan hệ FK (orgId, managerId) trước khi ghi vào data store.
// Xóa phòng ban: tự gán lại DepartmentId = null cho các member thuộc phòng này.
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;
using Org.Shared.Contracts;

namespace Org.Frontend.Services.Departments;

public sealed class DepartmentMockService(FrontendMockDataStore mockDataStore) : IDepartmentService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;

    public Task<List<DepartmentDto>> GetDepartments(Guid orgId)
    {
        return _mockDataStore.UseAsync(data => data.Departments
            .Where(x => x.OrgId == orgId)
            .OrderBy(x => x.DeptName, StringComparer.OrdinalIgnoreCase)
            .Select(MapDto)
            .ToList());
    }

    public Task<DepartmentDto> CreateDepartment(CreateDepartmentRequest req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            EnsureOrganizationExists(data, req.OrgId);

            var item = new MockDepartment
            {
                Id = Guid.NewGuid(),
                OrgId = req.OrgId,
                DeptName = NormalizeDepartmentName(req.DeptName),
                ManagerId = req.ManagerId,
                Function = NormalizeOptionalText(req.Function)
            };

            if (item.ManagerId.HasValue)
            {
                EnsureManagerBelongsToOrganization(data, item.ManagerId.Value, item.OrgId);
            }

            data.Departments.Add(item);
            return MapDto(item);
        });
    }

    public Task<DepartmentDto> UpdateDepartment(Guid id, UpdateDepartmentRequest req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var current = data.Departments.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Department {id} not found in mock data.");

            if (req.ManagerId.HasValue)
            {
                EnsureManagerBelongsToOrganization(data, req.ManagerId.Value, current.OrgId);
            }

            current.DeptName = NormalizeDepartmentName(req.DeptName);
            current.ManagerId = req.ManagerId;
            current.Function = NormalizeOptionalText(req.Function);
            return MapDto(current);
        });
    }

    public async Task DeleteDepartment(Guid id)
    {
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Departments.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Department {id} not found in mock data.");

            // Keep mock relations consistent after deletion.
            foreach (var member in data.Members.Where(x => x.DepartmentId == current.Id))
            {
                member.DepartmentId = null;
            }

            data.Departments.Remove(current);
            return 0;
        });
    }

    public Task<DepartmentTasksOverviewViewModel> GetTasksOverviewAsync(Guid departmentId)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
                ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");

            var membersInDepartment = data.Members
                .Where(x => x.OrgId == department.OrgId && x.DepartmentId == departmentId)
                .ToDictionary(x => x.Id, x => x.DisplayName);

            var taskItems = data.Tasks
                .Where(task => task.AssigneeMemberId.HasValue
                    && membersInDepartment.ContainsKey(task.AssigneeMemberId.Value))
                .OrderBy(task => GetTaskRank(task.Status))
                .ThenBy(task => task.DueDate ?? DateTime.MaxValue)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .Select(task => new DepartmentTaskItemViewModel
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    Status = NormalizeStatus(task.Status),
                    Priority = "Medium",
                    DueDate = task.DueDate,
                    AssigneeName = task.AssigneeMemberId.HasValue
                        ? membersInDepartment[task.AssigneeMemberId.Value]
                        : null
                })
                .ToList();

            var completedTaskCount = taskItems.Count(x => string.Equals(x.Status, "DONE", StringComparison.OrdinalIgnoreCase));
            var totalTasks = taskItems.Count;

            return new DepartmentTasksOverviewViewModel
            {
                DepartmentId = departmentId,
                TotalTasks = totalTasks,
                OpenTaskCount = Math.Max(0, totalTasks - completedTaskCount),
                CompletedTaskCount = completedTaskCount,
                Items = taskItems.Take(8).ToList()
            };
        });
    }

    private static DepartmentDto MapDto(MockDepartment source)
    {
        return new DepartmentDto
        {
            Id = source.Id,
            OrgId = source.OrgId,
            DeptName = source.DeptName,
            ManagerId = source.ManagerId,
            Function = source.Function
        };
    }

    private static void EnsureOrganizationExists(MockDataSet data, Guid organizationId)
    {
        if (!data.Organizations.Any(x => x.Id == organizationId))
        {
            throw new KeyNotFoundException($"Organization {organizationId} not found in mock data.");
        }
    }

    private static void EnsureManagerBelongsToOrganization(MockDataSet data, Guid managerMemberId, Guid organizationId)
    {
        var manager = data.Members.FirstOrDefault(x => x.Id == managerMemberId)
            ?? throw new KeyNotFoundException($"Member {managerMemberId} not found in mock data.");

        if (manager.OrgId != organizationId)
        {
            throw new InvalidOperationException("Department manager must belong to the same organization.");
        }
    }

    private static string NormalizeDepartmentName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Department name is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int GetTaskRank(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "IN_PROGRESS" => 0,
            "TODO" => 1,
            "DONE" => 2,
            _ => 3
        };
    }

    private static string NormalizeStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "IN_PROGRESS" => "IN_PROGRESS",
            "DONE" => "DONE",
            _ => "TODO"
        };
    }
}
