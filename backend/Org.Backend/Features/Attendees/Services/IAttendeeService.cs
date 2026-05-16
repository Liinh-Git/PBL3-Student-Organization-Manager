using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Services;

public interface IAttendeeService
{
    Task<List<AttendeeDto>> GetEventAttendeesAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<AttendeeRegistrationDto> GetMyRegistrationAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<AttendeeDto> RegisterAsync(Guid eventId, Guid userId, RegisterEventAttendeeRequest request, CancellationToken ct = default);
    Task<AttendeeRegistrationDto> RegisterMeAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<AttendeeDto> CancelRegistrationAsync(Guid eventId, Guid userId, CancelEventRegistrationRequest request, CancellationToken ct = default);
    Task<AttendeeRegistrationDto> UnregisterAsync(Guid eventId, Guid userId, AttendeeRegistrationUpdateDto updateDto, CancellationToken ct = default);
}
