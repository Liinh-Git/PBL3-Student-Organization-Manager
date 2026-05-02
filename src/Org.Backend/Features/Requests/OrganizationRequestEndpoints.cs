using System.Security.Claims;
using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Backend.Services;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests;

public sealed class GetOrganizationRequestsEndpoint(AppDbContext db)
    : EndpointWithoutRequest<GetOrganizationRequestsResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{id:guid}/requests");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("id");
        var organizationExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!organizationExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var permissionKeys = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), permissionKeys);
        if (!capabilities.CanViewRequests)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var statusQuery = HttpContext.Request.Query["status"].ToString();
        var statusFilter = OrganizationRequestMapping.ParseStatusFilter(statusQuery);

        var query = db.Requests
            .AsNoTracking()
            .Include(x => x.Sender)
            .Where(x => x.OrgId == orgId);

        if (statusFilter.HasValue)
            query = query.Where(x => x.Status == statusFilter.Value);

        var requests = await query
            .OrderByDescending(x => x.RequestDate)
            .ToListAsync(ct);

        var items = requests.Select(OrganizationRequestMapping.MapRequest).ToList();
        await Send.OkAsync(new GetOrganizationRequestsResponse(items), ct);
    }
}

public sealed class GetOrganizationRequestByIdEndpoint(AppDbContext db)
    : EndpointWithoutRequest<GetOrganizationRequestByIdResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/requests/{requestId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var requestId = Route<Guid>("requestId");
        var request = await db.Requests
            .AsNoTracking()
            .Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == requestId, ct);

        if (request is null)
            ThrowError("Request not found.", StatusCodes.Status404NotFound);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, request!.OrgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var permissionKeys = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), permissionKeys);
        if (!capabilities.CanViewRequests)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await Send.OkAsync(new GetOrganizationRequestByIdResponse(OrganizationRequestMapping.MapRequest(request)), ct);
    }
}

public sealed class CreateOrganizationRequestEndpoint(AppDbContext db, INotificationService notificationService)
    : Endpoint<CreateOrganizationRequestSubmissionRequest, OrganizationRequestDto>
{
    public override void Configure()
    {
        Post("/api/organizations/{id:guid}/requests");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateOrganizationRequestSubmissionRequest req, CancellationToken ct)
    {
        var orgId = Route<Guid>("id");
        var organizationExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!organizationExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (caller is null)
            ThrowError("Only organization members can submit organization requests.", StatusCodes.Status403Forbidden);

        var normalizedType = OrganizationRequestMapping.NormalizeType(req.RequestType);
        var title = OrganizationRequestMapping.NormalizeOptional(req.Title);
        var message = OrganizationRequestMapping.NormalizeOptional(req.Message);

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(message))
            ThrowError("Either title or message is required.", StatusCodes.Status400BadRequest);

        var payload = new OrganizationRequestMapping.StoredRequestContent
        {
            RequestType = normalizedType,
            Title = title,
            Message = message,
            DesiredDepartment = OrganizationRequestMapping.NormalizeOptional(req.DesiredDepartment),
            DesiredPosition = OrganizationRequestMapping.NormalizeOptional(req.DesiredPosition),
            Experience = OrganizationRequestMapping.NormalizeOptional(req.Experience),
            Strengths = OrganizationRequestMapping.NormalizeOptional(req.Strengths),
            Reason = OrganizationRequestMapping.NormalizeOptional(req.Reason)
        };

        var request = new Domain.Entities.Request
        {
            SenderId = userId,
            OrgId = orgId,
            RequestType = OrganizationRequestMapping.ToDomainRequestType(normalizedType),
            Status = RequestStatus.Pending,
            RequestDate = DateTime.UtcNow,
            Content = JsonSerializer.Serialize(payload)
        };

        db.Requests.Add(request);
        await db.SaveChangesAsync(ct);

        var reviewerIds = await OrganizationRequestMapping.ResolveReviewerUserIdsAsync(db, orgId, ct);
        foreach (var reviewerUserId in reviewerIds.Where(x => x != userId))
        {
            await notificationService.CreateNotification(
                receiverId: reviewerUserId,
                title: "New organization request",
                message: title ?? message ?? "A new request needs review.",
                type: NotificationType.JoinRequestReceived,
                actorId: userId,
                relatedEntityId: request.Id,
                relatedEntityType: "Request",
                actionUrl: "/org/requests");
        }

        var created = await db.Requests
            .AsNoTracking()
            .Include(x => x.Sender)
            .FirstAsync(x => x.Id == request.Id, ct);

        await HttpContext.Response.SendAsync(OrganizationRequestMapping.MapRequest(created), StatusCodes.Status201Created, cancellation: ct);
    }
}

