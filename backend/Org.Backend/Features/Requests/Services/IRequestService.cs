using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Services;

public interface IRequestService
{
    Task<List<RequestDto>> GetOrganizationRequestsAsync(Guid userId, Guid orgId, CancellationToken ct = default);
    Task<RequestDto> CreateRequestAsync(Guid userId, Guid orgId, CreateRequestRequest request, CancellationToken ct = default);
    Task<RequestDto> GetRequestByIdAsync(Guid userId, Guid requestId, CancellationToken ct = default);
    Task<RequestDto> ReviewRequestAsync(Guid userId, Guid requestId, ReviewRequestRequest request, CancellationToken ct = default);
    Task<List<MyPendingJoinRequestDto>> GetMyPendingJoinRequestsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> WithdrawMyPendingJoinRequestAsync(Guid userId, Guid orgId, CancellationToken ct = default);
}
