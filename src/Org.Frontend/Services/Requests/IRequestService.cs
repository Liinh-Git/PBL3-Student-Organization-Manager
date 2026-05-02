using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Requests;

public interface IRequestService
{
    Task<bool> CanViewOrganizationRequestsAsync(Guid orgId, CancellationToken ct = default);
    Task<bool> CanReviewOrganizationRequestsAsync(Guid orgId, CancellationToken ct = default);

    Task<List<RequestViewModel>> GetPendingRequestsAsync(Guid orgId, CancellationToken ct = default);
    Task<RequestDetailViewModel?> GetRequestDetailAsync(Guid requestId, CancellationToken ct = default);
    Task ApproveRequestAsync(Guid requestId, CancellationToken ct = default);
    Task RejectRequestAsync(Guid requestId, CancellationToken ct = default);

    Task SubmitJoinRequestAsync(JoinRequestFormViewModel form, CancellationToken ct = default);
    Task SubmitOrganizationRequestAsync(CreateOrganizationRequestViewModel form, CancellationToken ct = default);
}
