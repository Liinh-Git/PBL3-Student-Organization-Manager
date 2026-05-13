using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Tasks.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Tasks;
using DomainTaskStatus = Org.Backend.Domain.Enums.TaskStatus;

namespace Org.Backend.Features.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskDto>> GetDepartmentTasksAsync(Guid orgId, Guid departmentId, Guid userId, CancellationToken ct = default)
    {
        await VerifyMembershipAsync(orgId, userId, ct);

        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        if (department == null || department.OrgId != orgId)
        {
            throw new KeyNotFoundException("Department not found");
        }

        var tasks = await _context.OrgTasks
            .Include(t => t.Assignee).ThenInclude(a => a!.User)
            .Include(t => t.Department)
            .Include(t => t.CreatedByMember).ThenInclude(cb => cb!.User)
            .Include(t => t.EventCategory).ThenInclude(c => c.Milestone).ThenInclude(m => m.Event)
            .Where(t => t.DeptId == departmentId && t.EventCategory.Milestone.Event.OrgId == orgId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        return tasks.Select(t => t.ToTaskDto()).ToList();
    }

    public async Task<TaskDto> CreateDepartmentTaskAsync(Guid orgId, Guid departmentId, CreateDepartmentTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        await VerifyMembershipAsync(orgId, userId, ct);
        await VerifyDepartmentTaskCreatePermissionAsync(orgId, userId, departmentId, ct);

        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        if (department == null || department.OrgId != orgId)
        {
            throw new KeyNotFoundException("Department not found");
        }

        Guid? categoryId = request.CategoryId;

        if (!categoryId.HasValue)
        {
            categoryId = await _context.EventCategories
                .Include(c => c.Milestone)
                    .ThenInclude(m => m.Event)
                .Where(c => c.OwnerDepartmentId == departmentId && c.Milestone.Event.OrgId == orgId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(ct);
        }

        if (!categoryId.HasValue)
        {
            categoryId = await _context.EventCategories
                .Include(c => c.Milestone)
                    .ThenInclude(m => m.Event)
                .Where(c => c.Milestone.Event.OrgId == orgId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(ct);
        }

        if (!categoryId.HasValue)
        {
            throw new InvalidOperationException("No event category available for this organization to attach department task");
        }

        var payload = request.Task with { DeptId = departmentId };
        return await CreateTaskInternalAsync(categoryId.Value, payload, userId, ct, skipTaskPermissionCheck: true);
    }

    public async Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        var task = await _context.OrgTasks
            .Include(t => t.EventCategory)
                .ThenInclude(c => c.Milestone)
                    .ThenInclude(m => m.Event)
            .Include(t => t.Assignee)
                .ThenInclude(a => a!.User)
            .Include(t => t.Department)
            .Include(t => t.CreatedByMember)
                .ThenInclude(cb => cb!.User)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        await VerifyMembershipAsync(task.EventCategory.Milestone.Event.OrgId, userId, ct);

        return task.ToTaskDto();
    }

    public async Task<TaskDto> CreateTaskAsync(Guid categoryId, CreateTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        return await CreateTaskInternalAsync(categoryId, request, userId, ct, skipTaskPermissionCheck: false);
    }

    private async Task<TaskDto> CreateTaskInternalAsync(Guid categoryId, CreateTaskRequest request, Guid userId, CancellationToken ct, bool skipTaskPermissionCheck)
    {
        // Get category and verify membership + permission
        var category = await _context.EventCategories
            .Include(c => c.Milestone)
                .ThenInclude(m => m.Event)
            .FirstOrDefaultAsync(c => c.Id == categoryId, ct);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        var orgId = category.Milestone.Event.OrgId;
        await VerifyMembershipAsync(orgId, userId, ct);
        if (!skipTaskPermissionCheck)
        {
            await VerifyTaskManagementPermissionAsync(orgId, userId, request.DeptId, ct);
        }

        // Get current user's member record
        var currentMember = await _context.Members
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (currentMember == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        // Verify assignee if provided
        if (request.AssigneeId.HasValue)
        {
            var assignee = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == request.AssigneeId.Value, ct);

            if (assignee == null)
            {
                throw new KeyNotFoundException("Assignee not found");
            }

            if (assignee.OrgId != orgId)
            {
                throw new InvalidOperationException("Assignee must belong to the same organization");
            }

            if (assignee.Status != MemberStatus.Active)
            {
                throw new InvalidOperationException("Assignee must be an active member");
            }

            if (request.DeptId.HasValue && assignee.DepartmentId != request.DeptId.Value)
            {
                throw new InvalidOperationException("Assignee must belong to the same department");
            }
        }

        // Verify department if provided
        if (request.DeptId.HasValue)
        {
            var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.DeptId.Value, ct);
            if (dept == null)
            {
                throw new KeyNotFoundException("Department not found");
            }
            if (dept.OrgId != orgId)
            {
                throw new InvalidOperationException("Department must belong to the same organization");
            }
        }

        // Parse priority
        var priority = TaskPriority.Medium;
        if (!string.IsNullOrEmpty(request.Priority) && !Enum.TryParse<TaskPriority>(request.Priority, out priority))
        {
            throw new ArgumentException($"Invalid priority: {request.Priority}");
        }

        // Convert to UTC if provided
        var utcDeadline = request.Deadline.HasValue ? DateTime.SpecifyKind(request.Deadline.Value, DateTimeKind.Utc) : (DateTime?)null;

        var task = new OrgTask
        {
            Id = Guid.NewGuid(),
            EventCategoryId = categoryId,
            TaskName = request.TaskName,
            Description = request.Description,
            AssigneeId = request.AssigneeId,
            DeptId = request.DeptId,
            Deadline = utcDeadline,
            Priority = priority,
            Status = DomainTaskStatus.Todo,
            Note = request.Note,
            CreatedByMemberId = currentMember.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrgTasks.Add(task);
        await _context.SaveChangesAsync(ct);
        await NotifyTaskAssignmentAsync(task, userId, isNewTask: true, ct);

        // Reload with navigation properties
        task = await _context.OrgTasks
            .Include(t => t.Assignee)
                .ThenInclude(a => a!.User)
            .Include(t => t.Department)
            .Include(t => t.CreatedByMember)
                .ThenInclude(cb => cb!.User)
            .FirstAsync(t => t.Id == task.Id, ct);

        return task.ToTaskDto();
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        var task = await _context.OrgTasks
            .Include(t => t.EventCategory)
                .ThenInclude(c => c.Milestone)
                    .ThenInclude(m => m.Event)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = task.EventCategory.Milestone.Event.OrgId;
        await VerifyMembershipAsync(orgId, userId, ct);
        await VerifyTaskManagementPermissionAsync(orgId, userId, task.DeptId ?? request.DeptId, ct);

        // Verify assignee if provided
        if (request.AssigneeId.HasValue)
        {
            var assignee = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == request.AssigneeId.Value, ct);

            if (assignee == null)
            {
                throw new KeyNotFoundException("Assignee not found");
            }

            if (assignee.OrgId != orgId)
            {
                throw new InvalidOperationException("Assignee must belong to the same organization");
            }

            if (assignee.Status != MemberStatus.Active)
            {
                throw new InvalidOperationException("Assignee must be an active member");
            }

            var effectiveDeptId = request.DeptId ?? task.DeptId;
            if (effectiveDeptId.HasValue && assignee.DepartmentId != effectiveDeptId.Value)
            {
                throw new InvalidOperationException("Assignee must belong to the same department");
            }
        }

        // Verify department if provided
        if (request.DeptId.HasValue)
        {
            var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.DeptId.Value, ct);
            if (dept == null)
            {
                throw new KeyNotFoundException("Department not found");
            }
            if (dept.OrgId != orgId)
            {
                throw new InvalidOperationException("Department must belong to the same organization");
            }
        }

        // Parse priority
        if (!string.IsNullOrEmpty(request.Priority))
        {
            if (!Enum.TryParse<TaskPriority>(request.Priority, out var priority))
            {
                throw new ArgumentException($"Invalid priority: {request.Priority}");
            }
            task.Priority = priority;
        }

        // Parse status
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (!Enum.TryParse<DomainTaskStatus>(request.Status, out var status))
            {
                throw new ArgumentException($"Invalid status: {request.Status}");
            }
            task.Status = status;

            // Set CompletedAt if status is Done
            if (status == DomainTaskStatus.Done && !task.CompletedAt.HasValue)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else if (status != DomainTaskStatus.Done)
            {
                task.CompletedAt = null;
            }
        }

        // Convert to UTC if provided
        var utcDeadline = request.Deadline.HasValue ? DateTime.SpecifyKind(request.Deadline.Value, DateTimeKind.Utc) : (DateTime?)null;

        task.TaskName = request.TaskName;
        task.Description = request.Description;
        task.AssigneeId = request.AssigneeId;
        task.DeptId = request.DeptId;
        task.Deadline = utcDeadline;
        task.Note = request.Note;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        await NotifyTaskAssignmentAsync(task, userId, isNewTask: false, ct);

        // Reload with navigation properties
        task = await _context.OrgTasks
            .Include(t => t.Assignee)
                .ThenInclude(a => a!.User)
            .Include(t => t.Department)
            .Include(t => t.CreatedByMember)
                .ThenInclude(cb => cb!.User)
            .FirstAsync(t => t.Id == taskId, ct);

        return task.ToTaskDto();
    }

    public async Task DeleteTaskAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        var task = await _context.OrgTasks
            .Include(t => t.EventCategory)
                .ThenInclude(c => c.Milestone)
                    .ThenInclude(m => m.Event)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = task.EventCategory.Milestone.Event.OrgId;
        await VerifyMembershipAsync(orgId, userId, ct);
        await VerifyTaskManagementPermissionAsync(orgId, userId, task.DeptId, ct);

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        await NotifyTaskStatusChangedAsync(task, userId, ct);
    }

    public async Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request, Guid userId, CancellationToken ct = default)
    {
        var task = await _context.OrgTasks
            .Include(t => t.EventCategory)
                .ThenInclude(c => c.Milestone)
                    .ThenInclude(m => m.Event)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = task.EventCategory.Milestone.Event.OrgId;
        await VerifyMembershipAsync(orgId, userId, ct);
        await VerifyTaskManagementPermissionAsync(orgId, userId, task.DeptId, ct);

        // Parse status
        if (!Enum.TryParse<DomainTaskStatus>(request.Status, out var status))
        {
            throw new ArgumentException($"Invalid status: {request.Status}");
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        // Set CompletedAt if status is Done
        if (status == DomainTaskStatus.Done && !task.CompletedAt.HasValue)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (status != DomainTaskStatus.Done)
        {
            task.CompletedAt = null;
        }

        await _context.SaveChangesAsync(ct);
        await NotifyTaskAssignmentAsync(task, userId, isNewTask: false, ct);

        // Reload with navigation properties
        task = await _context.OrgTasks
            .Include(t => t.Assignee)
                .ThenInclude(a => a!.User)
            .Include(t => t.Department)
            .Include(t => t.CreatedByMember)
                .ThenInclude(cb => cb!.User)
            .FirstAsync(t => t.Id == taskId, ct);

        return task.ToTaskDto();
    }

    public async Task<TaskDto> AssignTaskAsync(Guid taskId, AssignTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        var task = await _context.OrgTasks
            .Include(t => t.EventCategory)
                .ThenInclude(c => c.Milestone)
                    .ThenInclude(m => m.Event)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = task.EventCategory.Milestone.Event.OrgId;
        await VerifyMembershipAsync(orgId, userId, ct);
        await VerifyTaskManagementPermissionAsync(orgId, userId, task.DeptId ?? request.DeptId, ct);

        // Verify assignee if provided
        if (request.AssigneeId.HasValue)
        {
            var assignee = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == request.AssigneeId.Value, ct);

            if (assignee == null)
            {
                throw new KeyNotFoundException("Assignee not found");
            }

            if (assignee.OrgId != orgId)
            {
                throw new InvalidOperationException("Assignee must belong to the same organization");
            }

            if (assignee.Status != MemberStatus.Active)
            {
                throw new InvalidOperationException("Assignee must be an active member");
            }
        }

        // Verify department if provided
        if (request.DeptId.HasValue)
        {
            var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.DeptId.Value, ct);
            if (dept == null)
            {
                throw new KeyNotFoundException("Department not found");
            }
            if (dept.OrgId != orgId)
            {
                throw new InvalidOperationException("Department must belong to the same organization");
            }
        }

        task.AssigneeId = request.AssigneeId;
        task.DeptId = request.DeptId;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        // Reload with navigation properties
        task = await _context.OrgTasks
            .Include(t => t.Assignee)
                .ThenInclude(a => a!.User)
            .Include(t => t.Department)
            .Include(t => t.CreatedByMember)
                .ThenInclude(cb => cb!.User)
            .FirstAsync(t => t.Id == taskId, ct);

        return task.ToTaskDto();
    }

    private async Task VerifyMembershipAsync(Guid orgId, Guid userId, CancellationToken ct)
    {
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }
    }

    private async Task VerifyPermissionAsync(Guid orgId, Guid userId, string permissionKey, CancellationToken ct)
    {
        var hasPermission = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active)
            .SelectMany(m => m.Role!.RolePermissions.Select(rp => rp.Permission.PermissionKey))
            .AnyAsync(key => key == permissionKey, ct);

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException($"You do not have permission: {permissionKey}");
        }
    }

    private async Task NotifyTaskAssignmentAsync(OrgTask task, Guid actorUserId, bool isNewTask, CancellationToken ct)
    {
        var taskWithOrg = await _context.OrgTasks
            .Include(t => t.EventCategory).ThenInclude(c => c.Milestone).ThenInclude(m => m.Event)
            .FirstAsync(t => t.Id == task.Id, ct);
        var orgId = taskWithOrg.EventCategory.Milestone.Event.OrgId;

        if (task.AssigneeId.HasValue)
        {
            var assigneeUserId = await _context.Members
                .Where(m => m.Id == task.AssigneeId.Value)
                .Select(m => m.UserId)
                .FirstOrDefaultAsync(ct);

            if (assigneeUserId != Guid.Empty)
            {
                _context.Notifications.Add(new Notification
                {
                    ReceiverId = assigneeUserId,
                    ActorId = actorUserId,
                    Title = isNewTask ? "New task assigned" : "Task assignment updated",
                    Message = $"Task '{task.TaskName}' has been assigned to you.",
                    Type = NotificationType.TaskAssigned,
                    RelatedEntityType = nameof(OrgTask),
                    RelatedEntityId = task.Id,
                    ActionUrl = $"/org/departments?orgId={orgId}",
                    IsRead = false
                });
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task NotifyTaskStatusChangedAsync(OrgTask task, Guid actorUserId, CancellationToken ct)
    {
        var taskWithOrg = await _context.OrgTasks
            .Include(t => t.EventCategory).ThenInclude(c => c.Milestone).ThenInclude(m => m.Event)
            .FirstAsync(t => t.Id == task.Id, ct);
        var orgId = taskWithOrg.EventCategory.Milestone.Event.OrgId;

        var members = await _context.Members
            .Include(m => m.Role)
            .Where(m => m.OrgId == orgId && m.Status == MemberStatus.Active)
            .ToListAsync(ct);

        var receivers = members
            .Where(m =>
            {
                var roleName = (m.Role?.RoleName ?? string.Empty).Trim().ToLowerInvariant();
                if (roleName == "president" || roleName == "vice president" || roleName == "vicepresident")
                {
                    return true;
                }

                return task.DeptId.HasValue && m.DepartmentId == task.DeptId.Value;
            })
            .Select(m => m.UserId)
            .Distinct()
            .ToList();

        if (receivers.Count == 0)
        {
            return;
        }

        var notifications = receivers.Select(userId => new Notification
        {
            ReceiverId = userId,
            ActorId = actorUserId,
            Title = "Task status updated",
            Message = $"Task '{task.TaskName}' is now '{task.Status}'.",
            Type = NotificationType.System,
            RelatedEntityType = nameof(OrgTask),
            RelatedEntityId = task.Id,
            ActionUrl = $"/org/departments?orgId={orgId}",
            IsRead = false
        });

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);
    }

    private async Task VerifyTaskManagementPermissionAsync(Guid orgId, Guid userId, Guid? deptId, CancellationToken ct)
    {
        var hasGlobal = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active)
            .SelectMany(m => m.Role!.RolePermissions.Select(rp => rp.Permission.PermissionKey))
            .AnyAsync(key => key == "org.events.manage", ct);

        if (hasGlobal)
        {
            return;
        }

        if (!deptId.HasValue)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage tasks without department scope");
        }

        var currentMemberId = await _context.Members
            .Where(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(ct);

        var isDeptManager = await _context.Departments
            .AnyAsync(d => d.Id == deptId.Value && d.OrgId == orgId && d.ManagerId == currentMemberId, ct);

        if (!isDeptManager)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage tasks for this department");
        }
    }

    private async Task VerifyDepartmentTaskCreatePermissionAsync(Guid orgId, Guid userId, Guid departmentId, CancellationToken ct)
    {
        var member = await _context.Members
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }

        var roleName = (member.Role?.RoleName ?? string.Empty).Trim().ToLowerInvariant();
        var isLeadership = roleName == "president" || roleName == "vice president" || roleName == "vicepresident";
        if (isLeadership)
        {
            return;
        }

        var isDeptManager = await _context.Departments
            .AnyAsync(d => d.Id == departmentId && d.OrgId == orgId && d.ManagerId == member.Id, ct);

        if (!isDeptManager)
        {
            throw new UnauthorizedAccessException("Only President, Vice President, or Department Manager can create department tasks");
        }
    }
}
