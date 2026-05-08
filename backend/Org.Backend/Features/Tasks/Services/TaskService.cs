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
        await VerifyPermissionAsync(orgId, userId, "org.events.manage", ct);

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

        var task = new OrgTask
        {
            Id = Guid.NewGuid(),
            EventCategoryId = categoryId,
            TaskName = request.TaskName,
            Description = request.Description,
            AssigneeId = request.AssigneeId,
            DeptId = request.DeptId,
            Deadline = request.Deadline,
            Priority = priority,
            Status = DomainTaskStatus.Todo,
            Note = request.Note,
            CreatedByMemberId = currentMember.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrgTasks.Add(task);
        await _context.SaveChangesAsync(ct);

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
        await VerifyPermissionAsync(orgId, userId, "org.events.manage", ct);

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

        task.TaskName = request.TaskName;
        task.Description = request.Description;
        task.AssigneeId = request.AssigneeId;
        task.DeptId = request.DeptId;
        task.Deadline = request.Deadline;
        task.Note = request.Note;
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
        await VerifyPermissionAsync(orgId, userId, "org.events.manage", ct);

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
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
        await VerifyPermissionAsync(orgId, userId, "org.events.manage", ct);

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
        await VerifyPermissionAsync(orgId, userId, "org.events.manage", ct);

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
}
