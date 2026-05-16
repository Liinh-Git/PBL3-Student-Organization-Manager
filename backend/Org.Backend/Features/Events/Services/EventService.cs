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
            Status = EventStatus.Draft,
            CreatedByMemberId = member.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Events.Add(evt);
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

        // Verify user has org.events.manage permission
        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.events.manage");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage events");
        }

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

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.events.manage");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage events");
        }

        if (!Enum.TryParse<EventStatus>(request.Status, true, out var nextStatus) ||
            (nextStatus != EventStatus.Draft && nextStatus != EventStatus.Published))
        {
            throw new InvalidOperationException("Status must be 'Draft' or 'Published'");
        }

        evt.Status = nextStatus;
        if (nextStatus == EventStatus.Published)
        {
            evt.Visibility = EventVisibility.Public;
        }
        evt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return evt.ToEventDto();
    }

    public async Task<bool> DeleteEventAsync(Guid eventId, Guid userId, CancellationToken ct = default)
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

        // Verify user has org.events.manage permission
        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.events.manage");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage events");
        }

        // Soft delete: set status to Cancelled
        evt.Status = EventStatus.Cancelled;
        evt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return true;
    }
}
