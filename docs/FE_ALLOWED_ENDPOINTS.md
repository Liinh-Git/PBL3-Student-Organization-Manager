# FE_ALLOWED_ENDPOINTS

## FE May Safely Call Now
- All implemented Auth, Users, Organizations, RolesPermissions, Members, Departments, Events, Milestones, EventCategories, Tasks endpoints.
- Supporting endpoints:
  - `GET /api/organizations/{orgId}/requests`
  - `POST /api/organizations/{orgId}/requests`
  - `GET /api/requests/{requestId}`
  - `POST /api/organizations/requests/{requestId}/review`
  - `GET /api/notifications`
  - `GET /api/notifications/unread-count`
  - `POST /api/notifications/{id}/read`
  - `POST /api/notifications/read-all`
  - `GET /api/friends`
  - `GET /api/friends/requests`
  - `POST /api/friends/requests`
  - `POST /api/friends/requests/{id}/accept`
  - `POST /api/friends/requests/{id}/reject`
  - `GET /api/users/me/discover/events`
  - `GET /api/users/me/discover/organizations`

## FE Should Avoid (Not Implemented / Excluded)
- Posts
- Comments
- Messages/Chat
- Finance module APIs
- Reports working module APIs
- Resources working module APIs
- EventRatings APIs
- EventMembers/Attendees working APIs
- DigitalAssets upload APIs
- ActivityHistory feed APIs
