// ---- Các endpoint quản lý hồ sơ người dùng ----
// GET/PUT /api/users/me — hồ sơ bản thân
// GET /api/users/{id} — xem hồ sơ người khác (chỉ kết thành viên chung tổ chức mới xem được)
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Backend.Services;
using Org.Shared.Features.Users;
using System.Security.Claims;
using System.Text.Json;

namespace Org.Backend.Features.Users;

// ---- GET /api/users/me — lấy hồ sơ của user đang đăng nhập ----
public sealed class GetCurrentUserProfileEndpoint(AppDbContext db) : EndpointWithoutRequest<GetCurrentUserProfileResponse>
{
    public override void Configure()
    {
        Get("/api/users/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId!.Value, ct);

        if (user is null)
            ThrowError("User not found.", StatusCodes.Status404NotFound);

        await Send.OkAsync(new GetCurrentUserProfileResponse(UserContractMapping.ToUserProfileDto(user)), ct);
    }
}

// ---- GET /api/users/me/organizations — danh sách tổ chức user đang tham gia ----
public sealed class GetMyOrganizationsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetMyOrganizationsResponse>
{
    public override void Configure()
    {
        Get("/api/users/me/organizations");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var items = await db.Members
            .AsNoTracking()
            .Where(x => x.UserId == userId!.Value)
            .OrderByDescending(x => x.JoinDate)
            .Select(x => new MyOrganizationDto(
                x.OrgId,
                x.Organization.OrgName,
                x.Organization.Description,
                x.Organization.AvatarUrl,
                new DateTimeOffset(DateTime.SpecifyKind(x.JoinDate, DateTimeKind.Utc)),
                string.IsNullOrWhiteSpace(x.Role == null ? null : x.Role.RoleName)
                    ? "Member"
                    : x.Role!.RoleName))
            .ToListAsync(ct);

        await Send.OkAsync(new GetMyOrganizationsResponse(items), ct);
    }
}

// ---- GET /api/users/me/discover/organizations — danh sách tổ chức đề xuất ----
public sealed class GetSuggestedOrganizationsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetSuggestedOrganizationsResponse>
{
    private const string DefaultOrganizationImageUrl = "https://images.unsplash.com/photo-1529156069898-49953e39b3ac?auto=format&fit=crop&w=960&q=80";

    public override void Configure()
    {
        Get("/api/users/me/discover/organizations");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var joinedOrgIds = await db.Members
            .AsNoTracking()
            .Where(x => x.UserId == userId!.Value)
            .Select(x => x.OrgId)
            .Distinct()
            .ToListAsync(ct);

        var rows = await db.Organizations
            .AsNoTracking()
            .Where(x => !joinedOrgIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.OrgName,
                x.Description,
                x.AvatarUrl,
                x.CoverUrl,
                x.TotalMembers,
                x.Location,
                x.Status,
                MemberCount = x.Members.Count
            })
            .OrderBy(x => x.Status == OrgStatus.Active ? 0 : 1)
            .ThenByDescending(x => x.TotalMembers > 0 ? x.TotalMembers : x.MemberCount)
            .ThenBy(x => x.OrgName)
            .Take(12)
            .ToListAsync(ct);

        var items = rows
            .Select(x => new SuggestedOrganizationDto(
                x.Id,
                x.OrgName,
                x.Description,
                ResolveOrganizationImageUrl(x.AvatarUrl, x.CoverUrl),
                x.TotalMembers > 0 ? x.TotalMembers : x.MemberCount,
                x.Location,
                x.Status == OrgStatus.Active))
            .ToList();

        await Send.OkAsync(new GetSuggestedOrganizationsResponse(items), ct);
    }

    private static string ResolveOrganizationImageUrl(string? avatarUrl, string? coverUrl)
    {
        if (!string.IsNullOrWhiteSpace(avatarUrl))
            return avatarUrl.Trim();

        if (!string.IsNullOrWhiteSpace(coverUrl))
            return coverUrl.Trim();

        return DefaultOrganizationImageUrl;
    }
}

