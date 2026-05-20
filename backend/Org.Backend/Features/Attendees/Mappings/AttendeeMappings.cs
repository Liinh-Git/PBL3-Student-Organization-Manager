using Org.Backend.Domain.Entities;
using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Mappings;

public static class AttendeeMappings
{
    public static AttendeeDto ToAttendeeDto(this Attendee attendee)
    {
        return new AttendeeDto
        {
            Id = attendee.Id,
            EventId = attendee.EventId,
            UserId = attendee.UserId,
            FullName = attendee.User?.FullName ?? attendee.GuestName,
            Email = attendee.User?.Email ?? attendee.GuestEmail,
            PhoneNumber = attendee.User?.PhoneNumber ?? attendee.GuestPhone,
            Status = attendee.Status.ToString(),
            RegisteredAtUtc = attendee.RegisteredAt,
            CheckedInAtUtc = attendee.CheckedInAt,
            Note = attendee.Note
        };
    }
}
