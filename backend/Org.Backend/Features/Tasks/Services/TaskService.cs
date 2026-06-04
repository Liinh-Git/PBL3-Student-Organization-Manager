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
            .Include(t => t.EventCategory).ThenInclude(c => c!.Milestone).ThenInclude(m => m.Event)
            .Where(t => t.DeptId == departmentId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        return tasks.Select(t => t.ToTaskDto()).ToList();
    }

    public async Task<TaskDto> CreateDepartmentTaskAsync(Guid orgId, Guid departmentId, CreateDepartmentTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        await VerifyMembershipAsync(orgId, userId, ct);
        await VerifyDepartmentTaskManagePermissionAsync(orgId, userId, departmentId, ct);

        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        if (department == null || department.OrgId != orgId)
        {
            throw new KeyNotFoundException("Department not found");
        }

        var currentMember = await GetActiveMemberAsync(orgId, userId, ct);
        var payload = request.Task with { DeptId = departmentId };

        if (payload.AssigneeId.HasValue)
        {
            await EnsureDepartmentAssigneeIsEligibleAsync(orgId, payload.AssigneeId.Value, departmentId, ct);
        }

        var task = new OrgTask
        {
            Id = Guid.NewGuid(),
            EventCategoryId = null,
            TaskName = payload.TaskName,
            Description = payload.Description,
            AssigneeId = payload.AssigneeId,
            DeptId = departmentId,
            Deadline = ToUtc(payload.Deadline),
            Priority = ParsePriority(payload.Priority),
            Status = DomainTaskStatus.Todo,
            Note = payload.Note,
            CreatedByMemberId = currentMember.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrgTasks.Add(task);
        await _context.SaveChangesAsync(ct);
        await NotifyTaskAssignmentAsync(task, userId, isNewTask: true, ct);

        task = await LoadTaskForDtoAsync(task.Id, ct);
        return task.ToTaskDto();
    }

    public async Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        var task = await LoadTaskForScopeAsync(taskId, ct);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = await GetTaskOrgIdAsync(task, ct);
        await VerifyMembershipAsync(orgId, userId, ct);

        return task.ToTaskDto();
    }

    public async Task<TaskDto> CreateTaskAsync(Guid categoryId, CreateTaskRequest request, Guid userId, CancellationToken ct = default)
    {
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
        await EnsureEventManagerAsync(category.Milestone.EventId, userId, ct);

        var currentMember = await GetActiveMemberAsync(orgId, userId, ct);

        if (request.DeptId.HasValue)
        {
            await EnsureDepartmentBelongsToOrgAsync(orgId, request.DeptId.Value, ct);
        }

        if (request.AssigneeId.HasValue)
        {
            await EnsureAssigneeIsEligibleForEventTaskAsync(
                category.Milestone.EventId,
                orgId,
                request.AssigneeId.Value,
                request.DeptId,
                ct);
        }

        var task = new OrgTask
        {
            Id = Guid.NewGuid(),
            EventCategoryId = categoryId,
            TaskName = request.TaskName,
            Description = request.Description,
            AssigneeId = request.AssigneeId,
            DeptId = request.DeptId,
            Deadline = ToUtc(request.Deadline),
            Priority = ParsePriority(request.Priority),
            Status = DomainTaskStatus.Todo,
            Note = request.Note,
            CreatedByMemberId = currentMember.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrgTasks.Add(task);
        await _context.SaveChangesAsync(ct);
        await NotifyTaskAssignmentAsync(task, userId, isNewTask: true, ct);

        task = await LoadTaskForDtoAsync(task.Id, ct);
        return task.ToTaskDto();
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        var task = await LoadTaskForScopeAsync(taskId, ct);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = await GetTaskOrgIdAsync(task, ct);
        await VerifyMembershipAsync(orgId, userId, ct);

        var effectiveDeptId = request.DeptId ?? task.DeptId;
        if (task.EventCategoryId.HasValue)
        {
            await EnsureEventManagerAsync(task.EventCategory!.Milestone.EventId, userId, ct);
            if (effectiveDeptId.HasValue)
            {
                await EnsureDepartmentBelongsToOrgAsync(orgId, effectiveDeptId.Value, ct);
            }

            if (request.AssigneeId.HasValue)
            {
                await EnsureAssigneeIsEligibleForEventTaskAsync(
                    task.EventCategory.Milestone.EventId,
                    orgId,
                    request.AssigneeId.Value,
                    effectiveDeptId,
                    ct);
            }
        }
        else
        {
            if (!effectiveDeptId.HasValue)
            {
                throw new InvalidOperationException("Department task must belong to a department");
            }

            await VerifyDepartmentTaskManagePermissionAsync(orgId, userId, effectiveDeptId.Value, ct);
            if (request.AssigneeId.HasValue)
            {
                await EnsureDepartmentAssigneeIsEligibleAsync(orgId, request.AssigneeId.Value, effectiveDeptId.Value, ct);
            }
        }

        var previousAssigneeId = task.AssigneeId;
        task.TaskName = request.TaskName;
        task.Description = request.Description;
        task.AssigneeId = request.AssigneeId;
        task.DeptId = effectiveDeptId;
        task.Deadline = ToUtc(request.Deadline);
        task.Note = request.Note;
        task.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.Priority))
        {
            task.Priority = ParsePriority(request.Priority);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            task.Status = ParseStatus(request.Status);
            ApplyCompletedAt(task);
        }

        await _context.SaveChangesAsync(ct);
        if (task.AssigneeId.HasValue && task.AssigneeId != previousAssigneeId)
        {
            await NotifyTaskAssignmentAsync(task, userId, isNewTask: false, ct);
        }

        task = await LoadTaskForDtoAsync(taskId, ct);
        return task.ToTaskDto();
    }

    public async Task DeleteTaskAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        var task = await LoadTaskForScopeAsync(taskId, ct);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = await GetTaskOrgIdAsync(task, ct);
        await VerifyMembershipAsync(orgId, userId, ct);
        if (task.EventCategoryId.HasValue)
        {
            await EnsureEventManagerAsync(task.EventCategory!.Milestone.EventId, userId, ct);
        }
        else if (task.DeptId.HasValue)
        {
            await VerifyDepartmentTaskManagePermissionAsync(orgId, userId, task.DeptId.Value, ct);
        }

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request, Guid userId, CancellationToken ct = default)
    {
        var task = await LoadTaskForScopeAsync(taskId, ct);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = await GetTaskOrgIdAsync(task, ct);
        await VerifyMembershipAsync(orgId, userId, ct);
        await EnsureTaskStatusUpdatePermissionAsync(task, orgId, userId, ct);

        var previousStatus = task.Status;
        task.Status = ParseStatus(request.Status);
        task.UpdatedAt = DateTime.UtcNow;
        ApplyCompletedAt(task);

        await _context.SaveChangesAsync(ct);
        if (previousStatus != DomainTaskStatus.Done && task.Status == DomainTaskStatus.Done && task.EventCategoryId.HasValue)
        {
            await NotifyTaskCompletedAsync(task, task.EventCategory!.Milestone.EventId, userId, ct);
        }

        task = await LoadTaskForDtoAsync(taskId, ct);
        return task.ToTaskDto();
    }

    public async Task<TaskDto> AssignTaskAsync(Guid taskId, AssignTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        var task = await LoadTaskForScopeAsync(taskId, ct);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var orgId = await GetTaskOrgIdAsync(task, ct);
        await VerifyMembershipAsync(orgId, userId, ct);

        var effectiveDeptId = request.DeptId ?? task.DeptId;
        if (task.EventCategoryId.HasValue)
        {
            await EnsureEventManagerAsync(task.EventCategory!.Milestone.EventId, userId, ct);
            if (effectiveDeptId.HasValue)
            {
                await EnsureDepartmentBelongsToOrgAsync(orgId, effectiveDeptId.Value, ct);
            }

            if (request.AssigneeId.HasValue)
            {
                await EnsureAssigneeIsEligibleForEventTaskAsync(
                    task.EventCategory.Milestone.EventId,
                    orgId,
                    request.AssigneeId.Value,
                    effectiveDeptId,
                    ct);
            }
        }
        else
        {
            if (!effectiveDeptId.HasValue)
            {
                throw new InvalidOperationException("Department task must belong to a department");
            }

            await VerifyDepartmentTaskManagePermissionAsync(orgId, userId, effectiveDeptId.Value, ct);
            if (request.AssigneeId.HasValue)
            {
                await EnsureDepartmentAssigneeIsEligibleAsync(orgId, request.AssigneeId.Value, effectiveDeptId.Value, ct);
            }
        }

        var previousAssigneeId = task.AssigneeId;
        task.AssigneeId = request.AssigneeId;
        task.DeptId = effectiveDeptId;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        if (task.AssigneeId.HasValue && task.AssigneeId != previousAssigneeId)
        {
            await NotifyTaskAssignmentAsync(task, userId, isNewTask: false, ct);
        }

        task = await LoadTaskForDtoAsync(taskId, ct);
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

    private async Task<Member> GetActiveMemberAsync(Guid orgId, Guid userId, CancellationToken ct)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        return member;
    }

    private async Task EnsureEventManagerAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        if (!await IsEventManagerAsync(eventId, userId, ct))
        {
            throw new UnauthorizedAccessException("Only event organizers can manage tasks");
        }
    }

    private async Task<bool> IsEventManagerAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        return await _context.EventMembers.AnyAsync(
            em => em.EventId == eventId &&
                  em.Member.UserId == userId &&
                  em.Member.Status == MemberStatus.Active,
            ct);
    }

    private async Task EnsureTaskStatusUpdatePermissionAsync(OrgTask task, Guid orgId, Guid userId, CancellationToken ct)
    {
        if (task.EventCategoryId.HasValue)
        {
            var eventId = task.EventCategory!.Milestone.EventId;

            if (await IsEventManagerAsync(eventId, userId, ct))
            {
                return;
            }

            if (!task.AssigneeId.HasValue)
            {
                throw new UnauthorizedAccessException("Only event organizers or assigned event members can update task status");
            }

            var isAssignedUser = await _context.EventMembers.AnyAsync(
                em => em.EventId == eventId &&
                      em.MemberId == task.AssigneeId.Value &&
                      em.Member.UserId == userId &&
                      em.Member.Status == MemberStatus.Active,
                ct);

            if (!isAssignedUser)
            {
                throw new UnauthorizedAccessException("Only event organizers or assigned event members can update task status");
            }

            return;
        }

        if (task.DeptId.HasValue && await CanManageDepartmentTaskAsync(orgId, userId, task.DeptId.Value, ct))
        {
            return;
        }

        if (task.AssigneeId.HasValue)
        {
            var isAssignedUser = await _context.Members.AnyAsync(
                m => m.Id == task.AssigneeId.Value &&
                     m.OrgId == orgId &&
                     m.UserId == userId &&
                     m.Status == MemberStatus.Active,
                ct);

            if (isAssignedUser)
            {
                return;
            }
        }

        throw new UnauthorizedAccessException("Only department managers, leaders, or the assigned member can update task status");
    }

    private async Task EnsureAssigneeIsEligibleForEventTaskAsync(
        Guid eventId,
        Guid orgId,
        Guid assigneeMemberId,
        Guid? deptId,
        CancellationToken ct)
    {
        await EnsureMemberIsActiveInOrgAsync(orgId, assigneeMemberId, deptId, ct);

        var isEventMember = await _context.EventMembers.AnyAsync(
            em => em.EventId == eventId && em.MemberId == assigneeMemberId,
            ct);

        if (!isEventMember)
        {
            throw new InvalidOperationException("Assignee must be an event organizer member of this event");
        }
    }

    private async Task EnsureDepartmentAssigneeIsEligibleAsync(Guid orgId, Guid assigneeMemberId, Guid departmentId, CancellationToken ct)
    {
        await EnsureMemberIsActiveInOrgAsync(orgId, assigneeMemberId, departmentId, ct);
    }

    private async Task EnsureMemberIsActiveInOrgAsync(Guid orgId, Guid memberId, Guid? departmentId, CancellationToken ct)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == memberId, ct);
        if (member == null)
        {
            throw new KeyNotFoundException("Assignee not found");
        }

        if (member.OrgId != orgId)
        {
            throw new InvalidOperationException("Assignee must belong to the same organization");
        }

        if (member.Status != MemberStatus.Active)
        {
            throw new InvalidOperationException("Assignee must be an active member");
        }

        if (departmentId.HasValue && member.DepartmentId != departmentId.Value)
        {
            throw new InvalidOperationException("Assignee must belong to the same department");
        }
    }

    private async Task NotifyTaskAssignmentAsync(OrgTask task, Guid actorUserId, bool isNewTask, CancellationToken ct)
    {
        if (!task.AssigneeId.HasValue)
        {
            return;
        }

        var orgId = await GetTaskOrgIdAsync(task, ct);
        var assigneeUserId = await _context.Members
            .Where(m => m.Id == task.AssigneeId.Value)
            .Select(m => m.UserId)
            .FirstOrDefaultAsync(ct);

        if (assigneeUserId == Guid.Empty)
        {
            return;
        }

        _context.Notifications.Add(new Notification
        {
            ReceiverId = assigneeUserId,
            ActorId = actorUserId,
            Title = isNewTask ? "New task assigned" : "Task assignment updated",
            Message = $"Task '{task.TaskName}' has been assigned to you.",
            Type = NotificationType.TaskAssigned,
            RelatedEntityType = nameof(OrgTask),
            RelatedEntityId = task.Id,
            ActionUrl = task.EventCategoryId.HasValue
                ? $"/org/events/{task.EventCategory!.Milestone.EventId}?orgId={orgId}"
                : $"/org/departments?orgId={orgId}",
            IsRead = false
        });

        await _context.SaveChangesAsync(ct);
    }

    private async Task NotifyTaskCompletedAsync(OrgTask task, Guid eventId, Guid actorUserId, CancellationToken ct)
    {
        var orgId = await GetTaskOrgIdAsync(task, ct);
        var receivers = await _context.EventMembers
            .Where(em => em.EventId == eventId && em.Member.Status == MemberStatus.Active)
            .Select(em => em.Member.UserId)
            .Distinct()
            .Where(userId => userId != actorUserId)
            .ToListAsync(ct);

        if (receivers.Count == 0)
        {
            return;
        }

        var notifications = receivers.Select(userId => new Notification
        {
            ReceiverId = userId,
            ActorId = actorUserId,
            Title = "Task completed",
            Message = $"Task '{task.TaskName}' has been moved to Done.",
            Type = NotificationType.System,
            RelatedEntityType = nameof(OrgTask),
            RelatedEntityId = task.Id,
            ActionUrl = $"/org/events/{eventId}?orgId={orgId}",
            IsRead = false
        });

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);
    }

    private async Task VerifyDepartmentTaskManagePermissionAsync(Guid orgId, Guid userId, Guid departmentId, CancellationToken ct)
    {
        if (!await CanManageDepartmentTaskAsync(orgId, userId, departmentId, ct))
        {
            throw new UnauthorizedAccessException("Only President, Vice President, or Department Manager can manage department tasks");
        }
    }

    private async Task<bool> CanManageDepartmentTaskAsync(Guid orgId, Guid userId, Guid departmentId, CancellationToken ct)
    {
        var member = await _context.Members
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            return false;
        }

        var roleName = (member.Role?.RoleName ?? string.Empty).Trim().ToLowerInvariant();
        var isLeadership = roleName == "president" || roleName == "vice president" || roleName == "vicepresident";
        if (isLeadership)
        {
            return true;
        }

        return await _context.Departments
            .AnyAsync(d => d.Id == departmentId && d.OrgId == orgId && d.ManagerId == member.Id, ct);
    }

    private async Task EnsureDepartmentBelongsToOrgAsync(Guid orgId, Guid departmentId, CancellationToken ct)
    {
        var belongs = await _context.Departments.AnyAsync(d => d.Id == departmentId && d.OrgId == orgId, ct);
        if (!belongs)
        {
            throw new InvalidOperationException("Department must belong to the same organization");
        }
    }

    private async Task<OrgTask?> LoadTaskForScopeAsync(Guid taskId, CancellationToken ct)
    {
        return await _context.OrgTasks
            .Include(t => t.EventCategory).ThenInclude(c => c!.Milestone).ThenInclude(m => m.Event)
            .Include(t => t.Department)
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, ct);
    }

    private async Task<OrgTask> LoadTaskForDtoAsync(Guid taskId, CancellationToken ct)
    {
        return await _context.OrgTasks
            .Include(t => t.EventCategory).ThenInclude(c => c!.Milestone).ThenInclude(m => m.Event)
            .Include(t => t.Assignee).ThenInclude(a => a!.User)
            .Include(t => t.Department)
            .Include(t => t.CreatedByMember).ThenInclude(cb => cb!.User)
            .FirstAsync(t => t.Id == taskId, ct);
    }

    private async Task<Guid> GetTaskOrgIdAsync(OrgTask task, CancellationToken ct)
    {
        if (task.EventCategory?.Milestone?.Event != null)
        {
            return task.EventCategory.Milestone.Event.OrgId;
        }

        if (task.Department != null)
        {
            return task.Department.OrgId;
        }

        if (task.DeptId.HasValue)
        {
            var orgId = await _context.Departments
                .Where(d => d.Id == task.DeptId.Value)
                .Select(d => (Guid?)d.OrgId)
                .FirstOrDefaultAsync(ct);

            if (orgId.HasValue)
            {
                return orgId.Value;
            }
        }

        throw new InvalidOperationException("Task is not attached to an event category or department");
    }

    private static TaskPriority ParsePriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
        {
            return TaskPriority.Medium;
        }

        if (!Enum.TryParse<TaskPriority>(priority, out var parsed))
        {
            throw new ArgumentException($"Invalid priority: {priority}");
        }

        return parsed;
    }

    private static DomainTaskStatus ParseStatus(string status)
    {
        if (!Enum.TryParse<DomainTaskStatus>(status, out var parsed))
        {
            throw new ArgumentException($"Invalid status: {status}");
        }

        return parsed;
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        return value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
    }

    private static void ApplyCompletedAt(OrgTask task)
    {
        if (task.Status == DomainTaskStatus.Done && !task.CompletedAt.HasValue)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (task.Status != DomainTaskStatus.Done)
        {
            task.CompletedAt = null;
        }
    }
}