// ---- GET /api/users/{id} — xem hồ sơ người khác ----
// Bảo mật: 
// - Nếu profile là Public → ai cũng xem được
// - Nếu profile là OrganizationOnly → chỉ xem được nếu cùng tổ chức và có quyền CanRead
// - Nếu profile là Private → chỉ chủ nhân xem được
public sealed class GetUserByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetUserByIdResponse>
{
    public override void Configure()
    {
        Get("/api/users/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (user is null)
            ThrowError("User not found.", StatusCodes.Status404NotFound);

        var callerUserId = UserValidation.ParseUserId(User);
        if (callerUserId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        // Nếu là chính mình → cho phép xem
        if (callerUserId.Value == id)
        {
            await Send.OkAsync(new GetUserByIdResponse(UserContractMapping.ToUserProfileDto(user)), ct);
            return;
        }

        // Kiểm tra profile visibility
        if (user.ProfileVisibility == ProfileVisibility.Public)
        {
            // Public profile → ai cũng xem được
            await Send.OkAsync(new GetUserByIdResponse(UserContractMapping.ToUserProfileDto(user)), ct);
            return;
        }

        if (user.ProfileVisibility == ProfileVisibility.Private)
        {
            // Private profile → chỉ chủ nhân xem được
            ThrowError("This profile is private.", StatusCodes.Status403Forbidden);
            return;
        }

        // OrganizationOnly → kiểm tra cùng tổ chức
        var targetOrgIds = await db.Members
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .Select(x => x.OrgId)
            .Distinct()
            .ToListAsync(ct);

        var canReadTarget = false;
        foreach (var orgId in targetOrgIds)
        {
            var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
            if (callerContext is not null && OrganizationAuthorization.CanRead(callerContext.Value.Role))
            {
                canReadTarget = true;
                break;
            }
        }

        if (!canReadTarget)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await Send.OkAsync(new GetUserByIdResponse(UserContractMapping.ToUserProfileDto(user)), ct);
    }
}

// ---- PUT /api/users/me — cập nhật hồ sơ cá nhân (không đổi được email) ----
public sealed class UpdateCurrentUserProfileEndpoint(AppDbContext db) : Endpoint<UpdateCurrentUserProfileRequest, UserProfileDto>
{
    public override void Configure()
    {
        Put("/api/users/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateCurrentUserProfileRequest req, CancellationToken ct)
    {
        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var normalizedName = UserValidation.NormalizeName(req.FullName);
        if (normalizedName is null)
            ThrowError("FullName must be at least 2 characters.", StatusCodes.Status400BadRequest);

        if (req.DateOfBirth is not null && req.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow.Date))
            ThrowError("DateOfBirth cannot be in the future.", StatusCodes.Status400BadRequest);

        var normalizedSocialLinks = UserValidation.NormalizeOptional(req.SocialLinksJson);
        if (normalizedSocialLinks is not null && !UserValidation.IsValidJsonObject(normalizedSocialLinks))
            ThrowError("SocialLinksJson must be a valid JSON object.", StatusCodes.Status400BadRequest);

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId!.Value, ct);
        if (user is null)
            ThrowError("User not found.", StatusCodes.Status404NotFound);

        user.FullName = normalizedName;
        user.PhoneNumber = UserValidation.NormalizeOptional(req.PhoneNumber);
        user.Dob = req.DateOfBirth?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        user.Gender = UserValidation.NormalizeOptional(req.Gender);
        user.Address = UserValidation.NormalizeOptional(req.Address);
        user.AvatarUrl = UserValidation.NormalizeOptional(req.AvatarUrl);
        user.Bio = UserValidation.NormalizeOptional(req.Bio);
        user.SocialLinks = normalizedSocialLinks;

        // Update profile visibility if provided
        if (!string.IsNullOrWhiteSpace(req.ProfileVisibility))
        {
            if (Enum.TryParse<ProfileVisibility>(req.ProfileVisibility, ignoreCase: true, out var visibility))
                user.ProfileVisibility = visibility;
            else
                ThrowError("Invalid ProfileVisibility value. Must be Public, OrganizationOnly, or Private.", StatusCodes.Status400BadRequest);
        }

        await db.SaveChangesAsync(ct);

        await Send.OkAsync(UserContractMapping.ToUserProfileDto(user), ct);
    }
}

// ---- Helper validate và parse thông tin user ----
internal static class UserValidation
{
    public static Guid? ParseUserId(ClaimsPrincipal user)
    {
        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId)
            ? userId
            : null;
    }

    public static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        return normalized.Length >= 2 ? normalized : null;
    }

