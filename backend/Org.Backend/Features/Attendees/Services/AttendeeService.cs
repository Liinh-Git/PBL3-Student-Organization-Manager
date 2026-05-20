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

    public async Task<AttendeeRegistrationDto> GetMyRegistrationAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var evt = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        var isEventMember = await IsEventOrganizerAsync(evt, userId, ct);

        var attendee = await _context.Attendees
            .AsNoTracking()
            .Where(a => a.EventId == eventId && a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (attendee == null || attendee.Status == AttendeeStatus.Cancelled)
        {
            return new AttendeeRegistrationDto
            {
                EventId = eventId,
                UserId = userId,
                IsEventMember = isEventMember,
                IsRegistered = false,
                AttendeeId = attendee?.Id,
                Status = attendee?.Status.ToString(),
                RegisteredAtUtc = attendee?.RegisteredAt,
                CheckedInAtUtc = attendee?.CheckedInAt
            };
        }

        return attendee.ToRegistrationDto(userId, isEventMember);
    }

    public async Task<AttendeeDto> RegisterAsync(Guid eventId, Guid userId, RegisterEventAttendeeRequest request, CancellationToken ct = default)
    {
        var attendee = await RegisterInternalAsync(eventId, userId, request.Note, ct);

        var savedAttendee = await _context.Attendees
            .Include(a => a.User)
            .Where(a => a.Id == attendee.Id)
            .FirstAsync(ct);

        return savedAttendee.ToAttendeeDto();
    }

    public async Task<AttendeeRegistrationDto> RegisterMeAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var attendee = await RegisterInternalAsync(eventId, userId, note: null, ct);
        return attendee.ToRegistrationDto(userId, isEventMember: false);
    }

    public async Task<AttendeeDto> CancelRegistrationAsync(Guid eventId, Guid userId, CancelEventRegistrationRequest request, CancellationToken ct = default)
    {
        var attendee = await CancelInternalAsync(eventId, userId, request.Note, ct);

        return attendee!.ToAttendeeDto();
    }

    public async Task<AttendeeRegistrationDto> UnregisterAsync(
        Guid eventId,
        Guid userId,
        AttendeeRegistrationUpdateDto updateDto,
        CancellationToken ct = default)
    {
        var attendee = await CancelInternalAsync(eventId, userId, updateDto?.Note, ct, throwIfMissing: false);
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        var isEventMember = evt != null && await IsEventOrganizerAsync(evt, userId, ct);
        return attendee?.ToRegistrationDto(userId) ?? new AttendeeRegistrationDto
        {
            EventId = eventId,
            UserId = userId,
            IsEventMember = isEventMember,
            IsRegistered = false,
            Status = AttendeeStatus.Cancelled.ToString()
        };
    }

    public async Task<AttendeeRegistrationDto> RequestMyCheckInAsync(
        Guid eventId,
        Guid userId,
        RequestCheckInRequest request,
        CancellationToken ct = default)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        if (evt.Status is EventStatus.Cancelled or EventStatus.Completed)
        {
            throw new InvalidOperationException("This event is no longer accepting check-in requests");
        }

        var attendee = await _context.Attendees
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId, ct);

        if (attendee == null || attendee.Status == AttendeeStatus.Cancelled)
        {
            throw new InvalidOperationException("You are not registered for this event");
        }

        if (attendee.Status == AttendeeStatus.CheckedIn)
        {
            throw new InvalidOperationException("You have already checked in");
        }

        if (attendee.Status == AttendeeStatus.CheckInPending)
        {
            throw new InvalidOperationException("Your check-in request is already pending approval");
        }

        var nowUtc = DateTime.UtcNow;
        var startUtc = evt.StartDate.Kind == DateTimeKind.Utc
            ? evt.StartDate
            : DateTime.SpecifyKind(evt.StartDate, DateTimeKind.Utc);
        var openAtUtc = startUtc.AddHours(-1);
        var closeAtUtc = startUtc.Date.AddDays(1);

        if (nowUtc < openAtUtc)
        {
            throw new InvalidOperationException("Check-in opens 1 hour before the event starts");
        }

        if (nowUtc >= closeAtUtc)
        {
            throw new InvalidOperationException("Check-in is closed for this event");
        }

        attendee.Status = AttendeeStatus.CheckInPending;
        attendee.Note = request.Note?.Trim() ?? attendee.Note;
        attendee.UpdatedAt = nowUtc;

        await _context.SaveChangesAsync(ct);

        var isEventMember = await IsEventOrganizerAsync(evt, userId, ct);
        return attendee.ToRegistrationDto(userId, isEventMember);
    }

    public async Task<AttendeeDto> ReviewCheckInAsync(
        Guid attendeeId,
        Guid reviewerUserId,
        ReviewCheckInRequest request,
        CancellationToken ct = default)
    {
        var attendee = await _context.Attendees
            .Include(a => a.User)
            .Include(a => a.Event)
            .FirstOrDefaultAsync(a => a.Id == attendeeId, ct);

        if (attendee == null)
        {
            throw new KeyNotFoundException("Attendee not found");
        }

        var canReview = await IsEventOrganizerAsync(attendee.Event, reviewerUserId, ct);
        if (!canReview)
        {
            throw new UnauthorizedAccessException("Only event organizers can review check-in requests");
        }

        if (attendee.Status == AttendeeStatus.CheckedIn)
        {
            throw new InvalidOperationException("Attendee has already checked in");
        }

        if (attendee.Status != AttendeeStatus.CheckInPending)
        {
            throw new InvalidOperationException("No pending check-in request to review");
        }

        attendee.Status = request.Approve ? AttendeeStatus.CheckedIn : AttendeeStatus.Registered;
        attendee.CheckedInAt = request.Approve ? DateTime.UtcNow : null;
        attendee.Note = request.Note?.Trim() ?? attendee.Note;
        attendee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return attendee.ToAttendeeDto();
    }

    private async Task<Attendee> RegisterInternalAsync(Guid eventId, Guid userId, string? note, CancellationToken ct)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        await EnsureUserCanRegisterAsync(evt, userId, ct);

        var attendee = await _context.Attendees
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId, ct);

        if (attendee != null)
        {
            if (attendee.Status != AttendeeStatus.Cancelled)
            {
                return attendee;
            }

            attendee.Status = AttendeeStatus.Registered;
            attendee.RegisteredAt = DateTime.UtcNow;
            attendee.CheckedInAt = null;
            attendee.Note = note?.Trim();
            attendee.UpdatedAt = DateTime.UtcNow;
            evt.RegisteredParticipants += 1;
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
                Note = note?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Attendees.Add(attendee);
            evt.RegisteredParticipants += 1;
        }

        await _context.SaveChangesAsync(ct);
        return attendee;
    }

    private async Task<Attendee?> CancelInternalAsync(
        Guid eventId,
        Guid userId,
        string? note,
        CancellationToken ct,
        bool throwIfMissing = true)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        var attendee = await _context.Attendees
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId, ct);

        if (attendee == null || attendee.Status == AttendeeStatus.Cancelled)
        {
            if (throwIfMissing)
            {
                throw new KeyNotFoundException("Registration not found");
            }

            return attendee;
        }

        attendee.Status = AttendeeStatus.Cancelled;
        attendee.Note = note?.Trim() ?? attendee.Note;
        attendee.UpdatedAt = DateTime.UtcNow;
        evt.RegisteredParticipants = Math.Max(0, evt.RegisteredParticipants - 1);

        await _context.SaveChangesAsync(ct);
        return attendee;
    }

    private async Task EnsureUserCanRegisterAsync(Event evt, Guid userId, CancellationToken ct)
    {
        if (evt.Status is EventStatus.Cancelled or EventStatus.Completed)
        {
            throw new InvalidOperationException("This event is not accepting new attendees");
        }

        var isEventMember = await IsEventOrganizerAsync(evt, userId, ct);
        if (isEventMember)
        {
            throw new InvalidOperationException("Event organizers join as BTC and cannot register as attendee");
        }

        if (evt.Visibility == EventVisibility.Public)
        {
            EnsureCapacity(evt);
            return;
        }

        var isOrgMember = await _context.Members
            .AnyAsync(m => m.OrgId == evt.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isOrgMember)
        {
            throw new UnauthorizedAccessException("Only organization members can join this event");
        }

        EnsureCapacity(evt);
    }

    private async Task<bool> IsEventOrganizerAsync(Event evt, Guid userId, CancellationToken ct)
    {
        var isEventMember = await _context.EventMembers
            .AnyAsync(
                em => em.EventId == evt.Id &&
                      em.Member.UserId == userId &&
                      em.Member.Status == MemberStatus.Active,
                ct);

        if (isEventMember)
        {
            return true;
        }

        if (!evt.CreatedByMemberId.HasValue)
        {
            return false;
        }

        return await _context.Members.AnyAsync(
            m => m.Id == evt.CreatedByMemberId.Value &&
                 m.UserId == userId &&
                 m.Status == MemberStatus.Active,
            ct);
    }

    private static void EnsureCapacity(Event evt)
    {
        if (evt.TargetParticipants.HasValue && evt.TargetParticipants.Value > 0)
        {
            if (evt.RegisteredParticipants >= evt.TargetParticipants.Value)
            {
                throw new InvalidOperationException("This event has reached participant capacity");
            }
        }
    }
}

file static class AttendeeServiceMappings
{
    public static AttendeeRegistrationDto ToRegistrationDto(this Attendee attendee, Guid fallbackUserId, bool isEventMember = false)
    {
        return new AttendeeRegistrationDto
        {
            EventId = attendee.EventId,
            UserId = attendee.UserId ?? fallbackUserId,
            IsEventMember = isEventMember,
            IsRegistered = attendee.Status != AttendeeStatus.Cancelled,
            AttendeeId = attendee.Id,
            Status = attendee.Status.ToString(),
            RegisteredAtUtc = attendee.RegisteredAt,
            CheckedInAtUtc = attendee.CheckedInAt
        };
    }
}
