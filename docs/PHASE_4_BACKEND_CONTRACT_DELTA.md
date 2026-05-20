# PHASE_4_BACKEND_CONTRACT_DELTA

## Freeze Status
- Date: 2026-05-08
- Status: Frozen for FE integration in current phase.

## Supporting API Delta Added

### Requests
- `GET /api/organizations/{orgId}/requests`
- `POST /api/organizations/{orgId}/requests`
- `GET /api/requests/{requestId}`
- `POST /api/organizations/requests/{requestId}/review`
- Envelope: `ApiResponse<RequestDto>` or `ApiResponse<List<RequestDto>>`

### Notifications
- `GET /api/notifications`
- `GET /api/notifications/unread-count`
- `POST /api/notifications/{id}/read`
- `POST /api/notifications/read-all`
- Envelope: `ApiResponse<NotificationDto>`, `ApiResponse<List<NotificationDto>>`, `ApiResponse<UnreadCountDto>`, `ApiResponse<bool>`

### Friends
- `GET /api/friends`
- `GET /api/friends/requests`
- `POST /api/friends/requests`
- `POST /api/friends/requests/{id}/accept`
- `POST /api/friends/requests/{id}/reject`
- Envelope: `ApiResponse<FriendDto>`, `ApiResponse<List<FriendDto>>`, `ApiResponse<FriendRequestDto>`, `ApiResponse<List<FriendRequestDto>>`, `ApiResponse<bool>`

### Discover
- `GET /api/users/me/discover/events`
- Existing retained: `GET /api/users/me/discover/organizations`
- Envelope: `ApiResponse<List<DiscoverEventDto>>` and existing discover org DTO envelope

## Build/Run Contract Notes
- Supporting endpoints were fixed to project FastEndpoints pattern:
  - Success: `Response = ApiResponse<T>.SuccessResponse(...)`
  - Error: `HttpContext.Response.StatusCode = <code>; Response = ApiResponse<T>.ErrorResponse(...)`

## Endpoint Count
- Registered endpoints after fix: 67

## FE Guidance
- FE may integrate against supporting routes above.
- FE must not integrate excluded modules listed in `docs/FE_ALLOWED_ENDPOINTS.md`.
