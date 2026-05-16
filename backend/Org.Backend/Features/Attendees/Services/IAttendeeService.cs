using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Services;

public interface IAttendeeService
{
    Task<List<AttendeeDto>> GetEventAttendeesAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<AttendeeDto?> GetMyRegistrationAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<AttendeeDto> RegisterAsync(Guid eventId, Guid userId, RegisterEventAttendeeRequest request, CancellationToken ct = default);
    Task<AttendeeDto> CancelRegistrationAsync(Guid eventId, Guid userId, CancelEventRegistrationRequest request, CancellationToken ct = default);
}
