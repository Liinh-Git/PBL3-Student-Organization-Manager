# Notifications Module

## Module Purpose
In-app notification management for users.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `Notification`, `User`
- Enums: `NotificationType`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/notifications` | List user notifications |
| GET | `/api/notifications/unread-count` | Get unread notification count |
| POST | `/api/notifications/{id}/read` | Mark notification as read |
| POST | `/api/notifications/read-all` | Mark all notifications as read |

## Required Permissions
- Valid JWT token (user-scoped endpoints)

## Important Notes
- Base prototype uses REST only
- SignalR is optional future enhancement
- Do not block readiness on SignalR
- Notifications are user-scoped, not organization-scoped

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Notifications/NotificationContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/notificationService.js`
- Future adapter: `frontend/org-frontend/src/adapters/notificationAdapter.js`
- Future component: `NotificationBadge.jsx`
- Permissions: Valid JWT token
- Status: **CORE**
