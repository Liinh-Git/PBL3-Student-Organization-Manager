using Org.Shared.Features.EventMembers;

namespace Org.Backend.Features.EventMembers.Services;

public interface IEventMemberService
{
    Task<List<EventMemberDto>> GetEventMembersAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<List<EventMemberDto>> AddEventMembersAsync(Guid eventId, Guid userId, AddEventMembersRequest request, CancellationToken ct = default);
    Task<bool> RemoveEventMemberAsync(Guid eventMemberId, Guid userId, CancellationToken ct = default);
}