    public static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static bool IsValidJsonObject(string value)
    {
        try
        {
            using var doc = JsonDocument.Parse(value);
            return doc.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch
        {
            return false;
        }
    }
}

// ---- Mapping entity User sang UserProfileDto ----
internal static class UserContractMapping
{
    public static UserProfileDto ToUserProfileDto(User user)
        => new(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Dob is null ? null : DateOnly.FromDateTime(user.Dob.Value),
            user.Gender,
            user.Address,
            user.AvatarUrl,
            user.Bio,
            user.SocialLinks,
            user.Status.ToString(),
            user.ProfileVisibility.ToString(),
            ToUtcOffset(user.CreatedAt),
            user.UpdatedAt is null ? null : ToUtcOffset(user.UpdatedAt.Value),
            user.LastLogin is null ? null : ToUtcOffset(user.LastLogin.Value));

    private static DateTimeOffset ToUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}

// ---- PUT /api/users/me/change-password — đổi mật khẩu ----
public sealed class ChangePasswordEndpoint(AppDbContext db) : Endpoint<ChangePasswordRequest>
{
    public override void Configure()
    {
        Put("/api/users/me/change-password");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        if (string.IsNullOrWhiteSpace(req.CurrentPassword))
            ThrowError("Current password is required.", StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(req.NewPassword))
            ThrowError("New password is required.", StatusCodes.Status400BadRequest);

        if (req.NewPassword.Length < 6)
            ThrowError("New password must be at least 6 characters.", StatusCodes.Status400BadRequest);

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId!.Value, ct);
        if (user is null)
            ThrowError("User not found.", StatusCodes.Status404NotFound);

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            ThrowError("Current password is incorrect.", StatusCodes.Status400BadRequest);

