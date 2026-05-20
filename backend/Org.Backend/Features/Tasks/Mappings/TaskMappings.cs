using Org.Backend.Domain.Entities;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Mappings;

public static class TaskMappings
{
    public static TaskDto ToTaskDto(this OrgTask task)
    {
        return new TaskDto
        {
            Id = task.Id,
            EventCategoryId = task.EventCategoryId,
            TaskName = task.TaskName,
            Description = task.Description,
            AssigneeId = task.AssigneeId,
            AssigneeName = task.Assignee?.User?.FullName,
            DeptId = task.DeptId,
            DeptName = task.Department?.DeptName,
            CreatedByMemberId = task.CreatedByMemberId,
            CreatedByMemberName = task.CreatedByMember?.User?.FullName,
            Deadline = task.Deadline,
            Priority = task.Priority.ToString(),
            Status = task.Status.ToString(),
            OrderIndex = 0, // OrgTask doesn't have OrderIndex in domain, using default
            Note = task.Note,
            CompletedAt = task.CompletedAt,
            CreatedAtUtc = task.CreatedAt,
            UpdatedAtUtc = task.UpdatedAt
        };
    }
}
