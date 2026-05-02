using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Tasks;

public sealed class TaskMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : ITaskService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<List<TaskViewModel>> GetTasksAsync(Guid categoryId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var context = ResolveCategoryContext(data, categoryId);
            var member = ResolveMembership(data, context.Event.OrgId, currentUserId.Value);
            if (member is null)
                throw new UnauthorizedAccessException("You do not have access to this category.");

            var canManage = CanManageCategory(data, context, member.Value.MemberId, member.Value.RoleName);

            return data.Tasks
                .Where(x => x.CategoryId == categoryId)
                .OrderBy(x => x.DueDate ?? DateTime.MaxValue)
                .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
                .Select(x => MapTask(data, x, member.Value.MemberId, canManage))
                .ToList();
        });
    }

    public async Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusViewModel req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        await _mockDataStore.UseAsync(data =>
        {
            var task = data.Tasks.FirstOrDefault(x => x.Id == taskId)
                ?? throw new KeyNotFoundException($"Task {taskId} not found in mock data.");

            var context = ResolveCategoryContext(data, task.CategoryId);
            var member = ResolveMembership(data, context.Event.OrgId, currentUserId.Value);
            if (member is null)
                throw new UnauthorizedAccessException("You do not have access to this task.");

            var targetStatus = NormalizeStatus(req.Status);
            var canManage = CanManageCategory(data, context, member.Value.MemberId, member.Value.RoleName);
            var isAssigned = task.AssigneeMemberId == member.Value.MemberId
                             || task.CoAssigneeMemberIds.Contains(member.Value.MemberId);

            if (targetStatus == "DONE")
            {
                if (!canManage && !isAssigned)
                    throw new UnauthorizedAccessException("Only assigned members (or manager+) can mark this task as completed.");
            }
            else
            {
                if (!canManage)
                    throw new UnauthorizedAccessException("Only category manager or higher can change this task status.");
            }

            task.Status = targetStatus;
            return 0;
        });
    }

    public async Task<TaskViewModel> CreateTaskAsync(Guid categoryId, CreateTaskViewModel req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var context = ResolveCategoryContext(data, categoryId);
            var member = ResolveMembership(data, context.Event.OrgId, currentUserId.Value);
            if (member is null)
                throw new UnauthorizedAccessException("You do not have access to this category.");

            var canManage = CanManageCategory(data, context, member.Value.MemberId, member.Value.RoleName);
            if (!canManage)
                throw new UnauthorizedAccessException("Only category manager or higher can create tasks.");

            var assignees = (req.AssigneeMemberIds ?? [])
                .Distinct()
                .ToList();

            foreach (var assigneeMemberId in assignees)
            {
                var assignee = data.Members.FirstOrDefault(x => x.Id == assigneeMemberId)
                    ?? throw new InvalidOperationException("Assignee member does not exist.");

                if (assignee.OrgId != context.Event.OrgId)
                    throw new InvalidOperationException("Assignee must belong to the same organization as the event.");
            }

            var item = new MockTask
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Title = NormalizeTitle(req.Title),
                Status = "TODO",
                Priority = NormalizePriority(req.Priority),
                AssigneeMemberId = assignees.FirstOrDefault(),
                CoAssigneeMemberIds = assignees.Skip(1).ToList(),
                DueDate = req.DueDate,
                Note = null
            };

            data.Tasks.Insert(0, item);
            return MapTask(data, item, member.Value.MemberId, canManage);
        });
    }

    public async Task<bool> CanManageTasksAsync(Guid categoryId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            return false;

        return await _mockDataStore.UseAsync(data =>
        {
            var context = ResolveCategoryContext(data, categoryId);
            var member = ResolveMembership(data, context.Event.OrgId, currentUserId.Value);
            if (member is null)
                return false;

            return CanManageCategory(data, context, member.Value.MemberId, member.Value.RoleName);
        });
    }

    private static TaskViewModel MapTask(MockDataSet data, MockTask source, Guid currentMemberId, bool canManage)
    {
        var assigneeName = "Unassigned";
        if (source.AssigneeMemberId.HasValue)
        {
            assigneeName = data.Members.FirstOrDefault(x => x.Id == source.AssigneeMemberId.Value)?.DisplayName
                ?? "Unassigned";
        }

        var isAssigned = source.AssigneeMemberId == currentMemberId || source.CoAssigneeMemberIds.Contains(currentMemberId);

        return new TaskViewModel
        {
            Id = source.Id,
            CategoryId = source.CategoryId,
            Title = source.Title,
            Status = NormalizeStatus(source.Status),
            Priority = source.Priority,
            AssigneeMemberId = source.AssigneeMemberId,
            CoAssigneeMemberIds = source.CoAssigneeMemberIds.ToList(),
            AssigneeName = assigneeName,
            DueDate = source.DueDate,
            CanMarkCompleted = canManage || isAssigned
        };
    }

    private async Task<Guid?> TryGetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }

    private static (MockCategoryContext Category, MockMilestone Milestone, MockEvent Event) ResolveCategoryContext(MockDataSet data, Guid categoryId)
    {
        var category = data.EventCategories.FirstOrDefault(x => x.Id == categoryId)
            ?? throw new KeyNotFoundException($"Category {categoryId} not found in mock data.");

        var milestone = data.Milestones.FirstOrDefault(x => x.Id == category.MilestoneId)
            ?? throw new KeyNotFoundException($"Milestone {category.MilestoneId} not found in mock data.");

        var eventItem = data.Events.FirstOrDefault(x => x.Id == milestone.EventId)
            ?? throw new KeyNotFoundException($"Event {milestone.EventId} not found in mock data.");

        return (new MockCategoryContext(category.Id, category.LeadMemberId), milestone, eventItem);
    }

    private static (Guid MemberId, string RoleName)? ResolveMembership(MockDataSet data, Guid orgId, Guid userId)
    {
        var member = data.Members.FirstOrDefault(x => x.OrgId == orgId && x.UserId == userId);
        if (member is null)
            return null;

        var roleName = "Member";
        if (member.RoleId.HasValue)
        {
            roleName = data.OrganizationRoles.FirstOrDefault(x => x.Id == member.RoleId.Value)?.RoleName ?? "Member";
        }

        return (member.Id, roleName);
    }

    private static bool CanManageCategory(MockDataSet data, (MockCategoryContext Category, MockMilestone Milestone, MockEvent Event) context, Guid memberId, string roleName)
    {
        if (roleName.Trim().ToUpperInvariant() is "PRESIDENT" or "VICEPRESIDENT" or "MANAGER")
            return true;

        if (context.Category.LeadMemberId.HasValue && context.Category.LeadMemberId.Value == memberId)
            return true;

        return data.EventMembers.Any(x =>
            x.EventId == context.Event.Id
            && x.MemberId == memberId
            && string.Equals(x.EventRole, "Coordinator", StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeTitle(string? title)
        => string.IsNullOrWhiteSpace(title) ? "New Task" : title.Trim();

    private static string NormalizeStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "IN_PROGRESS" => "IN_PROGRESS",
            "DONE" => "DONE",
            _ => "TODO"
        };
    }

    private static string NormalizePriority(string? priority)
    {
        return priority?.Trim().ToUpperInvariant() switch
        {
            "LOW" => "LOW",
            "HIGH" => "HIGH",
            "URGENT" => "URGENT",
            _ => "MEDIUM"
        };
    }

    private readonly record struct MockCategoryContext(Guid CategoryId, Guid? LeadMemberId);
}
