using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Attendees.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Services;

public class AttendeeService : IAttendeeService
{
    private readonly AppDbContext _context;

    public AttendeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AttendeeDto>> GetEventAttendeesAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        var isOrgMember = await _context.Members
            .AnyAsync(m => m.OrgId == evt.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isOrgMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this attendee list");
        }

        var attendees = await _context.Attendees
            .Include(a => a.User)
            .Where(a => a.EventId == eventId && a.Status != AttendeeStatus.Cancelled)
            .OrderByDescending(a => a.RegisteredAt)
            .ToListAsync(ct);

        return attendees.Select(a => a.ToAttendeeDto()).ToList();
    }

    public async Task<AttendeeDto?> GetMyRegistrationAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        await EnsurePublicEventExists(eventId, ct);

        var attendee = await _context.Attendees
            .Include(a => a.User)
            .Where(a => a.EventId == eventId && a.UserId == userId)
            .OrderByDescending(a => a.RegisteredAt)
            .FirstOrDefaultAsync(ct);

        return attendee?.ToAttendeeDto();
    }

    public async Task<AttendeeDto> RegisterAsync(Guid eventId, Guid userId, RegisterEventAttendeeRequest request, CancellationToken ct = default)
    {
        var evt = await EnsurePublicEventExists(eventId, ct);

        if (evt.Status == EventStatus.Cancelled)
        {
            throw new InvalidOperationException("This event has been cancelled");
        }

        var attendee = await _context.Attendees
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId, ct);

        if (attendee != null)
        {
            if (attendee.Status != AttendeeStatus.Cancelled)
            {
                return attendee.ToAttendeeDto();
            }

            attendee.Status = AttendeeStatus.Registered;
            attendee.RegisteredAt = DateTime.UtcNow;
            attendee.CheckedInAt = null;
            attendee.Note = request.Note?.Trim();
            attendee.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            attendee = new Attendee
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Status = AttendeeStatus.Registered,
                RegisteredAt = DateTime.UtcNow,
                Note = request.Note?.Trim()
            };
            _context.Attendees.Add(attendee);
        }

        await _context.SaveChangesAsync(ct);

        var savedAttendee = await _context.Attendees
            .Include(a => a.User)
            .Where(a => a.Id == attendee.Id)
            .FirstAsync(ct);

        return savedAttendee.ToAttendeeDto();
    }

    public async Task<AttendeeDto> CancelRegistrationAsync(Guid eventId, Guid userId, CancelEventRegistrationRequest request, CancellationToken ct = default)
    {
        await EnsurePublicEventExists(eventId, ct);

        var attendee = await _context.Attendees
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId, ct);

        if (attendee == null || attendee.Status == AttendeeStatus.Cancelled)
        {
            throw new KeyNotFoundException("Registration not found");
        }

        attendee.Status = AttendeeStatus.Cancelled;
        attendee.Note = request.Note?.Trim() ?? attendee.Note;
        attendee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return attendee.ToAttendeeDto();
    }

    private async Task<Event> EnsurePublicEventExists(Guid eventId, CancellationToken ct)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        if (evt.Visibility != EventVisibility.Public)
        {
            throw new UnauthorizedAccessException("This event is not public");
        }

        return evt;
    }
}
