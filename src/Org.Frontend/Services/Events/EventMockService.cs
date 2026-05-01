using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
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

    public Task<List<EventViewModel>> GetEventsAsync(Guid orgId)
    {
        return _mockDataStore.UseAsync(data => data.Events
            .Where(x => x.OrgId == orgId)
            .OrderByDescending(x => x.StartDate)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => MapEventCard(x, data))
            .ToList());
    }

    public async Task<EventViewModel> CreateEventAsync(CreateEventViewModel req)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            if (!data.Organizations.Any(x => x.Id == orgId))
            {
                throw new KeyNotFoundException($"Organization {orgId} not found in mock data.");
            }

            var startDate = DateOnly.FromDateTime(req.Date ?? DateTime.Today);
            var item = new MockEvent
            {
                Id = Guid.NewGuid(),
                OrgId = orgId,
                Name = NormalizeTitle(req.Title),
                Description = "New event created from FE mock workflow.",
                StartDate = startDate,
                EndDate = startDate.AddDays(1),
                StatusLabel = "UPCOMING",
                Location = string.IsNullOrWhiteSpace(req.Location) ? "To be announced" : req.Location.Trim(),
                TotalSlots = req.TotalSlots <= 0 ? 150 : req.TotalSlots,
                ImageUrl = "/images/mockimages/Org1/Card1.jpg",
                CompletionLabel = "0%",
                BudgetLabel = "0%",
                RiskLevel = "Low",
                TotalFiles = 0,
                ActualSpending = 0
            };

            data.Events.Add(item);
            return MapEventCard(item, data);
        });
    }

    public Task<EventViewModel?> GetEventDetailAsync(Guid eventId)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var source = data.Events.FirstOrDefault(x => x.Id == eventId);
            return source is null ? null : MapEventDetail(source, data);
        });
    }

    public Task<EventViewModel> UpdateEventAsync(Guid eventId, UpdateEventViewModel req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var source = data.Events.FirstOrDefault(x => x.Id == eventId)
                ?? throw new KeyNotFoundException($"Event {eventId} not found in mock data.");

            source.Name = string.IsNullOrWhiteSpace(req.Name) ? source.Name : req.Name.Trim();
            if (req.Description is not null)
            {
                source.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
            }

            if (req.StartDate.HasValue)
            {
                source.StartDate = DateOnly.FromDateTime(req.StartDate.Value.Date);
            }

            if (req.EndDate.HasValue)
            {
                source.EndDate = DateOnly.FromDateTime(req.EndDate.Value.Date);
            }

            if (source.EndDate < source.StartDate)
            {
                source.EndDate = source.StartDate;
            }

            if (req.Location is not null)
            {
                source.Location = string.IsNullOrWhiteSpace(req.Location) ? null : req.Location.Trim();
            }

            if (req.TotalSlots.HasValue)
            {
                source.TotalSlots = Math.Max(0, req.TotalSlots.Value);
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            source.StatusLabel = source.EndDate < today
                ? "COMPLETED"
                : source.StartDate <= today
                    ? "ONGOING"
                    : "UPCOMING";

            return MapEventDetail(source, data);
        });
    }

    public async Task DeleteEventAsync(Guid eventId)
    {
        await _mockDataStore.UseAsync(data =>
        {
            var source = data.Events.FirstOrDefault(x => x.Id == eventId)
                ?? throw new KeyNotFoundException($"Event {eventId} not found in mock data.");

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
            data.EventMembers.RemoveAll(x => x.EventId == eventId);
            data.Events.Remove(source);

            return 0;
        });
    }

    public async Task RegisterEventAsync(Guid eventId)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdStr = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) throw new UnauthorizedAccessException("User not logged in.");
        var userId = Guid.Parse(userIdStr);

        await _mockDataStore.UseAsync(data =>
        {
            var existing = data.Attendees.FirstOrDefault(x => x.EventId == eventId && x.UserId == userId);
            if (existing != null)
            {
                existing.Status = "REGISTERED";
            }
            else
            {
                data.Attendees.Add(new MockAttendee
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = userId,
                    Status = "REGISTERED",
                    CreatedAt = DateTime.UtcNow
                });
            }
            return 0;
        });
    }

    public async Task UnregisterEventAsync(Guid eventId)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdStr = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) throw new UnauthorizedAccessException("User not logged in.");
        var userId = Guid.Parse(userIdStr);

        await _mockDataStore.UseAsync(data =>
        {
            var existing = data.Attendees.FirstOrDefault(x => x.EventId == eventId && x.UserId == userId);
            if (existing != null)
            {
                existing.Status = "CANCELLED";
            }
            return 0;
        });
    }

    private static EventViewModel MapEventCard(MockEvent source, MockDataSet data)
    {
        // Participant count from BOTH EventMembers (staff) and Attendees (registered users)
        var staffCount = data.EventMembers.Count(x => x.EventId == source.Id);
        var attendeeCount = data.Attendees.Count(x => x.EventId == source.Id && string.Equals(x.Status, "REGISTERED", StringComparison.OrdinalIgnoreCase));
        
        var (totalTasks, completedTasks) = CountTaskProgress(data, source.Id);

        return new EventViewModel
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            StatusLabel = source.StatusLabel,
            Location = source.Location,
            RegisteredCount = attendeeCount, // UI uses this as attendee count
            TotalSlots = source.TotalSlots,
            ImageUrl = source.ImageUrl,
            CompletionLabel = source.CompletionLabel ?? ToPercentLabel(completedTasks, totalTasks),
            CompletionPercentage = ToPercent(completedTasks, totalTasks),
            BudgetLabel = source.BudgetLabel,
            RiskLevel = source.RiskLevel,
            TotalFiles = source.TotalFiles,
            ActualSpending = source.ActualSpending
        };
    }


    private static EventViewModel MapEventDetail(MockEvent source, MockDataSet data)
    {
        var model = MapEventCard(source, data);
        var (totalTasks, completedTasks) = CountTaskProgress(data, source.Id);
        var completion = ToPercent(completedTasks, totalTasks);

        model.Description ??= "Detailed plan and execution notes are tracked in the mock dataset.";
        model.CompletionLabel ??= $"{completion}%";
        model.BudgetLabel ??= $"{Math.Clamp(completion + 10, 0, 100)}%";
        model.RiskLevel ??= completion >= 80 ? "Low" : completion >= 40 ? "Medium" : "High";

        return model;
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

        var tasks = data.Tasks
            .Where(x => categoryIds.Contains(x.CategoryId))
            .ToList();

        var totalTasks = tasks.Count;
        var completedTasks = tasks.Count(x => string.Equals(x.Status, "DONE", StringComparison.OrdinalIgnoreCase));
        return (totalTasks, completedTasks);
    }

    private static int ToPercent(int completedTasks, int totalTasks)
        => totalTasks <= 0 ? 0 : (int)Math.Round((double)completedTasks * 100 / totalTasks, MidpointRounding.AwayFromZero);

    private static string ToPercentLabel(int completedTasks, int totalTasks)
        => $"{ToPercent(completedTasks, totalTasks)}%";

    private static string NormalizeTitle(string? title)
        => string.IsNullOrWhiteSpace(title) ? "Untitled Event" : title.Trim();
}