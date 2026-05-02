// ---- Mock service phòng ban — CRUD trên in-memory MockDataStore ----
// Validate quan hệ FK (orgId, managerId) trước khi ghi vào data store.
// Xóa phòng ban: tự gán lại DepartmentId = null cho các member thuộc phòng này.
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;
using Org.Shared.Contracts;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Org.Frontend.Services.Departments;

public sealed class DepartmentMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IDepartmentService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<List<DepartmentDto>> GetDepartments(Guid orgId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureCanReadOrganization(data, currentUserId, orgId);

            return data.Departments
                .Where(x => x.OrgId == orgId)
                .OrderBy(x => x.DeptName, StringComparer.OrdinalIgnoreCase)
                .Select(MapDto)
                .ToList();
        });
    }

    public async Task<DepartmentDto> CreateDepartment(CreateDepartmentRequest req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureOrganizationExists(data, req.OrgId);
            EnsureCanManageMembers(data, currentUserId, req.OrgId);

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

    public async Task<DepartmentDto> UpdateDepartment(Guid id, UpdateDepartmentRequest req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            var current = data.Departments.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Department {id} not found in mock data.");
            EnsureCanManageMembers(data, currentUserId, current.OrgId);

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
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Departments.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Department {id} not found in mock data.");
            EnsureCanManageMembers(data, currentUserId, current.OrgId);

            // Keep mock relations consistent after deletion.
            foreach (var member in data.Members.Where(x => x.DepartmentId == current.Id))
            {
                member.DepartmentId = null;
            }

            data.DepartmentTasks.RemoveAll(x => x.DepartmentId == current.Id);
            data.Departments.Remove(current);
            return 0;
        });
    }

    public async Task<List<MemberDto>> GetDepartmentMembersAsync(Guid departmentId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
                ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");

            EnsureCanReadOrganization(data, currentUserId, department.OrgId);

            return data.Members
                .Where(x => x.OrgId == department.OrgId && x.DepartmentId == departmentId)
                .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(member => MapMemberDto(member, data))
                .ToList();
        });
    }

    public async Task<DepartmentDto> AssignManagerAsync(Guid departmentId, Guid? managerMemberId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
                ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");
            EnsureCanManageMembers(data, currentUserId, department.OrgId);

            if (managerMemberId.HasValue)
            {
                EnsureManagerBelongsToOrganization(data, managerMemberId.Value, department.OrgId);
                var managerMember = data.Members.First(x => x.Id == managerMemberId.Value);
                managerMember.DepartmentId = departmentId;
            }

            department.ManagerId = managerMemberId;
            return MapDto(department);
        });
    }

    public async Task AssignMemberAsync(Guid departmentId, Guid memberId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
                ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");
            EnsureCanManageMembers(data, currentUserId, department.OrgId);

            var member = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");
            if (member.OrgId != department.OrgId)
            {
                throw new InvalidOperationException("Department member must belong to the same organization.");
            }

            member.DepartmentId = departmentId;
            return 0;
        });
    }

    public async Task RemoveMemberAsync(Guid departmentId, Guid memberId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
                ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");
            EnsureCanManageMembers(data, currentUserId, department.OrgId);

            var member = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");
            if (member.OrgId != department.OrgId)
            {
                throw new InvalidOperationException("Department member must belong to the same organization.");
            }

            if (department.ManagerId == memberId)
            {
                department.ManagerId = null;
            }

            if (member.DepartmentId == departmentId)
            {
                member.DepartmentId = null;
            }

            foreach (var task in data.DepartmentTasks.Where(x => x.DepartmentId == departmentId))
            {
                task.AssigneeMemberIds.RemoveAll(x => x == memberId);
            }

            return 0;
        });
    }

    public async Task<List<DepartmentTaskDto>> GetDepartmentTasksAsync(Guid departmentId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
                ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");
            EnsureCanReadOrganization(data, currentUserId, department.OrgId);

            return data.DepartmentTasks
                .Where(x => x.DepartmentId == departmentId)
                .OrderBy(x => x.DeadlineAt ?? DateTime.MaxValue)
                .ThenByDescending(x => x.CreatedAt)
                .Select(MapDepartmentTaskDto)
                .ToList();
        });
    }

    public async Task<DepartmentTaskDto> CreateDepartmentTaskAsync(Guid departmentId, CreateDepartmentTaskRequest request)
    {
        var currentUserId = await TryGetCurrentUserIdAsync()
            ?? throw new UnauthorizedAccessException("User is not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
                ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");

            var actorMember = ResolveOrganizationMember(data, department.OrgId, currentUserId)
                ?? throw new UnauthorizedAccessException("Current user is not a member of this organization.");
            EnsureCanManageDepartmentTasks(data, department, actorMember);

            var task = new MockDepartmentTask
            {
                Id = Guid.NewGuid(),
                OrganizationId = department.OrgId,
                DepartmentId = department.Id,
                Title = NormalizeDepartmentTaskTitle(request.Title),
                Description = NormalizeOptionalText(request.Description),
                DeadlineAt = request.DeadlineAt,
                Status = "TODO",
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                AssigneeMemberIds = NormalizeDepartmentTaskAssignees(data, department, request.AssigneeMemberIds)
            };

            data.DepartmentTasks.Add(task);
            return MapDepartmentTaskDto(task);
        });
    }

    public async Task<DepartmentTaskDto> UpdateDepartmentTaskAsync(Guid taskId, UpdateDepartmentTaskRequest request)
    {
        var currentUserId = await TryGetCurrentUserIdAsync()
            ?? throw new UnauthorizedAccessException("User is not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var task = data.DepartmentTasks.FirstOrDefault(x => x.Id == taskId)
                ?? throw new KeyNotFoundException($"Department task {taskId} not found in mock data.");
            var department = data.Departments.FirstOrDefault(x => x.Id == task.DepartmentId)
                ?? throw new InvalidOperationException($"Department {task.DepartmentId} not found for task {taskId}.");

            var actorMember = ResolveOrganizationMember(data, department.OrgId, currentUserId)
                ?? throw new UnauthorizedAccessException("Current user is not a member of this organization.");
            EnsureCanManageDepartmentTasks(data, department, actorMember);

            task.Title = NormalizeDepartmentTaskTitle(request.Title);
            task.Description = NormalizeOptionalText(request.Description);
            task.DeadlineAt = request.DeadlineAt;
            task.Status = NormalizeDepartmentTaskStatus(request.Status);
            task.AssigneeMemberIds = NormalizeDepartmentTaskAssignees(data, department, request.AssigneeMemberIds);
            return MapDepartmentTaskDto(task);
        });
    }

    public async Task DeleteDepartmentTaskAsync(Guid taskId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync()
            ?? throw new UnauthorizedAccessException("User is not logged in.");

        await _mockDataStore.UseAsync(data =>
        {
            var task = data.DepartmentTasks.FirstOrDefault(x => x.Id == taskId)
                ?? throw new KeyNotFoundException($"Department task {taskId} not found in mock data.");
            var department = data.Departments.FirstOrDefault(x => x.Id == task.DepartmentId)
                ?? throw new InvalidOperationException($"Department {task.DepartmentId} not found for task {taskId}.");

            var actorMember = ResolveOrganizationMember(data, department.OrgId, currentUserId)
                ?? throw new UnauthorizedAccessException("Current user is not a member of this organization.");
            EnsureCanManageDepartmentTasks(data, department, actorMember);

            data.DepartmentTasks.Remove(task);
            return 0;
        });
    }

    public async Task<DepartmentTaskDto> CompleteDepartmentTaskAsync(Guid taskId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync()
            ?? throw new UnauthorizedAccessException("User is not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var task = data.DepartmentTasks.FirstOrDefault(x => x.Id == taskId)
                ?? throw new KeyNotFoundException($"Department task {taskId} not found in mock data.");
            var department = data.Departments.FirstOrDefault(x => x.Id == task.DepartmentId)
                ?? throw new InvalidOperationException($"Department {task.DepartmentId} not found for task {taskId}.");

            var actorMember = ResolveOrganizationMember(data, department.OrgId, currentUserId)
                ?? throw new UnauthorizedAccessException("Current user is not a member of this organization.");

            var canManage = CanManageDepartmentTasks(data, department, actorMember);
            var isAssigned = task.AssigneeMemberIds.Contains(actorMember.Id);
            if (!canManage && !isAssigned)
            {
                throw new UnauthorizedAccessException("Only assignees, department manager, or members with org.members.manage can complete this task.");
            }

            task.Status = "DONE";
            return MapDepartmentTaskDto(task);
        });
    }

    public async Task<DepartmentTasksOverviewViewModel> GetTasksOverviewAsync(Guid departmentId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
                ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");
            EnsureCanReadOrganization(data, currentUserId, department.OrgId);

            var membersInDepartment = data.Members
                .Where(x => x.OrgId == department.OrgId && x.DepartmentId == departmentId)
                .ToDictionary(x => x.Id, x => x.DisplayName);

            var taskItems = data.DepartmentTasks
                .Where(task => task.DepartmentId == departmentId)
                .OrderBy(task => GetTaskRank(task.Status))
                .ThenBy(task => task.DeadlineAt ?? DateTime.MaxValue)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .Select(task => new DepartmentTaskItemViewModel
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    Status = NormalizeStatus(task.Status),
                    Priority = "Medium",
                    DueDate = task.DeadlineAt,
                    AssigneeName = task.AssigneeMemberIds
                        .Select(memberId => membersInDepartment.TryGetValue(memberId, out var name) ? name : null)
                        .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name))
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

    private static MemberDto MapMemberDto(MockMember source, MockDataSet data)
    {
        var email = data.Users.FirstOrDefault(x => x.Id == source.UserId)?.Email ?? string.Empty;
        var roleName = data.OrganizationRoles.FirstOrDefault(x => x.Id == source.RoleId)?.RoleName;

        return new MemberDto
        {
            Id = source.Id,
            OrgId = source.OrgId,
            UserId = source.UserId,
            DisplayName = source.DisplayName,
            Email = email,
            DepartmentId = source.DepartmentId,
            RoleId = source.RoleId,
            RoleName = roleName,
            JoinDate = source.JoinDate
        };
    }

    private static DepartmentTaskDto MapDepartmentTaskDto(MockDepartmentTask source)
    {
        return new DepartmentTaskDto
        {
            Id = source.Id,
            OrganizationId = source.OrganizationId,
            DepartmentId = source.DepartmentId,
            Title = source.Title,
            Description = source.Description,
            DeadlineAt = source.DeadlineAt,
            Status = NormalizeDepartmentTaskStatus(source.Status),
            CreatedByUserId = source.CreatedByUserId,
            CreatedAt = source.CreatedAt,
            AssigneeMemberIds = source.AssigneeMemberIds.Distinct().ToList()
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

    private static void EnsureCanReadOrganization(MockDataSet data, Guid? userId, Guid organizationId)
    {
        if (!HasAnyPermission(data, userId, organizationId, "org.workspace.access", "org.overview.read", "org.members.manage"))
        {
            throw new UnauthorizedAccessException("You do not have permission to read department data.");
        }
    }

    private static void EnsureCanManageMembers(MockDataSet data, Guid? userId, Guid organizationId)
    {
        if (!HasAnyPermission(data, userId, organizationId, "org.members.manage"))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage departments or members.");
        }
    }

    private static MockMember? ResolveOrganizationMember(MockDataSet data, Guid organizationId, Guid userId)
    {
        return data.Members.FirstOrDefault(x => x.OrgId == organizationId && x.UserId == userId);
    }

    private static bool CanManageDepartmentTasks(MockDataSet data, MockDepartment department, MockMember actorMember)
    {
        if (department.ManagerId == actorMember.Id)
        {
            return true;
        }

        return HasAnyPermission(data, actorMember.UserId, department.OrgId, "org.members.manage", "org.tasks.manage");
    }

    private static void EnsureCanManageDepartmentTasks(MockDataSet data, MockDepartment department, MockMember actorMember)
    {
        if (!CanManageDepartmentTasks(data, department, actorMember))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage tasks for this department.");
        }
    }

    private static List<Guid> NormalizeDepartmentTaskAssignees(
        MockDataSet data,
        MockDepartment department,
        IEnumerable<Guid>? assigneeMemberIds)
    {
        var result = (assigneeMemberIds ?? [])
            .Distinct()
            .ToList();

        foreach (var memberId in result)
        {
            var member = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new InvalidOperationException($"Assignee member {memberId} not found.");
            if (member.OrgId != department.OrgId)
            {
                throw new InvalidOperationException("Assignee member must belong to the same organization.");
            }

            if (member.DepartmentId.HasValue && member.DepartmentId.Value != department.Id)
            {
                throw new InvalidOperationException("Assignee member must belong to the same department.");
            }
        }

        return result;
    }

    private static bool HasAnyPermission(MockDataSet data, Guid? userId, Guid organizationId, params string[] expectedPermissions)
    {
        if (!userId.HasValue)
        {
            return false;
        }

        var member = data.Members.FirstOrDefault(x => x.OrgId == organizationId && x.UserId == userId.Value);
        if (member is null || !member.RoleId.HasValue)
        {
            return false;
        }

        var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == member.RoleId.Value && x.OrgId == organizationId);
        if (role is null)
        {
            return false;
        }

        var permissions = role.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return expectedPermissions.Any(permissions.Contains);
    }

    private async Task<Guid?> TryGetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var rawUserId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
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
            "TODO" => 0,
            "DONE" => 1,
            _ => 3
        };
    }

    private static string NormalizeStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "DONE" => "DONE",
            _ => "TODO"
        };
    }

    private static string NormalizeDepartmentTaskTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Department task title is required.");
        }

        return title.Trim();
    }

    private static string NormalizeDepartmentTaskStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "DONE" => "DONE",
            _ => "TODO"
        };
    }
}