public sealed class ReviewOrganizationRequestEndpoint(AppDbContext db, INotificationService notificationService)
    : Endpoint<ReviewOrganizationRequestSubmissionRequest, OrganizationRequestDto>
{
    public override void Configure()
    {
        Post("/api/organizations/requests/{requestId:guid}/review");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ReviewOrganizationRequestSubmissionRequest req, CancellationToken ct)
    {
        var requestId = Route<Guid>("requestId");
        var request = await db.Requests
            .Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == requestId, ct);

        if (request is null)
            ThrowError("Request not found.", StatusCodes.Status404NotFound);

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var reviewerUserId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, request!.OrgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var permissionKeys = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), permissionKeys);
        if (!capabilities.CanReviewRequests)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (request.Status != RequestStatus.Pending)
            ThrowError("Only pending requests can be reviewed.", StatusCodes.Status409Conflict);

        var decision = req.Decision?.Trim().ToUpperInvariant();
        request.Status = decision switch
        {
            "APPROVE" or "APPROVED" => RequestStatus.Approved,
            "REJECT" or "REJECTED" => RequestStatus.Rejected,
            _ => throw new ValidationFailureException("Decision must be APPROVE or REJECT.")
        };

        var payload = OrganizationRequestMapping.ParseStoredContent(request.Content);
        payload.ReviewResponse = OrganizationRequestMapping.NormalizeOptional(req.ResponseMessage);
        payload.ReviewedAtUtc = DateTime.UtcNow;
        payload.ReviewedByUserId = reviewerUserId;
        request.Content = JsonSerializer.Serialize(payload);

        await db.SaveChangesAsync(ct);

        var statusLabel = request.Status == RequestStatus.Approved ? "approved" : "rejected";
        await notificationService.CreateNotification(
            receiverId: request.SenderId,
            title: $"Your request was {statusLabel}",
            message: payload.Title ?? payload.Message ?? "Your organization request has been reviewed.",
            type: request.Status == RequestStatus.Approved ? NotificationType.JoinRequestApproved : NotificationType.JoinRequestRejected,
            actorId: reviewerUserId,
            relatedEntityId: request.Id,
            relatedEntityType: "Request",
            actionUrl: "/user/organizations");

        var updated = await db.Requests
            .AsNoTracking()
            .Include(x => x.Sender)
            .FirstAsync(x => x.Id == request.Id, ct);

        await Send.OkAsync(OrganizationRequestMapping.MapRequest(updated), ct);
    }
}

internal static class OrganizationRequestMapping
{
    public static OrganizationRequestDto MapRequest(Domain.Entities.Request request)
    {
        var payload = ParseStoredContent(request.Content);
        return new OrganizationRequestDto(
            request.Id,
            request.OrgId,
            request.SenderId,
            request.Sender.FullName,
            request.Sender.Email,
            request.Sender.AvatarUrl,
            NormalizeType(payload.RequestType ?? request.RequestType.ToString()),
            request.Status.ToString().ToUpperInvariant(),
            payload.Title,
            payload.Message,
            payload.DesiredDepartment,
            payload.DesiredPosition,
            payload.Experience,
            payload.Strengths,
            payload.Reason,
            payload.ReviewResponse,
            payload.ReviewedByUserId,
            ToUtcOffset(request.RequestDate),
            payload.ReviewedAtUtc.HasValue ? ToUtcOffset(payload.ReviewedAtUtc.Value) : null);
    }

    public static StoredRequestContent ParseStoredContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new StoredRequestContent();

        try
        {
            var parsed = JsonSerializer.Deserialize<StoredRequestContent>(content);
            if (parsed is not null)
                return parsed;
        }
        catch
        {
            // Fallback to plain-text payload.
        }

        return new StoredRequestContent { Message = content };
    }

    public static RequestStatus? ParseStatusFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return RequestStatus.Pending;

        return value.Trim().ToUpperInvariant() switch
        {
            "PENDING" => RequestStatus.Pending,
            "APPROVED" => RequestStatus.Approved,
            "REJECTED" => RequestStatus.Rejected,
            "ALL" => null,
            _ => throw new ValidationFailureException("Invalid status filter.")
        };
    }

    public static string NormalizeType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "GENERAL_ORG_REQUEST";

        return value.Trim().ToUpperInvariant() switch
        {
            "JOIN" => "JOIN",
            "JOINCLUB" => "JOIN",
            "JOIN_ORG" => "JOIN",
            _ => value.Trim()
        };
    }

    public static RequestType ToDomainRequestType(string normalizedType)
    {
        return normalizedType switch
        {
            "JOIN" => RequestType.JoinClub,
            "RESOURCE_BORROW" => RequestType.ResourceBorrow,
            _ => RequestType.ApproveEvent
        };
    }

    public static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static DateTimeOffset ToUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public static async Task<IReadOnlyList<Guid>> ResolveReviewerUserIdsAsync(AppDbContext db, Guid orgId, CancellationToken ct)
    {
        var rolePermissions = await db.RolePermissions
            .AsNoTracking()
            .Where(x => x.Role.OrgId == orgId)
            .Select(x => new { x.RoleId, x.Permission.PermissionKey })
            .ToListAsync(ct);

        var rolePermissionMap = rolePermissions
            .GroupBy(x => x.RoleId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.PermissionKey).ToHashSet(StringComparer.OrdinalIgnoreCase));

        var members = await db.Members
            .AsNoTracking()
            .Include(x => x.Role)
            .Where(x => x.OrgId == orgId)
            .ToListAsync(ct);

        var result = new HashSet<Guid>();
        foreach (var member in members)
        {
            var permissionKeys = member.RoleId.HasValue
                ? rolePermissionMap.GetValueOrDefault(member.RoleId.Value) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(
                isAuthenticated: true,
                isMember: true,
                memberRole: member.Role?.RoleName,
                permissionKeys);

            if (capabilities.CanReviewRequests)
                result.Add(member.UserId);
        }

        return result.ToList();
    }

    public sealed class StoredRequestContent
    {
        public string? RequestType { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? DesiredDepartment { get; set; }
        public string? DesiredPosition { get; set; }
        public string? Experience { get; set; }
        public string? Strengths { get; set; }
        public string? Reason { get; set; }
        public string? ReviewResponse { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }
    }
}
