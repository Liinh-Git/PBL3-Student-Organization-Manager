// ---- Các endpoint CRUD cho module thông báo (Notification) ----
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Enums;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Notifications;
using System.Security.Claims;

namespace Org.Backend.Features.Notifications;

// ---- GET /api/notifications — lấy danh sách thông báo của user hiện tại ----
public sealed class GetNotificationsEndpoint(AppDbContext db) : Endpoint<GetNotificationsRequest, GetNotificationsResponse>
{
    public override void Configure()
    {
        Get("/api/notifications");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetNotificationsRequest req, CancellationToken ct)
    {
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        if (req.Page < 1)
            ThrowError("Page must be greater than 0.", StatusCodes.Status400BadRequest);

        if (req.PageSize < 1 || req.PageSize > 100)
            ThrowError("PageSize must be between 1 and 100.", StatusCodes.Status400BadRequest);

        var query = db.Notifications
            .AsNoTracking()
            .Include(x => x.Actor)
            .Where(x => x.ReceiverId == userId);

        // Filter by IsRead
        if (req.IsRead.HasValue)
            query = query.Where(x => x.IsRead == req.IsRead.Value);

        // Filter by Type
        if (!string.IsNullOrWhiteSpace(req.Type))
        {
            if (Enum.TryParse<NotificationType>(req.Type, ignoreCase: true, out var type))
                query = query.Where(x => x.Type == type);
        }

        var totalCount = await query.CountAsync(ct);
        var unreadCount = await db.Notifications
            .AsNoTracking()
            .Where(x => x.ReceiverId == userId && !x.IsRead)
            .CountAsync(ct);

        var skip = (req.Page - 1) * req.PageSize;

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(req.PageSize)
            .Select(x => new NotificationDto(
                x.Id,
                x.Title,
                x.Message,
                x.Type.ToString(),
                x.IsRead,
                x.ReadAt,
                x.ActionUrl,
                x.IconUrl,
                x.Actor == null ? null : new NotificationActorDto(
                    x.Actor.Id,
                    x.Actor.FullName,
                    x.Actor.AvatarUrl),
                new DateTimeOffset(DateTime.SpecifyKind(x.CreatedAt, DateTimeKind.Utc))))
            .ToListAsync(ct);

        await Send.OkAsync(new GetNotificationsResponse(items, totalCount, unreadCount, req.Page, req.PageSize), ct);
    }
}

// ---- GET /api/notifications/unread-count — số lượng thông báo chưa đọc ----
public sealed class GetUnreadCountEndpoint(AppDbContext db) : EndpointWithoutRequest<GetUnreadCountResponse>
{
    public override void Configure()
    {
        Get("/api/notifications/unread-count");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var count = await db.Notifications
            .AsNoTracking()
            .Where(x => x.ReceiverId == userId && !x.IsRead)
            .CountAsync(ct);

        await Send.OkAsync(new GetUnreadCountResponse(count), ct);
    }
}

// ---- GET /api/notifications/{id} — chi tiết một thông báo (tự động mark as read) ----
public sealed class GetNotificationByIdEndpoint(AppDbContext db) : EndpointWithoutRequest<GetNotificationByIdResponse>
{
    public override void Configure()
    {
        Get("/api/notifications/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var notification = await db.Notifications
            .Include(x => x.Actor)
            .FirstOrDefaultAsync(x => x.Id == id && x.ReceiverId == userId, ct);

        if (notification is null)
            ThrowError("Notification not found.", StatusCodes.Status404NotFound);

        // Tự động mark as read khi xem
        if (!notification!.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        var dto = new NotificationDto(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type.ToString(),
            notification.IsRead,
            notification.ReadAt,
            notification.ActionUrl,
            notification.IconUrl,
            notification.Actor == null ? null : new NotificationActorDto(
                notification.Actor.Id,
                notification.Actor.FullName,
                notification.Actor.AvatarUrl),
            new DateTimeOffset(DateTime.SpecifyKind(notification.CreatedAt, DateTimeKind.Utc)));

        await Send.OkAsync(new GetNotificationByIdResponse(dto), ct);
    }
}

// ---- PUT /api/notifications/{id}/read — đánh dấu một thông báo là đã đọc ----
public sealed class MarkAsReadEndpoint(AppDbContext db) : EndpointWithoutRequest<MarkAsReadResponse>
{
    public override void Configure()
    {
        Put("/api/notifications/{id:guid}/read");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var notification = await db.Notifications
            .Include(x => x.Actor)
            .FirstOrDefaultAsync(x => x.Id == id && x.ReceiverId == userId, ct);

        if (notification is null)
            ThrowError("Notification not found.", StatusCodes.Status404NotFound);

        if (!notification!.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        var dto = new NotificationDto(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type.ToString(),
            notification.IsRead,
            notification.ReadAt,
            notification.ActionUrl,
            notification.IconUrl,
            notification.Actor == null ? null : new NotificationActorDto(
                notification.Actor.Id,
                notification.Actor.FullName,
                notification.Actor.AvatarUrl),
            new DateTimeOffset(DateTime.SpecifyKind(notification.CreatedAt, DateTimeKind.Utc)));

        await Send.OkAsync(new MarkAsReadResponse(dto), ct);
    }
}

// ---- PUT /api/notifications/read-all — đánh dấu tất cả thông báo là đã đọc ----
public sealed class MarkAllAsReadEndpoint(AppDbContext db) : EndpointWithoutRequest<MarkAllAsReadResponse>
{
    public override void Configure()
    {
        Put("/api/notifications/read-all");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var unreadNotifications = await db.Notifications
            .Where(x => x.ReceiverId == userId && !x.IsRead)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await db.SaveChangesAsync(ct);

        await Send.OkAsync(new MarkAllAsReadResponse(unreadNotifications.Count), ct);
    }
}

// ---- DELETE /api/notifications/{id} — xóa một thông báo ----
public sealed class DeleteNotificationEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/notifications/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var notification = await db.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.ReceiverId == userId, ct);

        if (notification is null)
            ThrowError("Notification not found.", StatusCodes.Status404NotFound);

        notification!.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

// ---- DELETE /api/notifications/clear-all — xóa tất cả thông báo (hoặc chỉ đã đọc) ----
public sealed class ClearAllNotificationsEndpoint(AppDbContext db) : Endpoint<ClearNotificationsRequest, ClearNotificationsResponse>
{
    public override void Configure()
    {
        Delete("/api/notifications/clear-all");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ClearNotificationsRequest req, CancellationToken ct)
    {
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var query = db.Notifications.Where(x => x.ReceiverId == userId);

        // Nếu chỉ xóa đã đọc
        if (req.OnlyRead)
            query = query.Where(x => x.IsRead);

        var notifications = await query.ToListAsync(ct);

        foreach (var notification in notifications)
        {
            notification.IsDeleted = true;
        }

        await db.SaveChangesAsync(ct);

        await Send.OkAsync(new ClearNotificationsResponse(notifications.Count), ct);
    }
}
