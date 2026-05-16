using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
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

    public async Task<AttendeeRegistrationDto> GetMyRegistrationAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var evt = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

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
                IsRegistered = false,
                AttendeeId = attendee?.Id,
                Status = attendee?.Status.ToString(),
                RegisteredAtUtc = attendee?.RegisteredAt,
                CheckedInAtUtc = attendee?.CheckedInAt
            };
        }

        return attendee.ToRegistrationDto();
    }

    public async Task<AttendeeRegistrationDto> RegisterMeAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        await EnsureUserCanRegisterAsync(evt, userId, ct);

        var attendee = await _context.Attendees
            .Where(a => a.EventId == eventId && a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (attendee == null)
        {
            attendee = new Attendee
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Status = AttendeeStatus.Registered,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Attendees.Add(attendee);
            await _context.SaveChangesAsync(ct);
            return attendee.ToRegistrationDto();
        }

        if (attendee.Status == AttendeeStatus.Cancelled)
        {
            attendee.Status = AttendeeStatus.Registered;
            attendee.RegisteredAt = DateTime.UtcNow;
            attendee.CheckedInAt = null;
            attendee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        return attendee.ToRegistrationDto();
    }

    public async Task<AttendeeRegistrationDto> UnregisterAsync(
        Guid eventId,
        Guid userId,
        AttendeeRegistrationUpdateDto updateDto,
        CancellationToken ct = default)
    {
        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        var attendee = await _context.Attendees
            .Where(a => a.EventId == eventId && a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (attendee == null)
        {
            return new AttendeeRegistrationDto
            {
                EventId = eventId,
                UserId = userId,
                IsRegistered = false,
                Status = AttendeeStatus.Cancelled.ToString()
            };
        }

        attendee.Status = AttendeeStatus.Cancelled;
        attendee.Note = updateDto?.Note;
        attendee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return attendee.ToRegistrationDto();
    }

    private async Task EnsureUserCanRegisterAsync(Event evt, Guid userId, CancellationToken ct)
    {
        if (evt.Status is EventStatus.Cancelled or EventStatus.Archived or EventStatus.Completed)
        {
            throw new InvalidOperationException("This event is not accepting new attendees");
        }

        if (evt.Visibility == EventVisibility.Public)
        {
            return;
        }

        var isOrgMember = await _context.Members
            .AnyAsync(m => m.OrgId == evt.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isOrgMember)
        {
            throw new UnauthorizedAccessException("Only organization members can join this event");
        }
    }
}

file static class AttendeeServiceMappings
{
    public static AttendeeRegistrationDto ToRegistrationDto(this Attendee attendee)
    {
        return new AttendeeRegistrationDto
        {
            EventId = attendee.EventId,
            UserId = attendee.UserId ?? Guid.Empty,
            IsRegistered = attendee.Status != AttendeeStatus.Cancelled,
            AttendeeId = attendee.Id,
            Status = attendee.Status.ToString(),
            RegisteredAtUtc = attendee.RegisteredAt,
            CheckedInAtUtc = attendee.CheckedInAt
        };
    }
}
