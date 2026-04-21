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
// Bảo mật: chỉ xem được nếu cùng tổ chức và có quyền CanRead
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

        if (callerUserId.Value != id)
        {
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
        }

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
            ToUtcOffset(user.CreatedAt),
            user.UpdatedAt is null ? null : ToUtcOffset(user.UpdatedAt.Value),
            user.LastLogin is null ? null : ToUtcOffset(user.LastLogin.Value));

    private static DateTimeOffset ToUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}