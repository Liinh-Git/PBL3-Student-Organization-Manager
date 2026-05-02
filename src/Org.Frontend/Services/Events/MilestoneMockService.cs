using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public sealed class MilestoneMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IMilestoneService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<List<MilestoneViewModel>> GetMilestonesAsync(Guid eventId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var eventItem = data.Events.FirstOrDefault(x => x.Id == eventId)
                ?? throw new KeyNotFoundException($"Event {eventId} not found in mock data.");

            EnsureCanReadEvent(data, eventItem, currentUserId.Value);

            return data.Milestones
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.OrderIndex)
                .Select(MapMilestone)
                .ToList();
        });
    }

    public async Task<MilestoneViewModel> CreateMilestoneAsync(CreateMilestoneViewModel req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var eventItem = data.Events.FirstOrDefault(x => x.Id == req.EventId)
                ?? throw new KeyNotFoundException($"Event {req.EventId} not found in mock data.");

            EnsureCanManageEvent(data, eventItem, currentUserId.Value);

            var start = req.StartDate?.Date ?? DateTime.Today;
            var end = req.EndDate?.Date ?? start;
            if (end < start)
                end = start;

            var item = new MockMilestone
            {
                Id = Guid.NewGuid(),
                EventId = req.EventId,
                Name = NormalizeName(req.Name),
                StartDate = start,
                EndDate = end,
                OrderIndex = req.OrderIndex <= 0
                    ? data.Milestones.Where(x => x.EventId == req.EventId).Select(x => x.OrderIndex).DefaultIfEmpty(0).Max() + 1
                    : req.OrderIndex
            };

            data.Milestones.Add(item);
            return MapMilestone(item);
        });
    }

    public async Task<MilestoneViewModel> UpdateMilestoneAsync(Guid milestoneId, UpdateMilestoneViewModel req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var current = data.Milestones.FirstOrDefault(x => x.Id == milestoneId)
                ?? throw new KeyNotFoundException($"Milestone {milestoneId} not found in mock data.");

            var eventItem = data.Events.FirstOrDefault(x => x.Id == current.EventId)
                ?? throw new KeyNotFoundException($"Event {current.EventId} not found in mock data.");

            EnsureCanManageEvent(data, eventItem, currentUserId.Value);

            current.Name = NormalizeName(req.Name);
            current.StartDate = req.StartDate?.Date ?? current.StartDate;
            current.EndDate = req.EndDate?.Date ?? current.EndDate;
            if (current.EndDate < current.StartDate)
                current.EndDate = current.StartDate;
            current.OrderIndex = req.OrderIndex <= 0 ? current.OrderIndex : req.OrderIndex;

            return MapMilestone(current);
        });
    }

    public async Task DeleteMilestoneAsync(Guid milestoneId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Milestones.FirstOrDefault(x => x.Id == milestoneId)
                ?? throw new KeyNotFoundException($"Milestone {milestoneId} not found in mock data.");

            var eventItem = data.Events.FirstOrDefault(x => x.Id == current.EventId)
                ?? throw new KeyNotFoundException($"Event {current.EventId} not found in mock data.");

            EnsureCanManageEvent(data, eventItem, currentUserId.Value);

            var categoryIds = data.EventCategories
                .Where(x => x.MilestoneId == milestoneId)
                .Select(x => x.Id)
                .ToHashSet();

            data.Tasks.RemoveAll(x => categoryIds.Contains(x.CategoryId));
            data.EventCategories.RemoveAll(x => x.MilestoneId == milestoneId);
            data.Milestones.Remove(current);

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

    private static MilestoneViewModel MapMilestone(MockMilestone x)
    {
        return new MilestoneViewModel
        {
            Id = x.Id,
            EventId = x.EventId,
            Name = x.Name,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            OrderIndex = x.OrderIndex
        };
    }

    private static string NormalizeName(string? value)
        => string.IsNullOrWhiteSpace(value) ? "Untitled Milestone" : value.Trim();
}
