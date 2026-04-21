using Org.Backend.Domain.Entities;
using Org.Shared;
using Org.Shared.Features.Departments;
using Org.Shared.Features.EventCategories;
using Org.Shared.Features.Events;
using Org.Shared.Features.Members;
using Org.Shared.Features.Milestones;
using Org.Shared.Features.Tasks;
using SharedMemberRole = Org.Shared.MemberRole;

namespace Org.Backend.Features.Common;

internal static class ContractMapping
{
    public static DepartmentDto ToDepartmentDto(Department department, int memberCount)
        => new(
            department.Id,
            department.OrgId,
            department.Code ?? BuildDepartmentCode(department.DeptName),
            department.DeptName,
            department.Function,
            department.ManagerId,
            memberCount,
            ToUtcOffset(department.CreatedAt),
            department.UpdatedAt is null ? null : ToUtcOffset(department.UpdatedAt.Value));

    public static MemberDto ToMemberDto(Member member)
        => new(
            member.Id,
            member.OrgId,
            member.DepartmentId,
            $"MEM-{member.Id.ToString("N")[..8].ToUpperInvariant()}",
            member.User.FullName,
            member.User.Email,
            ParseRole(member.Role?.RoleName),
            !member.IsDeleted,
            ToUtcOffset(member.JoinDate));

    public static EventDto ToEventDto(Event entity)
        => new(
            entity.Id,
            entity.OrgId,
            entity.EventName,
            entity.Description,
            DateOnly.FromDateTime(entity.StartDate),
            DateOnly.FromDateTime(entity.EndDate),
            entity.Status,
            ToUtcOffset(entity.CreatedAt),
            entity.UpdatedAt is null ? null : ToUtcOffset(entity.UpdatedAt.Value));

    public static MilestoneDto ToMilestoneDto(Milestone entity)
        => new(
            entity.Id,
            entity.EventId,
            entity.Title,
            entity.Description,
            DateOnly.FromDateTime(entity.StartDate),
            DateOnly.FromDateTime(entity.EndDate),
            entity.OrderIndex,
            entity.Status);

    public static EventCategoryDto ToCategoryDto(EventCategory category, int taskCount, int completedTaskCount)
        => new(
            category.Id,
            category.MilestoneId,
            category.CategoryName,
            category.Description,
            category.OrderIndex,
            taskCount,
            completedTaskCount,
            category.OwnerDepartmentId,
            category.OwnerDepartment?.ManagerId,
            category.OwnerDepartment?.Manager?.User?.FullName);

    public static TaskDto ToTaskDto(OrgTask task)
        => new(
            task.Id,
            task.EventCategoryId,
            task.AssigneeId,
            task.TaskName,
            task.Note,
            task.Status,
            task.Deadline is null ? null : DateOnly.FromDateTime(task.Deadline.Value),
            task.Priority,
            ToUtcOffset(task.CreatedAt),
            task.UpdatedAt is null ? null : ToUtcOffset(task.UpdatedAt.Value));

    private static SharedMemberRole ParseRole(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return SharedMemberRole.Member;

        return Enum.TryParse<SharedMemberRole>(roleName, ignoreCase: true, out var parsed)
            ? parsed
            : SharedMemberRole.Member;
    }

    private static string BuildDepartmentCode(string name)
    {
        var compact = new string(name.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(compact))
            return "DEPT";

        return compact.Length <= 8
            ? compact.ToUpperInvariant()
            : compact[..8].ToUpperInvariant();
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
