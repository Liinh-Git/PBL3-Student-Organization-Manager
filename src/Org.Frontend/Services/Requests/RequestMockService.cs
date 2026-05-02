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
    private static readonly string[] AvatarPool =
    [
        "/images/mockimages/AvtUser/Avt1.jpg",
        "/images/mockimages/AvtUser/Avt2.jpg",
        "/images/mockimages/AvtUser/Avt3.jpg",
        "/images/mockimages/AvtUser/Avt4.jpg",
        "/images/mockimages/AvtUser/Avt5.jpg",
        "/images/mockimages/AvtUser/Avt6.jpg",
        "/images/mockimages/AvtUser/Avt7.jpg",
        "/images/mockimages/AvtUser/Avt8.jpg",
        "/images/mockimages/AvtUser/Avt9.jpg",
        "/images/mockimages/AvtUser/Avt10.jpg",
        "/images/mockimages/AvtUser/Avt11.jpg",
        "/images/mockimages/AvtUser/Avt12.jpg",
        "/images/mockimages/AvtUser/Avt13.jpg",
        "/images/mockimages/AvtUser/Avt14.jpg",
        "/images/mockimages/AvtUser/Avt15.jpg",
        "/images/mockimages/AvtUser/Avt16.jpg",
        "/images/mockimages/AvtUser/Avt17.jpg"
    ];

    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<bool> CanViewOrganizationRequestsAsync(Guid orgId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
            HasAnyPermission(data, currentUserId, orgId, "org.requests.view", "org.requests.review", "org.requests.approve", "org.members.manage"), ct);
    }

    public async Task<bool> CanReviewOrganizationRequestsAsync(Guid orgId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
            HasAnyPermission(data, currentUserId, orgId, "org.requests.review", "org.requests.approve", "org.members.manage"), ct);
    }

    public async Task<List<RequestViewModel>> GetPendingRequestsAsync(Guid orgId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureCanViewRequests(data, currentUserId, orgId);
            return data.Requests
                .Where(r => r.OrgId == orgId && IsPending(r.Status))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToViewModel(r, data))
                .ToList();
        }, ct);
    }

    public async Task<RequestDetailViewModel?> GetRequestDetailAsync(Guid requestId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            var request = data.Requests.FirstOrDefault(r => r.Id == requestId);
            if (request is null)
            {
                return null;
            }

            EnsureCanViewRequests(data, currentUserId, request.OrgId);
            var user = data.Users.FirstOrDefault(u => u.Id == request.UserId);
            return new RequestDetailViewModel
            {
                Id = request.Id,
                OrgId = request.OrgId,
                UserId = request.UserId,
                UserName = user?.FullName ?? "Nguoi dung",
                Email = user?.Email ?? string.Empty,
                AvatarUrl = ResolveAvatar(user),
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

    public async Task ApproveRequestAsync(Guid requestId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var request = data.Requests.FirstOrDefault(r => r.Id == requestId)
                ?? throw new KeyNotFoundException($"Request {requestId} not found.");

            EnsureCanReviewRequests(data, currentUserId, request.OrgId);
            request.Status = "Approved";

            if (IsJoinRequest(request.RequestType))
            {
                var alreadyMember = data.Members.Any(m => m.OrgId == request.OrgId && m.UserId == request.UserId);
                if (!alreadyMember)
                {
                    Guid? departmentId = null;
                    if (!string.IsNullOrWhiteSpace(request.DesiredDepartment))
                    {
                        var dept = data.Departments.FirstOrDefault(d =>
                            d.OrgId == request.OrgId
                            && d.DeptName.Contains(request.DesiredDepartment, StringComparison.OrdinalIgnoreCase));
                        departmentId = dept?.Id;
                    }

                    var user = data.Users.FirstOrDefault(u => u.Id == request.UserId);
                    var memberRole = data.OrganizationRoles.FirstOrDefault(x =>
                        x.OrgId == request.OrgId && string.Equals(x.RoleName, "Member", StringComparison.OrdinalIgnoreCase));

                    data.Members.Add(new MockMember
                    {
                        Id = Guid.NewGuid(),
                        OrgId = request.OrgId,
                        UserId = request.UserId,
                        DisplayName = user?.FullName ?? "Thanh vien moi",
                        DepartmentId = departmentId,
                        RoleId = memberRole?.Id,
                        JoinDate = DateTime.UtcNow
                    });

                    var org = data.Organizations.FirstOrDefault(o => o.Id == request.OrgId);
                    if (org is not null)
                    {
                        org.TotalMembers = data.Members.Count(m => m.OrgId == org.Id);
                        org.LastActivityAtUtc = DateTime.UtcNow;
                    }
                }
            }

            var actorId = ResolveActorId(data, currentUserId, request.OrgId);
            data.Notifications.Add(new MockNotification
            {
                Id = Guid.NewGuid(),
                ReceiverId = request.UserId,
                ActorId = actorId,
                Title = "Yeu cau da duoc chap nhan",
                Message = request.Title ?? "Yeu cau cua ban da duoc chap nhan.",
                Type = "JoinRequestApproved",
                ActionUrl = $"/org-overview?orgId={request.OrgId:D}",
                CreatedAt = DateTime.UtcNow
            });
            return 0;
        }, ct);
    }

    public async Task RejectRequestAsync(Guid requestId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var request = data.Requests.FirstOrDefault(r => r.Id == requestId)
                ?? throw new KeyNotFoundException($"Request {requestId} not found.");

            EnsureCanReviewRequests(data, currentUserId, request.OrgId);
            request.Status = "Rejected";

            var actorId = ResolveActorId(data, currentUserId, request.OrgId);
            data.Notifications.Add(new MockNotification
            {
                Id = Guid.NewGuid(),
                ReceiverId = request.UserId,
                ActorId = actorId,
                Title = "Yeu cau da bi tu choi",
                Message = request.Title ?? "Yeu cau cua ban da bi tu choi.",
                Type = "JoinRequestRejected",
                ActionUrl = "/user/organizations",
                CreatedAt = DateTime.UtcNow
            });
            return 0;
        }, ct);
    }

    public async Task SubmitJoinRequestAsync(JoinRequestFormViewModel form, CancellationToken ct = default)
    {
        var userId = await RequireCurrentUserIdAsync();

        await _mockDataStore.UseAsync(data =>
        {
            var existing = data.Requests.FirstOrDefault(r =>
                r.OrgId == form.OrgId
                && r.UserId == userId
                && IsPending(r.Status)
                && IsJoinRequest(r.RequestType));
            if (existing is not null)
            {
                throw new InvalidOperationException("Ban da gui don dang ky cho to chuc nay roi.");
            }

            var alreadyMember = data.Members.Any(m => m.OrgId == form.OrgId && m.UserId == userId);
            if (alreadyMember)
            {
                throw new InvalidOperationException("Ban da la thanh vien cua to chuc nay.");
            }

            var org = data.Organizations.FirstOrDefault(o => o.Id == form.OrgId);
            var request = new MockRequest
            {
                Id = Guid.NewGuid(),
                OrgId = form.OrgId,
                UserId = userId,
                RequestType = "JoinClub",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Title = $"Xin tham gia {org?.OrgName ?? "to chuc"}",
                DesiredDepartment = form.DesiredDepartmentName,
                DesiredPosition = form.DesiredPosition,
                Experience = form.Experience,
                Strengths = form.Strengths,
                Reason = form.Reason
            };
            data.Requests.Add(request);

            var reviewerIds = ResolveReviewerUserIds(data, form.OrgId);
            foreach (var reviewerId in reviewerIds)
            {
                data.Notifications.Add(new MockNotification
                {
                    Id = Guid.NewGuid(),
                    ReceiverId = reviewerId,
                    ActorId = userId,
                    Title = "Co yeu cau tham gia moi",
                    Message = request.Title ?? "Co yeu cau tham gia moi.",
                    Type = "JoinRequestReceived",
                    ActionUrl = "/org/requests",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return 0;
        }, ct);
    }

    public async Task SubmitOrganizationRequestAsync(CreateOrganizationRequestViewModel form, CancellationToken ct = default)
    {
        var userId = await RequireCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            EnsureOrganizationMember(data, userId, form.OrgId);
            EnsureValidOrganizationRequest(form);

            var request = new MockRequest
            {
                Id = Guid.NewGuid(),
                OrgId = form.OrgId,
                UserId = userId,
                RequestType = form.RequestType.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Title = form.Title.Trim(),
                Message = form.Message.Trim()
            };
            data.Requests.Add(request);

            foreach (var reviewerId in ResolveReviewerUserIds(data, form.OrgId))
            {
                data.Notifications.Add(new MockNotification
                {
                    Id = Guid.NewGuid(),
                    ReceiverId = reviewerId,
                    ActorId = userId,
                    Title = "Co yeu cau noi bo moi",
                    Message = request.Title ?? "Co yeu cau moi can duyet.",
                    Type = "SystemAnnouncement",
                    ActionUrl = "/org/requests",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return 0;
        }, ct);
    }

    private static void EnsureValidOrganizationRequest(CreateOrganizationRequestViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Title) || form.Title.Trim().Length < 3)
        {
            throw new InvalidOperationException("Request title must be at least 3 characters.");
        }

        if (string.IsNullOrWhiteSpace(form.Message) || form.Message.Trim().Length < 5)
        {
            throw new InvalidOperationException("Request message must be at least 5 characters.");
        }

        if (string.IsNullOrWhiteSpace(form.RequestType))
        {
            throw new InvalidOperationException("Request type is required.");
        }
    }

    private static Guid? ResolveActorId(MockDataSet data, Guid? currentUserId, Guid organizationId)
    {
        if (currentUserId.HasValue)
        {
            return currentUserId;
        }

        return data.Members
            .Where(x => x.OrgId == organizationId && x.RoleId.HasValue)
            .Select(x => new
            {
                x.UserId,
                Role = data.OrganizationRoles.FirstOrDefault(r => r.Id == x.RoleId!.Value)
            })
            .OrderBy(x => ResolveRoleRank(x.Role?.RoleName))
            .Select(x => (Guid?)x.UserId)
            .FirstOrDefault();
    }

    private static IReadOnlyList<Guid> ResolveReviewerUserIds(MockDataSet data, Guid orgId)
    {
        return data.Members
            .Where(m => m.OrgId == orgId && m.RoleId.HasValue)
            .Where(m =>
            {
                var role = data.OrganizationRoles.FirstOrDefault(r => r.Id == m.RoleId!.Value && r.OrgId == orgId);
                if (role is null)
                {
                    return false;
                }

                var permissions = role.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
                return permissions.Contains("org.requests.view")
                    || permissions.Contains("org.requests.review")
                    || permissions.Contains("org.requests.approve")
                    || permissions.Contains("org.members.manage");
            })
            .Select(m => m.UserId)
            .Distinct()
            .ToList();
    }

    private static bool IsJoinRequest(string? requestType)
        => string.Equals(requestType, "JoinClub", StringComparison.OrdinalIgnoreCase)
           || string.Equals(requestType, "JOIN", StringComparison.OrdinalIgnoreCase);

    private static bool IsPending(string? status)
        => string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase);

    private static RequestViewModel MapToViewModel(MockRequest request, MockDataSet data)
    {
        var user = data.Users.FirstOrDefault(u => u.Id == request.UserId);
        var tags = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.DesiredDepartment))
        {
            tags.Add(request.DesiredDepartment);
        }

        if (!string.IsNullOrWhiteSpace(request.DesiredPosition))
        {
            tags.Add(request.DesiredPosition);
        }

        return new RequestViewModel
        {
            Id = request.Id,
            OrgId = request.OrgId,
            UserId = request.UserId,
            UserName = user?.FullName ?? "Nguoi dung",
            Email = user?.Email ?? string.Empty,
            AvatarUrl = ResolveAvatar(user),
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

    private static string? ResolveAvatar(MockUser? user)
    {
        if (user is null)
        {
            return AvatarPool[0];
        }

        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            return user.AvatarUrl;
        }

        var index = (user.Id.GetHashCode() & int.MaxValue) % AvatarPool.Length;
        return AvatarPool[index];
    }

    private static void EnsureCanViewRequests(MockDataSet data, Guid? userId, Guid orgId)
    {
        if (HasAnyPermission(data, userId, orgId, "org.requests.view", "org.requests.review", "org.requests.approve", "org.members.manage"))
        {
            return;
        }

        throw new UnauthorizedAccessException("You do not have permission to view organization requests.");
    }

    private static void EnsureCanReviewRequests(MockDataSet data, Guid? userId, Guid orgId)
    {
        if (HasAnyPermission(data, userId, orgId, "org.requests.review", "org.requests.approve", "org.members.manage"))
        {
            return;
        }

        throw new UnauthorizedAccessException("You do not have permission to review organization requests.");
    }

    private static bool HasAnyPermission(MockDataSet data, Guid? userId, Guid orgId, params string[] expectedPermissions)
    {
        var member = ResolveMemberByUserId(data, orgId, userId);
        if (member?.RoleId is null)
        {
            return false;
        }

        var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == member.RoleId.Value && x.OrgId == orgId);
        if (role is null)
        {
            return false;
        }

        var permissions = role.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return expectedPermissions.Any(permissions.Contains);
    }

    private static MockMember? ResolveMemberByUserId(MockDataSet data, Guid orgId, Guid? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        return data.Members.FirstOrDefault(x => x.OrgId == orgId && x.UserId == userId.Value);
    }

    private static void EnsureOrganizationMember(MockDataSet data, Guid userId, Guid orgId)
    {
        if (!data.Members.Any(x => x.OrgId == orgId && x.UserId == userId))
        {
            throw new UnauthorizedAccessException("Current user is not a member of the organization.");
        }
    }

    private static int ResolveRoleRank(string? roleName)
    {
        return roleName?.Trim().ToUpperInvariant() switch
        {
            "PRESIDENT" => 0,
            "VICEPRESIDENT" => 1,
            "MANAGER" => 2,
            _ => 9
        };
    }

    private async Task<Guid?> TryGetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }

    private async Task<Guid> RequireCurrentUserIdAsync()
    {
        var userId = await TryGetCurrentUserIdAsync();
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        return userId.Value;
    }
}
