# MVP API Smoke Checklist

This checklist is used to verify the core backend functionality of the Student Organization Manager system. Tests should be performed against a local PostgreSQL database.

> [!IMPORTANT]
> - Ensure `dotnet run` is active for the Backend service.
> - Use a tool like Postman, Bruno, or `curl` to execute these requests.
> - Replace `{id}`, `{orgId}`, etc., with real GUIDs from your database.

## 1. Authentication
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/auth/login` | POST | No | `{"email": "admin@org.com", "password": "password123"}` | `200 OK`, Returns JWT token + User Info
`/api/auth/register` | POST | No | `{"fullName": "New User", "email": "user@test.com", "password": "password123"}` | `201 Created`, Returns User DTO
`/api/users/me/change-password` | PUT | Yes | `{"currentPassword": "...", "newPassword": "..."}` | `204 No Content`

## 2. Organizations
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/organizations` | GET | Yes | `?search=Club&page=1` | `200 OK`, List of organizations
`/api/organizations/{id}` | GET | Yes | N/A | `200 OK`, Detailed organization info
`/api/organizations` | POST | Yes | `{"name": "New Club", "description": "...", "location": "..."}` | `201 Created`, Returns Org DTO
`/api/organizations/{id}` | PUT | Yes | `{"name": "Updated Name", "description": "...", "isActive": true}` | `200 OK`, Returns updated Org DTO
`/api/organizations/{id}` | DELETE | Yes | N/A | `204 No Content` (Soft delete)
`/api/organizations/default` | GET | Yes | N/A | `200 OK`, Returns the first active organization

## 3. Members
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/organizations/{orgId}/members` | GET | Yes | N/A | `200 OK`, List of members with roles
`/api/organizations/{orgId}/members` | POST | Yes | `{"fullName": "Member Name", "email": "member@test.com"}` | `201 Created`, Creates User (if needed) and Member
`/api/members/{id}/role` | PUT | Yes | `{"role": "Manager"}` | `200 OK`, Role updated
`/api/members/{id}/department` | PUT | Yes | `{"departmentId": "{deptId}"}` | `200 OK`, Member assigned to department
`/api/members/{id}` | DELETE | Yes | N/A | `204 No Content` (Soft delete)
`/api/organizations/{orgId}/leave` | POST | Yes | N/A | `204 No Content`, Current user leaves organization

## 4. Departments
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/organizations/{orgId}/departments` | GET | Yes | `?search=HR` | `200 OK`, List of departments
`/api/departments` | POST | Yes | `{"organizationId": "{orgId}", "name": "HR Dept", "code": "HR01"}` | `201 Created`
`/api/departments/{id}` | PUT | Yes | `{"name": "New Name", "isActive": true}` | `200 OK`
`/api/departments/{id}/members` | GET | Yes | N/A | `200 OK`, List of members in department
`/api/departments/{id}/members/{memberId}` | POST | Yes | N/A | `200 OK`, Member assigned to department
`/api/departments/{id}/members/{memberId}` | DELETE | Yes | N/A | `204 No Content`, Member removed from department

## 5. Events
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/events/public` | GET | Yes | N/A | `200 OK`, List of upcoming public events
`/api/organizations/{orgId}/events` | GET | Yes | N/A | `200 OK`, List of all events in organization
`/api/events` | POST | Yes | `{"organizationId": "{orgId}", "name": "Event A", "startDate": "2024-10-01"}` | `201 Created`
`/api/events/{id}` | GET | Yes | N/A | `200 OK`, Event details
`/api/events/{id}` | PUT | Yes | `{"name": "Updated", "status": "Active"}` | `200 OK`
`/api/events/{id}/ratings` | GET | Yes | N/A | `200 OK`, List of ratings/comments

## 6. Event Categories & Milestones
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/events/{eventId}/milestones` | GET | Yes | N/A | `200 OK`, List of milestones
`/api/milestones/{milestoneId}/categories` | GET | Yes | N/A | `200 OK`, List of categories in milestone
`/api/milestones/{id}` | PUT | Yes | `{"name": "Phase 1", "status": "InProgress"}` | `200 OK`
`/api/categories/{id}` | PUT | Yes | `{"name": "Logistics"}` | `200 OK`

## 7. Tasks
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/categories/{categoryId}/tasks` | GET | Yes | N/A | `200 OK`, List of tasks
`/api/tasks/{taskId}` | GET | Yes | N/A | `200 OK`, Task details
`/api/tasks/{taskId}/status` | PUT | Yes | `{"status": "InProgress"}` | `200 OK`
`/api/tasks/{taskId}/assign` | PUT | Yes | `{"assigneeMemberId": "{memberId}"}` | `200 OK`

## 8. Notifications
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/notifications` | GET | Yes | `?isRead=false` | `200 OK`, List of notifications
`/api/notifications/unread-count` | GET | Yes | N/A | `200 OK`, Integer count
`/api/notifications/{id}/read` | PUT | Yes | N/A | `200 OK`, Marked as read

## 9. User Profiles & Social
Endpoint | Method | Auth | Request Sample | Expected Result
---|---|---|---|---
`/api/users/me` | GET | Yes | N/A | `200 OK`, Own profile
`/api/users/{id}` | GET | Yes | N/A | `200 OK`, Other user's profile (if public/same org)
`/api/users/{id}/friend-request` | POST | Yes | N/A | `201 Created`
`/api/users/me/friend-requests` | GET | Yes | N/A | `200 OK`, Pending requests
`/api/users/me/friends` | GET | Yes | N/A | `200 OK`, List of friends

## 10. Deferred / Not MVP
The following modules/features are present in the code but are **not** part of the MVP smoke test. They may be incomplete or disabled in the UI.
- **Posts & Comments**: No live integration required for MVP.
- **Messages & Chat**: Navigation is hidden; backend logic is deferred.
- **Finance**: Unfinished feature.
- **Resources**: Unfinished feature.
- **Reports**: Unfinished feature.
