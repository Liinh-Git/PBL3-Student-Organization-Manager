using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public sealed class EventCategoryMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IEventCategoryService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<List<EventCategoryViewModel>> GetCategoriesAsync(Guid milestoneId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var milestone = data.Milestones.FirstOrDefault(x => x.Id == milestoneId)
                ?? throw new KeyNotFoundException($"Milestone {milestoneId} not found in mock data.");

            var eventItem = data.Events.FirstOrDefault(x => x.Id == milestone.EventId)
                ?? throw new KeyNotFoundException($"Event {milestone.EventId} not found in mock data.");

            EnsureCanReadEvent(data, eventItem, currentUserId.Value);

            return data.EventCategories
                .Where(x => x.MilestoneId == milestoneId)
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(category => MapCategory(data, category))
                .ToList();
        });
    }

    public async Task<EventCategoryViewModel> GetCategoryDetailAsync(Guid categoryId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var category = data.EventCategories.FirstOrDefault(x => x.Id == categoryId)
                ?? throw new KeyNotFoundException($"Category {categoryId} not found in mock data.");

            var milestone = data.Milestones.FirstOrDefault(x => x.Id == category.MilestoneId)
                ?? throw new KeyNotFoundException($"Milestone {category.MilestoneId} not found in mock data.");

            var eventItem = data.Events.FirstOrDefault(x => x.Id == milestone.EventId)
                ?? throw new KeyNotFoundException($"Event {milestone.EventId} not found in mock data.");

            EnsureCanReadEvent(data, eventItem, currentUserId.Value);
            return MapCategory(data, category);
        });
    }

    public async Task<EventCategoryViewModel> CreateCategoryAsync(CreateEventCategoryViewModel req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var milestone = data.Milestones.FirstOrDefault(x => x.Id == req.MilestoneId)
                ?? throw new KeyNotFoundException($"Milestone {req.MilestoneId} not found in mock data.");

            var eventItem = data.Events.FirstOrDefault(x => x.Id == milestone.EventId)
                ?? throw new KeyNotFoundException($"Event {milestone.EventId} not found in mock data.");

            EnsureCanManageEvent(data, eventItem, currentUserId.Value);

            if (req.LeadMemberId.HasValue)
            {
                var lead = data.Members.FirstOrDefault(x => x.Id == req.LeadMemberId.Value)
                    ?? throw new InvalidOperationException("Lead member does not exist.");

                if (lead.OrgId != eventItem.OrgId)
                    throw new InvalidOperationException("Lead member must belong to the same organization.");
            }

            var item = new MockEventCategory
            {
                Id = Guid.NewGuid(),
                MilestoneId = req.MilestoneId,
                Name = NormalizeName(req.Name),
                LeadMemberId = req.LeadMemberId,
                Description = null,
                OrderIndex = data.EventCategories.Where(x => x.MilestoneId == req.MilestoneId).Select(x => x.OrderIndex).DefaultIfEmpty(-1).Max() + 1,
                OwnerDepartmentId = null,
                IsUrgent = false,
                Guidelines = []
            };

            data.EventCategories.Add(item);
            return MapCategory(data, item);
        });
    }

    public async Task<EventCategoryViewModel> UpdateCategoryAsync(Guid categoryId, UpdateEventCategoryViewModel req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var category = data.EventCategories.FirstOrDefault(x => x.Id == categoryId)
                ?? throw new KeyNotFoundException($"Category {categoryId} not found in mock data.");

            var milestone = data.Milestones.FirstOrDefault(x => x.Id == category.MilestoneId)
                ?? throw new KeyNotFoundException($"Milestone {category.MilestoneId} not found in mock data.");

            var eventItem = data.Events.FirstOrDefault(x => x.Id == milestone.EventId)
                ?? throw new KeyNotFoundException($"Event {milestone.EventId} not found in mock data.");

            EnsureCanManageEvent(data, eventItem, currentUserId.Value);

            if (req.LeadMemberId.HasValue)
            {
                var lead = data.Members.FirstOrDefault(x => x.Id == req.LeadMemberId.Value)
                    ?? throw new InvalidOperationException("Lead member does not exist.");

                if (lead.OrgId != eventItem.OrgId)
                    throw new InvalidOperationException("Lead member must belong to the same organization.");
            }

            category.Name = NormalizeName(req.Name);
            category.LeadMemberId = req.LeadMemberId;
            return MapCategory(data, category);
        });
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        await _mockDataStore.UseAsync(data =>
        {
            var category = data.EventCategories.FirstOrDefault(x => x.Id == categoryId)
                ?? throw new KeyNotFoundException($"Category {categoryId} not found in mock data.");

            var milestone = data.Milestones.FirstOrDefault(x => x.Id == category.MilestoneId)
                ?? throw new KeyNotFoundException($"Milestone {category.MilestoneId} not found in mock data.");

            var eventItem = data.Events.FirstOrDefault(x => x.Id == milestone.EventId)
                ?? throw new KeyNotFoundException($"Event {milestone.EventId} not found in mock data.");

            EnsureCanManageEvent(data, eventItem, currentUserId.Value);

            data.Tasks.RemoveAll(x => x.CategoryId == categoryId);
            data.EventCategories.Remove(category);
            return 0;
        });
    }

    private async Task<Guid?> TryGetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }

    private static void EnsureCanReadEvent(MockDataSet data, MockEvent eventItem, Guid userId)
    {
        var member = data.Members.FirstOrDefault(x => x.OrgId == eventItem.OrgId && x.UserId == userId);
        if (member is null)
            throw new UnauthorizedAccessException("You do not have access to this event.");
    }

    private static void EnsureCanManageEvent(MockDataSet data, MockEvent eventItem, Guid userId)
    {
        var member = data.Members.FirstOrDefault(x => x.OrgId == eventItem.OrgId && x.UserId == userId);
        if (member is null)
            throw new UnauthorizedAccessException("You do not have permission to manage this event.");

        var roleName = ResolveRoleNameFromMember(data, member);
        var canPlan = roleName.Trim().ToUpperInvariant() is "PRESIDENT" or "VICEPRESIDENT" or "MANAGER";
        var isCoordinator = data.EventMembers.Any(x =>
            x.EventId == eventItem.Id
            && x.MemberId == member.Id
            && string.Equals(x.EventRole, "Coordinator", StringComparison.OrdinalIgnoreCase));

        if (!canPlan && !isCoordinator)
            throw new UnauthorizedAccessException("You do not have permission to manage this event.");
    }

    private static string ResolveRoleNameFromMember(MockDataSet data, MockMember member)
    {
        if (!member.RoleId.HasValue)
            return "Member";

        return data.OrganizationRoles.FirstOrDefault(x => x.Id == member.RoleId.Value)?.RoleName ?? "Member";
    }

    private static EventCategoryViewModel MapCategory(MockDataSet data, MockEventCategory category)
    {
        var leadName = "Chua gan";
        string? leadAvatarUrl = null;

        if (category.LeadMemberId.HasValue)
        {
            var member = data.Members.FirstOrDefault(x => x.Id == category.LeadMemberId.Value);
            if (member is not null)
            {
                leadName = member.DisplayName;
                leadAvatarUrl = data.Users.FirstOrDefault(x => x.Id == member.UserId)?.AvatarUrl;
            }
        }

        var categoryTasks = data.Tasks.Where(x => x.CategoryId == category.Id).ToList();
        var totalTasks = categoryTasks.Count;
        var completedTasks = categoryTasks.Count(x => string.Equals(x.Status, "DONE", StringComparison.OrdinalIgnoreCase));

        var responsibleMembers = new List<CategoryMemberViewModel>();
        if (category.LeadMemberId.HasValue)
        {
            var lead = data.Members.FirstOrDefault(x => x.Id == category.LeadMemberId.Value);
            if (lead is not null)
            {
                responsibleMembers.Add(new CategoryMemberViewModel
                {
                    MemberId = lead.Id,
                    Name = lead.DisplayName,
                    Role = "Category Lead",
                    AvatarUrl = data.Users.FirstOrDefault(u => u.Id == lead.UserId)?.AvatarUrl
                });
            }
        }

        return new EventCategoryViewModel
        {
            Id = category.Id,
            MilestoneId = category.MilestoneId,
            Name = category.Name,
            Description = category.Description,
            LeadName = leadName,
            LeadAvatarUrl = leadAvatarUrl,
            ActiveSubtasks = Math.Max(0, totalTasks - completedTasks),
            ProgressPercentage = totalTasks == 0 ? 0 : (int)Math.Round((double)completedTasks * 100 / totalTasks, MidpointRounding.AwayFromZero),
            DetailedDescription = category.Description,
            Guidelines = category.Guidelines,
            IsUrgent = category.IsUrgent,
            ResponsibleMembers = responsibleMembers
        };
    }

    private static string NormalizeName(string? value)
        => string.IsNullOrWhiteSpace(value) ? "Untitled Category" : value.Trim();
}
