using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Services;

public interface IAttendeeService
{
    Task<AttendeeRegistrationDto> GetMyRegistrationAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<AttendeeRegistrationDto> RegisterMeAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    // TODO:
    Task<AttendeeRegistrationDto> UnregisterAsync(Guid eventId, Guid userId, AttendeeRegistrationUpdateDto updateDto, CancellationToken ct = default);
}
