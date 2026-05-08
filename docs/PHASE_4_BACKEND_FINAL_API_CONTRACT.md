# PHASE_4_BACKEND_FINAL_API_CONTRACT

Contract freeze date: 2026-05-08

Response envelope (all groups): `ApiResponse<T>` with fields `success`, `data`, `message`, `errors`.

## Auth
- `POST /api/auth/login`: login request (`email`, `password`) -> token/session payload.
- `POST /api/auth/register`: registration payload -> created user auth payload.
- `GET /api/auth/me`: no body -> current auth user profile.
- Auth/Permission: JWT required for `/me`; login/register are public.
- Known limitations: standard credential flow only.

## Users
- `GET /api/users/me`
- `PUT /api/users/me`
- `PUT /api/users/me/change-password`
- `GET /api/users/me/organizations`
- `GET /api/users/me/events`
- `GET /api/users/me/discover/organizations`
- Request/Response: profile/update/change-password/discovery DTOs.
- Auth: JWT required.
- Known limitations: discover organizations is basic list behavior.

## Organizations
- `GET /api/organizations`
- `POST /api/organizations`
- `GET /api/organizations/default`
- `GET /api/organizations/{id}`
- `PUT /api/organizations/{id}`
- `GET /api/organizations/{id}/public-overview`
- Auth/Permission: JWT required; write requires org permission checks.
- Known limitations: no advanced governance workflow.

## RolesPermissions
- `GET /api/organizations/{orgId}/permissions/me`
- `GET /api/organizations/{orgId}/permissions`
- `GET /api/organizations/{orgId}/roles`
- `POST /api/organizations/{orgId}/roles`
- `PUT /api/organizations/roles/{roleId}`
- `DELETE /api/organizations/roles/{roleId}`
- `POST /api/organizations/{orgId}/members/{memberId}/role`
- Auth/Permission: JWT + role/permission checks.
- Known limitations: no bulk role assignment APIs.

## Members
- `GET /api/organizations/{orgId}/members`
- `POST /api/organizations/{orgId}/members`
- `PUT /api/members/{id}/department`
- `DELETE /api/members/{id}`
- Auth/Permission: JWT + org member management permissions.
- Known limitations: no advanced member audit trail API.

## Departments
- `GET /api/organizations/{orgId}/departments`
- `POST /api/organizations/{orgId}/departments`
- `GET /api/departments/{id}`
- `PUT /api/departments/{id}`
- `DELETE /api/departments/{id}`
- Auth/Permission: JWT + department management permissions for writes.
- Known limitations: delete is soft-delete behavior.

## Events
- `GET /api/organizations/{orgId}/events`
- `POST /api/organizations/{orgId}/events`
- `GET /api/events/{id}`
- `PUT /api/events/{id}`
- `DELETE /api/events/{id}`
- `GET /api/events/public`
- `GET /api/events/{id}/public`
- Auth/Permission: public reads for public routes; JWT + perms for writes.
- Known limitations: no attendee working API in this phase.

## Milestones
- `GET /api/events/{eventId}/milestones`
- `POST /api/events/{eventId}/milestones`
- `GET /api/milestones/{id}`
- `PUT /api/milestones/{id}`
- `DELETE /api/milestones/{id}`
- Auth/Permission: JWT + event workspace permissions.
- Known limitations: no milestone analytics API.

## EventCategories
- `GET /api/milestones/{milestoneId}/categories`
- `POST /api/milestones/{milestoneId}/categories`
- `GET /api/categories/{id}`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`
- Auth/Permission: JWT + workspace permissions.
- Known limitations: no category templates API.

## Tasks
- `POST /api/categories/{categoryId}/tasks`
- `GET /api/tasks/{taskId}`
- `PUT /api/tasks/{taskId}`
- `DELETE /api/tasks/{taskId}`
- `PUT /api/tasks/{taskId}/status`
- `PUT /api/tasks/{taskId}/assign`
- Auth/Permission: JWT + workspace permissions.
- Known limitations: no comments/time-tracking module.

## Requests
- `GET /api/organizations/{orgId}/requests`
- `POST /api/organizations/{orgId}/requests`
- `GET /api/requests/{requestId}`
- `POST /api/organizations/requests/{requestId}/review`
- Request body summary: create/review DTOs for request workflow.
- Response summary: request DTO with status/reviewer metadata.
- Auth/Permission: JWT for create; org request permissions for list/get/review.
- Known limitations: basic review workflow; no advanced approval chain.

## Notifications
- `GET /api/notifications`
- `GET /api/notifications/unread-count`
- `POST /api/notifications/{id}/read`
- `POST /api/notifications/read-all`
- Request body summary: no complex body required.
- Response summary: notification DTO/unread count/bool success.
- Auth/Permission: JWT required.
- Known limitations: no SignalR realtime; no delete API.

## Friends
- `GET /api/friends`
- `GET /api/friends/requests`
- `POST /api/friends/requests`
- `POST /api/friends/requests/{id}/accept`
- `POST /api/friends/requests/{id}/reject`
- Request body summary: send request with receiverId; accept/reject by route id.
- Response summary: friend DTO/friend request DTO/bool success.
- Auth/Permission: JWT required.
- Known limitations: no unfriend/block/search APIs.

## Discover
- `GET /api/users/me/discover/organizations`
- `GET /api/users/me/discover/events`
- Request body summary: none.
- Response summary: list DTOs for discoverable orgs/events.
- Auth/Permission: JWT required.
- Known limitations: no pagination/filter/search parameters.
