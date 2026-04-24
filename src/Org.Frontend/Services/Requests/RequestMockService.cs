// ---- Mock service cho Request: đọc/ghi dữ liệu từ FrontendMockDataStore ----
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Requests;

public sealed class RequestMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IRequestService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public Task<List<RequestViewModel>> GetPendingRequestsAsync(Guid orgId, CancellationToken ct = default)
    {
        return _mockDataStore.UseAsync(data =>
        {
            return data.Requests
                .Where(r => r.OrgId == orgId
                         && string.Equals(r.Status, "PENDING", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToViewModel(r, data))
                .ToList();
        }, ct);
    }

    public Task<RequestDetailViewModel?> GetRequestDetailAsync(Guid requestId, CancellationToken ct = default)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var request = data.Requests.FirstOrDefault(r => r.Id == requestId);
            if (request is null) return null;

            var user = data.Users.FirstOrDefault(u => u.Id == request.UserId);
            return new RequestDetailViewModel
            {
                Id = request.Id,
                OrgId = request.OrgId,
                UserId = request.UserId,
                UserName = user?.FullName ?? "Nguoi dung",
                Email = user?.Email ?? "",
                AvatarUrl = user?.AvatarUrl,
                RequestType = request.RequestType,
                Title = request.Title,
                Message = request.Message,
                CreatedAt = request.CreatedAt,
                DesiredDepartment = request.DesiredDepartment,
                DesiredPosition = request.DesiredPosition,
                Experience = request.Experience,
                Strengths = request.Strengths,
                Reason = request.Reason
            };
        }, ct);
    }

    public Task ApproveRequestAsync(Guid requestId, CancellationToken ct = default)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var request = data.Requests.FirstOrDefault(r => r.Id == requestId)
                ?? throw new KeyNotFoundException($"Request {requestId} not found.");

            request.Status = "APPROVED";

            // Nếu là đơn xin tham gia → tự động tạo Member mới
            if (string.Equals(request.RequestType, "JOIN", StringComparison.OrdinalIgnoreCase))
            {
                // Tìm department phù hợp (nếu có)
                Guid? departmentId = null;
                if (!string.IsNullOrWhiteSpace(request.DesiredDepartment))
                {
                    var dept = data.Departments.FirstOrDefault(d =>
                        d.OrgId == request.OrgId
                        && d.DeptName.Contains(request.DesiredDepartment, StringComparison.OrdinalIgnoreCase));
                    departmentId = dept?.Id;
                }

                var user = data.Users.FirstOrDefault(u => u.Id == request.UserId);

                var newMember = new MockMember
                {
                    Id = Guid.NewGuid(),
                    OrgId = request.OrgId,
                    UserId = request.UserId,
                    DisplayName = user?.FullName ?? "Thanh vien moi",
                    DepartmentId = departmentId,
                    RoleId = null, // role mặc định = Thành viên (null = chưa gán role cụ thể)
                    JoinDate = DateTime.UtcNow
                };

                data.Members.Add(newMember);

                // Cập nhật tổng thành viên
                var org = data.Organizations.FirstOrDefault(o => o.Id == request.OrgId);
                if (org is not null)
                {
                    org.TotalMembers = data.Members.Count(m => m.OrgId == org.Id);
                }
            }

            return 0;
        }, ct);
    }

    public Task RejectRequestAsync(Guid requestId, CancellationToken ct = default)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var request = data.Requests.FirstOrDefault(r => r.Id == requestId)
                ?? throw new KeyNotFoundException($"Request {requestId} not found.");

            request.Status = "REJECTED";
            return 0;
        }, ct);
    }

    public async Task SubmitJoinRequestAsync(JoinRequestFormViewModel form, CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = Guid.Parse(
            authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("User not authenticated."));

        await _mockDataStore.UseAsync(data =>
        {
            // Kiểm tra đã có request PENDING cho cùng org chưa
            var existing = data.Requests.FirstOrDefault(r =>
                r.OrgId == form.OrgId
                && r.UserId == userId
                && string.Equals(r.Status, "PENDING", StringComparison.OrdinalIgnoreCase)
                && string.Equals(r.RequestType, "JOIN", StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                throw new InvalidOperationException("Ban da gui don dang ky cho to chuc nay roi.");
            }

            // Kiểm tra đã là thành viên chưa
            var alreadyMember = data.Members.Any(m => m.OrgId == form.OrgId && m.UserId == userId);
            if (alreadyMember)
            {
                throw new InvalidOperationException("Ban da la thanh vien cua to chuc nay.");
            }

            var org = data.Organizations.FirstOrDefault(o => o.Id == form.OrgId);

            var newRequest = new MockRequest
            {
                Id = Guid.NewGuid(),
                OrgId = form.OrgId,
                UserId = userId,
                RequestType = "JOIN",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                Title = $"Xin tham gia {org?.OrgName ?? "to chuc"}",
                DesiredDepartment = form.DesiredDepartmentName,
                DesiredPosition = form.DesiredPosition,
                Experience = form.Experience,
                Strengths = form.Strengths,
                Reason = form.Reason
            };

            data.Requests.Add(newRequest);
            return 0;
        }, ct);
    }

    private static RequestViewModel MapToViewModel(MockRequest request, MockDataSet data)
    {
        var user = data.Users.FirstOrDefault(u => u.Id == request.UserId);
        var tags = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.DesiredDepartment))
            tags.Add(request.DesiredDepartment);
        if (!string.IsNullOrWhiteSpace(request.DesiredPosition))
            tags.Add(request.DesiredPosition);

        return new RequestViewModel
        {
            Id = request.Id,
            OrgId = request.OrgId,
            UserId = request.UserId,
            UserName = user?.FullName ?? "Nguoi dung",
            Email = user?.Email ?? "",
            AvatarUrl = user?.AvatarUrl,
            Tags = tags,
            RequestType = request.RequestType,
            Title = request.Title,
            Message = request.Message,
            CreatedAt = request.CreatedAt,
            DesiredDepartment = request.DesiredDepartment,
            DesiredPosition = request.DesiredPosition,
            Experience = request.Experience
        };
    }
}
