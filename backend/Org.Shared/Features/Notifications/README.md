# Notifications Module Contracts

## Module Purpose
In-app notification management (list, unread count, mark read).

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Notifications/`

## Related Domain Entities
- `Notification`, `User`, `NotificationType` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/notifications` | JWT | None | `ApiResponse<ListResponse<NotificationDto>>` |
| GET | `/api/notifications/unread-count` | JWT | None | `ApiResponse<UnreadCountResponse>` |
| POST | `/api/notifications/{id}/read` | JWT | None | `ApiResponse<MarkNotificationReadResponse>` |
| POST | `/api/notifications/read-all` | JWT | None | `ApiResponse<bool>` |

## Future Request DTO Names
- None (all actions use route parameters)

## Future Response DTO Names
- `NotificationDto`, `UnreadCountResponse`, `MarkNotificationReadResponse`

## Future Frontend Service File
`frontend/org-frontend/src/services/notificationService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/notificationAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/components/notifications/NotificationBadge.jsx`

## Required Permissions
- All routes require JWT token (authenticated user)

## Contract Notes

### NotificationDto
- **Fields**: `Id`, `ReceiverId`, `ActorId?`, `ActorName?`, `Title`, `Message`, `Type`, `RelatedEntityType?`, `RelatedEntityId?`, `ActionUrl?`, `IsRead`, `ReadAt?`, `CreatedAtUtc`
- **Note**: REST only, SignalR optional future

### UnreadCountResponse
- **Fields**: `UnreadCount`
- **Note**: For notification badge

### MarkNotificationReadResponse
- **Fields**: `Success`, `UnreadCount`
- **Note**: Returns updated unread count after marking read

## Validation Notes
- **Title**: Required, max 200 characters
- **Message**: Required, max 1000 characters
- **Type**: Required, System/RequestSubmitted/RequestReviewed/FriendRequest/EventCreated/etc.

## Mapping Notes
- **Entity → DTO**: Map `Notification` entity to `NotificationDto`, include actor name
- **DTO → Entity**: Map request DTOs to `Notification` entity

## What is NOT Implemented in This Phase
- ❌ No real notification logic
- ❌ No SignalR real-time notifications
- ❌ Only contract skeleton/TODO files

## Important Note
**REST is base**. SignalR is optional future enhancement.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Notifications/`
- **Shared Contract**: `backend/Org.Shared/Features/Notifications/NotificationContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/notificationService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/notificationAdapter.js`
- **Frontend Components**: `NotificationBadge.jsx`

---

**End of Notifications README.md**
