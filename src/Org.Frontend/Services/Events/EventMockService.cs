using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.Services.Organizations;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public sealed class EventMockService(
    FrontendMockDataStore mockDataStore,
    IOrganizationContext organizationContext,
    AuthenticationStateProvider authStateProvider) : IEventService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly IOrganizationContext _organizationContext = organizationContext;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<List<EventViewModel>> GetEventsAsync(Guid orgId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data => data.Events
            .Where(x => x.OrgId == orgId)
            .OrderByDescending(x => x.StartDate)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => MapEventCard(x, data, currentUserId))
            .ToList());
    }

    public async Task<MyEventsViewModel> GetMyEventsAsync()
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            return new MyEventsViewModel();

        return await _mockDataStore.UseAsync(data =>
        {
            var userMemberIds = data.Members
                .Where(x => x.UserId == currentUserId.Value)
                .Select(x => x.Id)
                .ToHashSet();

            var managedOrgIds = data.Members
                .Where(x => x.UserId == currentUserId.Value)
                .Where(x => HasOrgPlanPermission(ResolveRoleNameFromMember(data, x)))
                .Select(x => x.OrgId)
                .ToHashSet();

            var explicitOrganizerEventIds = data.EventMembers
                .Where(x => userMemberIds.Contains(x.MemberId))
                .Where(x => HasEventCoordinatorPermission(x.EventRole))
                .Select(x => x.EventId)
                .ToHashSet();

            var organizerEventIds = data.Events
                .Where(x => managedOrgIds.Contains(x.OrgId) || explicitOrganizerEventIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToHashSet();

            var attendeeEventIds = data.Attendees
                .Where(x => x.UserId == currentUserId.Value && !string.Equals(x.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.EventId)
                .ToHashSet();

            var organizerEvents = data.Events
                .Where(x => organizerEventIds.Contains(x.Id))
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => MapMyEventItem(x, data, isOrganizer: true))
                .ToList();

            var attendeeEvents = data.Events
                .Where(x => attendeeEventIds.Contains(x.Id) && !organizerEventIds.Contains(x.Id))
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => MapMyEventItem(x, data, isOrganizer: false))
                .ToList();

            return new MyEventsViewModel
            {
                OrganizerEvents = organizerEvents,
                AttendeeEvents = attendeeEvents
            };
        });
    }

    public Task<EventViewModel?> GetPublicEventDetailAsync(Guid eventId)
        => GetEventDetailAsync(eventId);

    public async Task<bool> CanCreateEventAsync(Guid orgId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            return false;

        return await _mockDataStore.UseAsync(data => CanCreateEventInternal(data, orgId, currentUserId.Value));
    }

    public async Task<bool> CanManageEventAsync(Guid eventId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            return false;

        return await _mockDataStore.UseAsync(data =>
        {
            var source = data.Events.FirstOrDefault(x => x.Id == eventId);
            return source is not null && CanManageEventInternal(data, source, currentUserId.Value);
        });
    }

    public async Task<EventViewModel> CreateEventAsync(CreateEventViewModel req)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync();
        var currentUserId = await TryGetCurrentUserIdAsync();

        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            if (!data.Organizations.Any(x => x.Id == orgId))
                throw new KeyNotFoundException($"Organization {orgId} not found in mock data.");

            if (!CanCreateEventInternal(data, orgId, currentUserId.Value))
                throw new UnauthorizedAccessException("You do not have permission to create events in this organization.");

            var normalizedName = NormalizeTitle(req.Title);
            if (normalizedName.Length < 2)
                throw new InvalidOperationException("Event title must be at least 2 characters.");

            var startDate = DateOnly.FromDateTime((req.StartDate ?? DateTime.Today).Date);
            var endDate = DateOnly.FromDateTime((req.EndDate ?? req.StartDate ?? DateTime.Today).Date);
            if (endDate < startDate)
                endDate = startDate;

            var item = new MockEvent
            {
                Id = Guid.NewGuid(),
                OrgId = orgId,
                Name = normalizedName,
                Description = NormalizeOptional(req.Description),
                StartDate = startDate,
                EndDate = endDate,
                StatusLabel = DeriveStatusLabel(startDate, endDate),
                Location = NormalizeOptional(req.Location),
                TotalSlots = Math.Max(0, req.ExpectedParticipants),
                ImageUrl = null,
                CompletionLabel = null,
                BudgetLabel = null,
                RiskLevel = null,
                TotalFiles = 0,
                ActualSpending = 0,
                Tags = NormalizeTags(req.Tags)
            };

            data.Events.Add(item);

            var currentMember = data.Members.FirstOrDefault(x => x.UserId == currentUserId.Value && x.OrgId == orgId);
            if (currentMember is not null)
            {
                data.EventMembers.Add(new MockEventMember
                {
                    EventId = item.Id,
                    MemberId = currentMember.Id,
                    EventRole = "Coordinator"
                });
            }

            return MapEventCard(item, data, currentUserId);
        });
    }

    public async Task<EventViewModel?> GetEventDetailAsync(Guid eventId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            var source = data.Events.FirstOrDefault(x => x.Id == eventId);
            return source is null ? null : MapEventDetail(source, data, currentUserId);
        });
    }

    public async Task<EventViewModel> UpdateEventAsync(Guid eventId, UpdateEventViewModel req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            var source = data.Events.FirstOrDefault(x => x.Id == eventId)
                ?? throw new KeyNotFoundException($"Event {eventId} not found in mock data.");

            if (!CanManageEventInternal(data, source, currentUserId.Value))
                throw new UnauthorizedAccessException("You do not have permission to edit this event.");

            if (!string.IsNullOrWhiteSpace(req.Name))
                source.Name = req.Name.Trim();

            if (req.Description is not null)
                source.Description = NormalizeOptional(req.Description);

            if (req.StartDate.HasValue)
                source.StartDate = DateOnly.FromDateTime(req.StartDate.Value.Date);

            if (req.EndDate.HasValue)
                source.EndDate = DateOnly.FromDateTime(req.EndDate.Value.Date);

            if (source.EndDate < source.StartDate)
                source.EndDate = source.StartDate;

            if (req.Location is not null)
                source.Location = NormalizeOptional(req.Location);

            if (req.TotalSlots.HasValue)
                source.TotalSlots = Math.Max(0, req.TotalSlots.Value);

            source.StatusLabel = DeriveStatusLabel(source.StartDate, source.EndDate);
            return MapEventDetail(source, data, currentUserId);
        });
    }

    public async Task DeleteEventAsync(Guid eventId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        await _mockDataStore.UseAsync(data =>
        {
            var source = data.Events.FirstOrDefault(x => x.Id == eventId)
                ?? throw new KeyNotFoundException($"Event {eventId} not found in mock data.");

            if (!CanManageEventInternal(data, source, currentUserId.Value))
                throw new UnauthorizedAccessException("You do not have permission to delete this event.");

            var milestoneIds = data.Milestones
                .Where(x => x.EventId == eventId)
                .Select(x => x.Id)
                .ToHashSet();

            var categoryIds = data.EventCategories
                .Where(x => milestoneIds.Contains(x.MilestoneId))
                .Select(x => x.Id)
                .ToHashSet();

            data.Tasks.RemoveAll(x => categoryIds.Contains(x.CategoryId));
            data.EventCategories.RemoveAll(x => milestoneIds.Contains(x.MilestoneId));
            data.Milestones.RemoveAll(x => x.EventId == eventId);
            data.Attendees.RemoveAll(x => x.EventId == eventId);
            data.EventMembers.RemoveAll(x => x.EventId == eventId);
            data.Events.Remove(source);

            return 0;
        });
    }

    public async Task RegisterEventAsync(Guid eventId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        await _mockDataStore.UseAsync(data =>
        {
            if (!data.Events.Any(x => x.Id == eventId))
                throw new KeyNotFoundException($"Event {eventId} not found in mock data.");

            var existing = data.Attendees.FirstOrDefault(x => x.EventId == eventId && x.UserId == currentUserId.Value);
            if (existing is not null)
            {
                existing.Status = "REGISTERED";
            }
            else
            {
                data.Attendees.Add(new MockAttendee
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = currentUserId.Value,
                    Status = "REGISTERED",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return 0;
        });
    }

    public async Task UnregisterEventAsync(Guid eventId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        await _mockDataStore.UseAsync(data =>
        {
            var existing = data.Attendees.FirstOrDefault(x => x.EventId == eventId && x.UserId == currentUserId.Value);
            if (existing is not null)
                existing.Status = "CANCELLED";
            return 0;
        });
    }

    private static EventViewModel MapEventCard(MockEvent source, MockDataSet data, Guid? currentUserId)
    {
        var attendeeCount = data.Attendees.Count(x =>
            x.EventId == source.Id
            && !string.Equals(x.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase));

        var (totalTasks, completedTasks) = CountTaskProgress(data, source.Id);
        var canManage = currentUserId.HasValue && CanManageEventInternal(data, source, currentUserId.Value);

        return new EventViewModel
        {
            Id = source.Id,
            OrganizationId = source.OrgId,
            Name = source.Name,
            Description = source.Description,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            StatusLabel = DeriveStatusLabel(source.StartDate, source.EndDate),
            Location = source.Location,
            RegisteredCount = attendeeCount,
            TotalSlots = source.TotalSlots,
            ImageUrl = source.ImageUrl,
            CompletionLabel = source.CompletionLabel ?? ToPercentLabel(completedTasks, totalTasks),
            CompletionPercentage = ToPercent(completedTasks, totalTasks),
            BudgetLabel = source.BudgetLabel,
            RiskLevel = source.RiskLevel,
            TotalFiles = source.TotalFiles,
            ActualSpending = source.ActualSpending,
            CanManage = canManage,
            CanEnterWorkspace = canManage
        };
    }

    private static EventViewModel MapEventDetail(MockEvent source, MockDataSet data, Guid? currentUserId)
        => MapEventCard(source, data, currentUserId);

    private static MyEventItemViewModel MapMyEventItem(MockEvent source, MockDataSet data, bool isOrganizer)
    {
        var org = data.Organizations.FirstOrDefault(x => x.Id == source.OrgId);
        var status = DeriveStatusLabel(source.StartDate, source.EndDate);
        return new MyEventItemViewModel
        {
            EventId = source.Id,
            OrganizationId = source.OrgId,
            OrganizationName = org?.OrgName ?? "Organization",
            Name = source.Name,
            Description = source.Description,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            StatusLabel = status,
            Location = source.Location,
            ImageUrl = source.ImageUrl,
            IsOrganizer = isOrganizer,
            CanEnterWorkspace = isOrganizer,
            CanManage = isOrganizer
        };
    }

    private static (int TotalTasks, int CompletedTasks) CountTaskProgress(MockDataSet data, Guid eventId)
    {
        var milestoneIds = data.Milestones
            .Where(x => x.EventId == eventId)
            .Select(x => x.Id)
            .ToHashSet();

        var categoryIds = data.EventCategories
            .Where(x => milestoneIds.Contains(x.MilestoneId))
            .Select(x => x.Id)
            .ToHashSet();

        var tasks = data.Tasks.Where(x => categoryIds.Contains(x.CategoryId)).ToList();
        var totalTasks = tasks.Count;
        var completedTasks = tasks.Count(x => string.Equals(x.Status, "DONE", StringComparison.OrdinalIgnoreCase));
        return (totalTasks, completedTasks);
    }

    private async Task<Guid?> TryGetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }

    private static bool CanCreateEventInternal(MockDataSet data, Guid orgId, Guid userId)
    {
        var member = data.Members.FirstOrDefault(x => x.OrgId == orgId && x.UserId == userId);
        if (member is null)
            return false;

        var roleName = ResolveRoleNameFromMember(data, member);
        return HasOrgPlanPermission(roleName);
    }

    private static bool CanManageEventInternal(MockDataSet data, MockEvent source, Guid userId)
    {
        var member = data.Members.FirstOrDefault(x => x.OrgId == source.OrgId && x.UserId == userId);
        if (member is null)
            return false;

        var roleName = ResolveRoleNameFromMember(data, member);
        if (HasOrgPlanPermission(roleName))
            return true;

        var eventMembership = data.EventMembers.FirstOrDefault(x => x.EventId == source.Id && x.MemberId == member.Id);
        return eventMembership is not null && HasEventCoordinatorPermission(eventMembership.EventRole);
    }

    private static string ResolveRoleNameFromMember(MockDataSet data, MockMember member)
    {
        if (!member.RoleId.HasValue)
            return "Member";

        var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == member.RoleId.Value);
        return role?.RoleName ?? "Member";
    }

    private static bool HasOrgPlanPermission(string? roleName)
    {
        return roleName?.Trim().ToUpperInvariant() switch
        {
            "PRESIDENT" => true,
            "VICEPRESIDENT" => true,
            "MANAGER" => true,
            _ => false
        };
    }

    private static bool HasEventCoordinatorPermission(string? eventRole)
    {
        return eventRole?.Trim().ToUpperInvariant() switch
        {
            "COORDINATOR" => true,
            "ORGANIZER" => true,
            "OWNER" => true,
            "MANAGER" => true,
            _ => false
        };
    }

    private static int ToPercent(int completedTasks, int totalTasks)
        => totalTasks <= 0 ? 0 : (int)Math.Round((double)completedTasks * 100 / totalTasks, MidpointRounding.AwayFromZero);

    private static string ToPercentLabel(int completedTasks, int totalTasks)
        => $"{ToPercent(completedTasks, totalTasks)}%";

    private static string NormalizeTitle(string? title)
        => string.IsNullOrWhiteSpace(title) ? string.Empty : title.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<string> NormalizeTags(IReadOnlyList<string>? tags)
    {
        if (tags is null)
            return [];

        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string DeriveStatusLabel(DateOnly startDate, DateOnly endDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (endDate < today)
            return "COMPLETED";
        if (startDate <= today)
            return "ONGOING";
        return "UPCOMING";
    }
}