        // Hash new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

// ---- POST /api/users/batch — lấy thông tin nhiều user cùng lúc ----
public sealed class GetUserProfilesBatchEndpoint(AppDbContext db) : Endpoint<List<Guid>, GetUserProfilesBatchResponse>
{
    public override void Configure()
    {
        Post("/api/users/batch");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(List<Guid> userIds, CancellationToken ct)
    {
        if (userIds.Count == 0)
        {
            await Send.OkAsync(new GetUserProfilesBatchResponse([]), ct);
            return;
        }

        if (userIds.Count > 100)
            ThrowError("Cannot request more than 100 users at once.", StatusCodes.Status400BadRequest);

        var callerUserId = UserValidation.ParseUserId(User);
        if (callerUserId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var users = await db.Users
            .AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .ToListAsync(ct);

        var result = new List<UserProfileDto>();

        foreach (var user in users)
        {
            // Nếu là chính mình → cho phép xem
            if (user.Id == callerUserId.Value)
            {
                result.Add(UserContractMapping.ToUserProfileDto(user));
                continue;
            }

            // Kiểm tra profile visibility
            if (user.ProfileVisibility == ProfileVisibility.Public)
            {
                result.Add(UserContractMapping.ToUserProfileDto(user));
                continue;
            }

            if (user.ProfileVisibility == ProfileVisibility.Private)
            {
                // Private profile → skip
                continue;
            }

            // OrganizationOnly → kiểm tra cùng tổ chức
            var targetOrgIds = await db.Members
                .AsNoTracking()
                .Where(x => x.UserId == user.Id)
                .Select(x => x.OrgId)
                .Distinct()
                .ToListAsync(ct);

            var canReadTarget = false;
            foreach (var orgId in targetOrgIds)
            {
                var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
                if (callerContext is not null && OrganizationAuthorization.CanRead(callerContext.Value.Role))
                {
                    canReadTarget = true;
                    break;
                }
            }

            if (canReadTarget)
                result.Add(UserContractMapping.ToUserProfileDto(user));
        }

        await Send.OkAsync(new GetUserProfilesBatchResponse(result), ct);
    }
}

// ---- POST /api/users/{id}/friend-request — gửi lời mời kết bạn ----
public sealed class SendFriendRequestEndpoint(AppDbContext db, INotificationService notificationService) : EndpointWithoutRequest<FriendRequestDto>
{
    public override void Configure()
    {
        Post("/api/users/{id:guid}/friend-request");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var receiverId = Route<Guid>("id");

        var senderId = UserValidation.ParseUserId(User);
        if (senderId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        if (senderId.Value == receiverId)
            ThrowError("Cannot send friend request to yourself.", StatusCodes.Status400BadRequest);

        var receiver = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == receiverId, ct);
        if (receiver is null)
            ThrowError("User not found.", StatusCodes.Status404NotFound);

        // Kiểm tra đã có lời mời pending chưa
        var existingRequest = await db.FriendRequests
            .FirstOrDefaultAsync(x =>
                ((x.SenderId == senderId.Value && x.ReceiverId == receiverId) ||
                 (x.SenderId == receiverId && x.ReceiverId == senderId.Value)) &&
                x.Status == FriendRequestStatus.Pending, ct);

        if (existingRequest is not null)
            ThrowError("Friend request already exists.", StatusCodes.Status409Conflict);

        // Kiểm tra đã là bạn bè chưa
        var alreadyFriends = await db.FriendRequests
            .AnyAsync(x =>
                ((x.SenderId == senderId.Value && x.ReceiverId == receiverId) ||
                 (x.SenderId == receiverId && x.ReceiverId == senderId.Value)) &&
                x.Status == FriendRequestStatus.Accepted, ct);

        if (alreadyFriends)
            ThrowError("Already friends.", StatusCodes.Status409Conflict);

        var friendRequest = new FriendRequest
        {
            SenderId = senderId.Value,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending
        };

        db.FriendRequests.Add(friendRequest);
        await db.SaveChangesAsync(ct);

        // Send notification to receiver
        try
        {
            await notificationService.NotifyFriendRequestReceived(receiverId, senderId.Value, friendRequest.Id);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - notification failure should not block business logic
            Console.WriteLine($"Failed to send friend request notification: {ex.Message}");
        }

        var sender = await db.Users.AsNoTracking().FirstAsync(x => x.Id == senderId.Value, ct);

        var dto = new FriendRequestDto(
            friendRequest.Id,
            senderId.Value,
            sender.FullName,
            sender.AvatarUrl,
            receiverId,
            receiver.FullName,
            receiver.AvatarUrl,
            friendRequest.Status.ToString(),
            new DateTimeOffset(DateTime.SpecifyKind(friendRequest.CreatedAt, DateTimeKind.Utc)),
            null);

        await HttpContext.Response.SendAsync(dto, StatusCodes.Status201Created, cancellation: ct);
    }
}

// ---- GET /api/users/me/friend-requests — danh sách lời mời kết bạn đến ----
public sealed class GetFriendRequestsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetFriendRequestsResponse>
{
    public override void Configure()
    {
        Get("/api/users/me/friend-requests");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var requests = await db.FriendRequests
            .AsNoTracking()
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .Where(x => x.ReceiverId == userId.Value && x.Status == FriendRequestStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var items = requests.Select(x => new FriendRequestDto(
            x.Id,
            x.SenderId,
            x.Sender.FullName,
            x.Sender.AvatarUrl,
            x.ReceiverId,
            x.Receiver.FullName,
            x.Receiver.AvatarUrl,
            x.Status.ToString(),
            new DateTimeOffset(DateTime.SpecifyKind(x.CreatedAt, DateTimeKind.Utc)),
            x.RespondedAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(x.RespondedAt.Value, DateTimeKind.Utc))))
            .ToList();

        await Send.OkAsync(new GetFriendRequestsResponse(items), ct);
    }
}

// ---- PUT /api/users/me/friend-requests/{id}/accept — chấp nhận lời mời kết bạn ----
public sealed class AcceptFriendRequestEndpoint(AppDbContext db, INotificationService notificationService) : EndpointWithoutRequest<FriendRequestDto>
{
    public override void Configure()
    {
        Put("/api/users/me/friend-requests/{id:guid}/accept");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var requestId = Route<Guid>("id");

        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var friendRequest = await db.FriendRequests
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .FirstOrDefaultAsync(x => x.Id == requestId, ct);

        if (friendRequest is null)
            ThrowError("Friend request not found.", StatusCodes.Status404NotFound);

        if (friendRequest.ReceiverId != userId.Value)
            ThrowError("You can only accept friend requests sent to you.", StatusCodes.Status403Forbidden);

        if (friendRequest.Status != FriendRequestStatus.Pending)
            ThrowError("Friend request is not pending.", StatusCodes.Status400BadRequest);

        friendRequest.Status = FriendRequestStatus.Accepted;
        friendRequest.RespondedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        // Notify sender that their friend request was accepted
        try
        {
            await notificationService.NotifyFriendRequestAccepted(friendRequest.SenderId, userId.Value);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - notification failure should not block business logic
            Console.WriteLine($"Failed to send friend request accepted notification: {ex.Message}");
        }

        var dto = new FriendRequestDto(
            friendRequest.Id,
            friendRequest.SenderId,
            friendRequest.Sender.FullName,
            friendRequest.Sender.AvatarUrl,
            friendRequest.ReceiverId,
            friendRequest.Receiver.FullName,
            friendRequest.Receiver.AvatarUrl,
            friendRequest.Status.ToString(),
            new DateTimeOffset(DateTime.SpecifyKind(friendRequest.CreatedAt, DateTimeKind.Utc)),
            new DateTimeOffset(DateTime.SpecifyKind(friendRequest.RespondedAt.Value, DateTimeKind.Utc)));

        await Send.OkAsync(dto, ct);
    }
}

// ---- DELETE /api/users/me/friend-requests/{id} — từ chối/hủy lời mời kết bạn ----
public sealed class RejectFriendRequestEndpoint(AppDbContext db, INotificationService notificationService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/users/me/friend-requests/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var requestId = Route<Guid>("id");

        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var friendRequest = await db.FriendRequests
            .FirstOrDefaultAsync(x => x.Id == requestId, ct);

        if (friendRequest is null)
            ThrowError("Friend request not found.", StatusCodes.Status404NotFound);

        // Người gửi có thể hủy, người nhận có thể từ chối
        if (friendRequest.SenderId != userId.Value && friendRequest.ReceiverId != userId.Value)
            ThrowError("You can only reject/cancel your own friend requests.", StatusCodes.Status403Forbidden);

        if (friendRequest.Status != FriendRequestStatus.Pending)
            ThrowError("Friend request is not pending.", StatusCodes.Status400BadRequest);

        // Nếu là người gửi → Cancelled, nếu là người nhận → Rejected
        var isRejection = friendRequest.ReceiverId == userId.Value;
        friendRequest.Status = friendRequest.SenderId == userId.Value
            ? FriendRequestStatus.Cancelled
            : FriendRequestStatus.Rejected;
        friendRequest.RespondedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        // Notify sender if request was rejected (not cancelled)
        if (isRejection)
        {
            try
            {
                await notificationService.NotifyFriendRequestRejected(friendRequest.SenderId, userId.Value);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - notification failure should not block business logic
                Console.WriteLine($"Failed to send friend request rejected notification: {ex.Message}");
            }
        }

        await Send.NoContentAsync(ct);
    }
}

// ---- GET /api/users/me/friends — danh sách bạn bè ----
public sealed class GetFriendsEndpoint(AppDbContext db) : EndpointWithoutRequest<GetFriendsResponse>
{
    public override void Configure()
    {
        Get("/api/users/me/friends");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var friendRequests = await db.FriendRequests
            .AsNoTracking()
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .Where(x =>
                (x.SenderId == userId.Value || x.ReceiverId == userId.Value) &&
                x.Status == FriendRequestStatus.Accepted)
            .OrderByDescending(x => x.RespondedAt)
            .ToListAsync(ct);

        var items = friendRequests.Select(x =>
        {
            var friend = x.SenderId == userId.Value ? x.Receiver : x.Sender;
            return new FriendDto(
                friend.Id,
                friend.FullName,
                friend.AvatarUrl,
                friend.Bio,
                new DateTimeOffset(DateTime.SpecifyKind(x.RespondedAt!.Value, DateTimeKind.Utc)));
        }).ToList();

        await Send.OkAsync(new GetFriendsResponse(items), ct);
    }
}

// ---- DELETE /api/users/me/friends/{id} — hủy kết bạn ----
public sealed class UnfriendEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/users/me/friends/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var friendId = Route<Guid>("id");

        var userId = UserValidation.ParseUserId(User);
        if (userId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var friendRequest = await db.FriendRequests
            .FirstOrDefaultAsync(x =>
                ((x.SenderId == userId.Value && x.ReceiverId == friendId) ||
                 (x.SenderId == friendId && x.ReceiverId == userId.Value)) &&
                x.Status == FriendRequestStatus.Accepted, ct);

        if (friendRequest is null)
            ThrowError("Friend relationship not found.", StatusCodes.Status404NotFound);

        db.FriendRequests.Remove(friendRequest);
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
