using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Events.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _context;

    public EventService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventSummaryDto>> GetOrganizationEventsAsync(Guid orgId, Guid userId, CancellationToken ct = default)
    {
        // Verify user is a member of this organization
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }

        // Get all events for this organization
        var events = await _context.Events
            .Where(e => e.OrgId == orgId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(ct);

        return events.Select(e => e.ToEventSummaryDto()).ToList();
    }

    public async Task<EventDto> GetEventByIdAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        // Verify user is a member of the event's organization
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == evt.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this event");
        }

        return evt.ToEventDto();
    }

    public async Task<List<EventPublicDto>> GetPublicEventsAsync(CancellationToken ct = default)
    {
        // Get all public events
        var events = await _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Attendees)
            .Where(e => e.Visibility == EventVisibility.Public && e.Status == EventStatus.Published)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(ct);

        return events.Select(e => e.ToEventPublicDto()).ToList();
    }

    public async Task<EventPublicDto> GetPublicEventByIdAsync(Guid eventId, CancellationToken ct = default)
    {
        var evt = await _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        // Verify event is public
        if (evt.Visibility != EventVisibility.Public)
        {
            throw new UnauthorizedAccessException("This event is not public");
        }

        return evt.ToEventPublicDto();
    }

    public async Task<EventDto> CreateEventAsync(Guid orgId, Guid userId, CreateEventRequest request, CancellationToken ct = default)
    {
        // Verify user is active member of organization
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        // Verify user has org.events.create permission
        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.events.create");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to create events");
        }

        // Parse visibility
        EventVisibility visibility = EventVisibility.Private;
        if (!string.IsNullOrEmpty(request.Visibility))
        {
            if (!Enum.TryParse<EventVisibility>(request.Visibility, true, out visibility))
            {
                throw new InvalidOperationException($"Invalid visibility value: {request.Visibility}");
            }
        }

        // Validate dates
        var endDate = request.EndDate ?? request.StartDate;
        if (endDate < request.StartDate)
        {
            throw new InvalidOperationException("End date must be greater than or equal to start date");
        }

        var utcStartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        var utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        // Create event
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            OrgId = orgId,
            EventName = request.EventName,
            Description = request.Description,
            StartDate = utcStartDate,
            EndDate = utcEndDate,
            Location = request.Location,
            BannerUrl = request.BannerUrl,
            Visibility = visibility,
            TargetParticipants = request.TargetParticipants,
            RegisteredParticipants = 0,
            Status = EventStatus.Draft,
            CreatedByMemberId = member.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Events.Add(evt);
        await _context.SaveChangesAsync(ct);

        var initialMemberIds = new HashSet<Guid>();
        initialMemberIds.Add(member.Id);
        if (request.InitialMemberIds != null)
        {
            foreach (var id in request.InitialMemberIds)
            {
                if (id != Guid.Empty)
                {
                    initialMemberIds.Add(id);
                }
            }
        }

        var validMemberIds = await _context.Members
            .Where(m => initialMemberIds.Contains(m.Id) && m.OrgId == orgId && m.Status == MemberStatus.Active)
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (validMemberIds.Count != initialMemberIds.Count)
        {
            throw new InvalidOperationException("Some initial event organizers are invalid or inactive");
        }

        var now = DateTime.UtcNow;
        var organizers = validMemberIds.Select(memberId => new EventMember
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            MemberId = memberId,
            EventRole = EventRole.Manager,
            AssignedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        });

        _context.EventMembers.AddRange(organizers);
        await _context.SaveChangesAsync(ct);

        return evt.ToEventDto();
    }

    public async Task<EventDto> UpdateEventAsync(Guid eventId, Guid userId, UpdateEventRequest request, CancellationToken ct = default)
    {
        // Find event
        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        // Verify user is active member of organization
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == evt.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        await EnsureEventManagerAsync(evt.Id, userId, ct);

        // Parse visibility
        EventVisibility visibility = evt.Visibility;
        if (!string.IsNullOrEmpty(request.Visibility))
        {
            if (!Enum.TryParse<EventVisibility>(request.Visibility, true, out visibility))
            {
                throw new InvalidOperationException($"Invalid visibility value: {request.Visibility}");
            }
        }

        // Validate dates
        var endDate = request.EndDate ?? request.StartDate;
        if (endDate < request.StartDate)
        {
            throw new InvalidOperationException("End date must be greater than or equal to start date");
        }

        var utcStartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        var utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        // Update event
        evt.EventName = request.EventName;
        evt.Description = request.Description;
        evt.StartDate = utcStartDate;
        evt.EndDate = utcEndDate;
        evt.Location = request.Location;
        evt.BannerUrl = request.BannerUrl;
        evt.Visibility = visibility;
        evt.TargetParticipants = request.TargetParticipants;
        evt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return evt.ToEventDto();
    }

    public async Task<EventDto> UpdateEventStatusAsync(Guid eventId, Guid userId, UpdateEventStatusRequest request, CancellationToken ct = default)
    {
        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == evt.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        await EnsureEventManagerAsync(evt.Id, userId, ct);

        if (!Enum.TryParse<EventStatus>(request.Status, true, out var nextStatus))
        {
            throw new InvalidOperationException("Invalid event status");
        }

        if (evt.Status == EventStatus.Cancelled && nextStatus != EventStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled events cannot change to another status");
        }

        if (evt.Status == EventStatus.Completed && nextStatus != EventStatus.Completed)
        {
            throw new InvalidOperationException("Completed events cannot change to another status");
        }

        if (nextStatus == EventStatus.Draft)
        {
            if (evt.Status != EventStatus.Published)
            {
                throw new InvalidOperationException("Only published events can move back to draft");
            }

            if (evt.Visibility == EventVisibility.Public)
            {
                throw new InvalidOperationException("Public published events cannot move back to draft");
            }
        }

        if (nextStatus == EventStatus.Published)
        {
            if (evt.Status != EventStatus.Draft)
            {
                throw new InvalidOperationException("Only draft events can be published");
            }
        }

        if (nextStatus == EventStatus.Completed)
        {
            var eventEndUtc = evt.EndDate.Kind == DateTimeKind.Utc
                ? evt.EndDate
                : DateTime.SpecifyKind(evt.EndDate, DateTimeKind.Utc);

            if (eventEndUtc > DateTime.UtcNow)
            {
                throw new InvalidOperationException("Can only mark event as Completed after event end time");
            }

            if (evt.Status != EventStatus.Published && evt.Status != EventStatus.Ongoing)
            {
                throw new InvalidOperationException("Only published/ongoing events can be marked as completed");
            }
        }

        if (nextStatus == EventStatus.Ongoing)
        {
            if (evt.Status != EventStatus.Published)
            {
                throw new InvalidOperationException("Only published events can move to ongoing");
            }
        }

        evt.Status = nextStatus;
        evt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return evt.ToEventDto();
    }

    public async Task<bool> DeleteEventAsync(Guid eventId, Guid userId, bool hardDelete = false, CancellationToken ct = default)
    {
        // Find event
        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        // Verify user is active member of organization
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == evt.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        await EnsureEventManagerAsync(evt.Id, userId, ct);

        if (!hardDelete)
        {
            evt.Status = EventStatus.Cancelled;
            evt.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        var now = DateTime.UtcNow;

        evt.Status = EventStatus.Cancelled;
        evt.IsDeleted = true;
        evt.UpdatedAt = now;

        var milestones = await _context.Milestones
            .Where(m => m.EventId == eventId)
            .ToListAsync(ct);
        var milestoneIds = milestones.Select(m => m.Id).ToList();
        foreach (var milestone in milestones)
        {
            milestone.IsDeleted = true;
            milestone.UpdatedAt = now;
        }

        var categories = await _context.EventCategories
            .Where(c => milestoneIds.Contains(c.MilestoneId))
            .ToListAsync(ct);
        var categoryIds = categories.Select(c => c.Id).ToList();
        foreach (var category in categories)
        {
            category.IsDeleted = true;
            category.UpdatedAt = now;
        }

        var tasks = await _context.OrgTasks
            .Where(t => categoryIds.Contains(t.EventCategoryId))
            .ToListAsync(ct);
        foreach (var task in tasks)
        {
            task.IsDeleted = true;
            task.UpdatedAt = now;
        }

        var eventMembers = await _context.EventMembers
            .Where(em => em.EventId == eventId)
            .ToListAsync(ct);
        foreach (var eventMember in eventMembers)
        {
            eventMember.IsDeleted = true;
            eventMember.UpdatedAt = now;
        }

        var attendees = await _context.Attendees
            .Where(a => a.EventId == eventId)
            .ToListAsync(ct);
        foreach (var attendee in attendees)
        {
            attendee.IsDeleted = true;
            attendee.UpdatedAt = now;
        }

        var digitalAssets = await _context.DigitalAssets
            .Where(a => a.EventId == eventId)
            .ToListAsync(ct);
        foreach (var asset in digitalAssets)
        {
            asset.IsDeleted = true;
            asset.UpdatedAt = now;
        }

        var ratings = await _context.EventRatings
            .Where(r => r.EventId == eventId)
            .ToListAsync(ct);
        foreach (var rating in ratings)
        {
            rating.IsDeleted = true;
            rating.UpdatedAt = now;
        }

        var report = await _context.EventReports
            .FirstOrDefaultAsync(r => r.EventId == eventId, ct);
        if (report != null)
        {
            report.IsDeleted = true;
            report.UpdatedAt = now;
        }

        var resources = await _context.Resources
            .Where(r => r.EventId == eventId)
            .ToListAsync(ct);
        foreach (var resource in resources)
        {
            resource.IsDeleted = true;
            resource.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task EnsureEventManagerAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        var isEventMember = await _context.EventMembers
            .AnyAsync(
                em => em.EventId == eventId &&
                      em.Member.UserId == userId &&
                      em.Member.Status == MemberStatus.Active,
                ct);

        if (!isEventMember)
        {
            throw new UnauthorizedAccessException("Only event organizers can manage this event");
        }
    }
}
