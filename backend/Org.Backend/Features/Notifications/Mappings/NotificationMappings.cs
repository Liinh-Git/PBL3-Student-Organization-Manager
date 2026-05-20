using Org.Backend.Domain.Entities;
using Org.Shared.Features.Notifications;

namespace Org.Backend.Features.Notifications.Mappings;

public static class NotificationMappings
{
    public static NotificationDto ToNotificationDto(this Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            ReceiverId = notification.ReceiverId,
            ActorId = notification.ActorId,
            ActorName = notification.Actor?.FullName,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString(),
            RelatedEntityType = notification.RelatedEntityType,
            RelatedEntityId = notification.RelatedEntityId,
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAtUtc = notification.CreatedAt
        };
    }
}
