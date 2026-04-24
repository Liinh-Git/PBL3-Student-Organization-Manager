// ---- Interface service cho request — CRUD đơn đăng ký và yêu cầu tổ chức ----
// Được implement bởi: RequestMockService (mock)
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Requests;

public interface IRequestService
{
    /// <summary>Lấy danh sách request đang chờ duyệt theo tổ chức</summary>
    Task<List<RequestViewModel>> GetPendingRequestsAsync(Guid orgId, CancellationToken ct = default);

    /// <summary>Lấy chi tiết đơn đăng ký (bao gồm application detail)</summary>
    Task<RequestDetailViewModel?> GetRequestDetailAsync(Guid requestId, CancellationToken ct = default);

    /// <summary>Chấp nhận request — nếu là JOIN thì tự động tạo Member</summary>
    Task ApproveRequestAsync(Guid requestId, CancellationToken ct = default);

    /// <summary>Từ chối request</summary>
    Task RejectRequestAsync(Guid requestId, CancellationToken ct = default);

    /// <summary>Sinh viên nộp đơn xin tham gia CLB</summary>
    Task SubmitJoinRequestAsync(JoinRequestFormViewModel form, CancellationToken ct = default);
}
