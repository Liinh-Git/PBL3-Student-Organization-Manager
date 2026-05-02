using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;

namespace Org.Frontend.Services.Overview;

public sealed class OverviewMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IOverviewService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<OverviewPageViewModel> GetOverviewAsync(CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var currentUserId))
            throw new InvalidOperationException("User not authenticated.");

        var displayName = user.FindFirst(ClaimTypes.Name)?.Value?.Trim();

        return await _mockDataStore.UseAsync(data =>
        {
            var userMembers = data.Members
                .Where(x => x.UserId == currentUserId)
                .ToList();
            var userMemberIds = userMembers.Select(x => x.Id).ToHashSet();
            var userOrganizationIds = userMembers.Select(x => x.OrgId).ToHashSet();

            var organizations = userMembers
                .Select(member =>
                {
                    var organization = data.Organizations.FirstOrDefault(x => x.Id == member.OrgId);
                    if (organization is null)
                        return null;

                    var orgEventIds = data.Events
                        .Where(x => x.OrgId == organization.Id)
                        .Select(x => x.Id)
                        .ToHashSet();

                    var orgMilestoneIds = data.Milestones
                        .Where(x => orgEventIds.Contains(x.EventId))
                        .Select(x => x.Id)
                        .ToHashSet();

                    var orgCategoryIds = data.EventCategories
                        .Where(x => orgMilestoneIds.Contains(x.MilestoneId))
                        .Select(x => x.Id)
                        .ToHashSet();

                    return new OverviewOrganizationItem(
                        organization.Id,
                        organization.OrgName,
                        organization.Description,
                        ResolveOrgImage(organization),
                        ResolveMemberRole(member.RoleId, data),
                        data.Members.Count(x => x.OrgId == organization.Id),
                        orgEventIds.Count,
                        data.Tasks.Count(x => orgCategoryIds.Contains(x.CategoryId)));
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var organizingEventIds = data.EventMembers
                .Where(x => userMemberIds.Contains(x.MemberId))
                .Select(x => x.EventId)
                .ToHashSet();

            var organizingEvents = organizingEventIds
                .Select(eventId =>
                {
                    var eventItem = data.Events.FirstOrDefault(x => x.Id == eventId);
                    if (eventItem is null)
                        return null;

                    var eventMember = data.EventMembers.FirstOrDefault(x =>
                        x.EventId == eventId && userMemberIds.Contains(x.MemberId));
                    var organization = data.Organizations.FirstOrDefault(x => x.Id == eventItem.OrgId);

                    return new OverviewEventItem(
                        eventItem.Id,
                        eventItem.OrgId,
                        eventItem.Name,
                        organization?.OrgName ?? "Organization",
                        eventItem.StartDate,
                        eventItem.EndDate,
                        eventItem.Location,
                        eventItem.StatusLabel,
                        eventMember?.EventRole ?? "Organizer",
                        null,
                        ResolveEventImage(eventItem, organization));
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.EventTitle, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var attendingEvents = data.Attendees
                .Where(x => x.UserId == currentUserId
                    && !string.Equals(x.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                .GroupBy(x => x.EventId)
                .Select(x =>
                {
                    var attendee = x.OrderByDescending(y => y.CreatedAt).First();
                    var eventItem = data.Events.FirstOrDefault(ev => ev.Id == attendee.EventId);
                    if (eventItem is null)
                        return null;

                    var organization = data.Organizations.FirstOrDefault(org => org.Id == eventItem.OrgId);
                    return new OverviewEventItem(
                        eventItem.Id,
                        eventItem.OrgId,
                        eventItem.Name,
                        organization?.OrgName ?? "Organization",
                        eventItem.StartDate,
                        eventItem.EndDate,
                        eventItem.Location,
                        eventItem.StatusLabel,
                        null,
                        attendee.Status,
                        ResolveEventImage(eventItem, organization));
                })
                .Where(x => x is not null && !organizingEventIds.Contains(x.EventId))
                .Select(x => x!)
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.EventTitle, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var assignedTasks = data.Tasks
                .Where(x => x.AssigneeMemberId.HasValue && userMemberIds.Contains(x.AssigneeMemberId.Value))
                .Select(task => MapTask(task, data))
                .OrderBy(x => x.DeadlineUtc ?? DateTime.MaxValue)
                .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var filteredTasks = assignedTasks
                .Where(x => userOrganizationIds.Contains(x.OrganizationId))
                .ToList();

            return new OverviewPageViewModel(
                displayName ?? "Nguoi dung",
                organizations,
                organizingEvents,
                attendingEvents,
                filteredTasks);
        }, ct);
    }

    private static OverviewTaskItem MapTask(MockTask task, MockDataSet data)
    {
        var category = data.EventCategories.First(x => x.Id == task.CategoryId);
        var milestone = data.Milestones.First(x => x.Id == category.MilestoneId);
        var eventItem = data.Events.First(x => x.Id == milestone.EventId);
        var organization = data.Organizations.First(x => x.Id == eventItem.OrgId);

        var assigneeMember = task.AssigneeMemberId.HasValue
            ? data.Members.FirstOrDefault(x => x.Id == task.AssigneeMemberId.Value)
            : null;
        var department = assigneeMember?.DepartmentId.HasValue == true
            ? data.Departments.FirstOrDefault(x => x.Id == assigneeMember.DepartmentId.Value)
            : null;

        var coAssigneeMembers = task.CoAssigneeMemberIds
            .Distinct()
            .Select(memberId => data.Members.FirstOrDefault(x => x.Id == memberId))
            .Where(x => x is not null)
            .Select(member =>
            {
                var user = data.Users.FirstOrDefault(x => x.Id == member!.UserId);
                return new OverviewTaskAssigneeItem(
                    member!.Id,
                    member.UserId,
                    member.DisplayName,
                    user?.AvatarUrl,
                    ResolveMemberRole(member.RoleId, data));
            })
            .ToList();

        return new OverviewTaskItem(
            task.Id,
            task.Title,
            ResolveTaskDescription(task),
            task.DueDate,
            task.Status,
            task.Priority,
            organization.Id,
            organization.OrgName,
            eventItem.Id,
            eventItem.Name,
            milestone.Id,
            milestone.Name,
            category.Id,
            category.Name,
            department?.Id,
            department?.DeptName,
            coAssigneeMembers);
    }

    private static string ResolveTaskDescription(MockTask task)
    {
        if (!string.IsNullOrWhiteSpace(task.Description))
            return task.Description.Trim();
        if (!string.IsNullOrWhiteSpace(task.Note))
            return task.Note.Trim();
        return "Chua co mo ta.";
    }

    private static string ResolveOrgImage(MockOrganization org)
    {
        if (!string.IsNullOrWhiteSpace(org.AvatarUrl))
            return org.AvatarUrl;
        if (!string.IsNullOrWhiteSpace(org.CoverUrl))
            return org.CoverUrl;
        return "/images/mockimages/Org1/Avt.jpg";
    }

    private static string ResolveEventImage(MockEvent eventItem, MockOrganization? org)
    {
        if (!string.IsNullOrWhiteSpace(eventItem.ImageUrl))
            return eventItem.ImageUrl;
        if (!string.IsNullOrWhiteSpace(org?.CoverUrl))
            return org.CoverUrl;
        if (!string.IsNullOrWhiteSpace(org?.AvatarUrl))
            return org.AvatarUrl;
        return "/images/mockimages/Org1/Card1.jpg";
    }

    private static string ResolveMemberRole(Guid? roleId, MockDataSet data)
    {
        if (!roleId.HasValue)
            return "Member";

        var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == roleId.Value);
        return role?.RoleName ?? "Member";
    }
}
