# CROSS_LAYER_TRACEABILITY

## Purpose
Complete traceability from Domain Entity → Backend Feature → Shared Contract → Frontend Service → Frontend Adapter → Frontend Page/Component → Permission for all CORE and SUPPORTING modules.

---

## Auth Module

**Status**: CORE

**Domain Entities**:
- User.cs (FullName, Email, PasswordHash, Status, etc.)
- UserStatus.cs enum (Active, Inactive, Suspended)

**Backend Feature Folder**: `backend/Org.Backend/Features/Auth/`
- README.md, Endpoints/README.md, Services/README.md, Validators/README.md, Mappings/README.md, Permissions.TODO.md

**Backend Routes**:
- POST /api/auth/login
- POST /api/auth/register
- GET /api/auth/me

**Shared Contract File**: `backend/Org.Shared/Features/Auth/AuthContracts.cs.TODO`
- LoginRequest, RegisterRequest
- AuthUserDto, AuthTokenResponse, CurrentUserResponse

**Frontend Service**: `frontend/src/services/authService.js`
- login(credentials), register(payload), getCurrentUser(), logoutLocalOnly()

**Frontend Adapter**: `frontend/src/adapters/userAdapter.js`
- toUserProfileViewModel(dto)

**Frontend Pages/Components**:
- LoginPage.jsx, RegisterPage.jsx

**Permissions**: Public (login/register), JWT (me)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement JWT token generation/validation, password hashing, user authentication logic

**Risks/Notes**: JWT implementation deferred; no real auth logic yet

---

## Users Module

**Status**: CORE

**Domain Entities**:
- User.cs (all user profile fields)
- ProfileVisibility.cs enum (Public, OrganizationOnly, Private)

**Backend Feature Folder**: `backend/Org.Backend/Features/Users/`

**Backend Routes**:
- GET /api/users/me
- PUT /api/users/me
- PUT /api/users/me/change-password
- GET /api/users/me/organizations
- GET /api/users/me/events
- GET /api/users/me/discover/organizations

**Shared Contract File**: `backend/Org.Shared/Features/Users/UserContracts.cs.TODO`
- UpdateUserProfileRequest, ChangePasswordRequest
- UserProfileDto, MyOrganizationDto, MyEventDto, DiscoverOrganizationDto

**Frontend Service**: `frontend/src/services/userService.js`
- getMe(), updateMe(payload), changePassword(payload), getMyOrganizations(params), getMyEvents(params), discoverMyOrganizations(params)

**Frontend Adapter**: `frontend/src/adapters/userAdapter.js`
- toUserProfileViewModel(dto), toMyOrganizationViewModel(dto), toMyEventViewModel(dto), toDiscoverOrganizationViewModel(dto)

**Frontend Pages/Components**:
- UserProfilePage.jsx, UserSettingsPage.jsx, UserOrganizationsPage.jsx, UserEventsPage.jsx

**Permissions**: JWT

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement user profile CRUD, password change logic, getMyOrganizations query

**Risks/Notes**: getMyOrganizations belongs to userService, NOT organizationService

---

## Organizations Module

**Status**: CORE

**Domain Entities**:
- Organization.cs (OrgName, Description, Status, TotalMembers, etc.)
- OrgStatus.cs enum (Active, Suspended, Archived)

**Backend Feature Folder**: `backend/Org.Backend/Features/Organizations/`

**Backend Routes**:
- GET /api/organizations
- POST /api/organizations
- GET /api/organizations/default
- GET /api/organizations/{id}
- PUT /api/organizations/{id}
- GET /api/organizations/{id}/public-overview

**Shared Contract File**: `backend/Org.Shared/Features/Organizations/OrganizationContracts.cs.TODO`
- CreateOrganizationRequest, UpdateOrganizationRequest
- OrganizationDto, OrganizationSummaryDto, OrganizationPublicOverviewDto

**Frontend Service**: `frontend/src/services/organizationService.js`
- listOrganizations(params), createOrganization(payload), getDefaultOrganization(), getOrganizationById(id), updateOrganization(id, payload), getPublicOverview(id)

**Frontend Adapter**: `frontend/src/adapters/organizationAdapter.js`
- toOrganizationViewModel(dto), toOrganizationSummaryViewModel(dto), toOrganizationPublicOverviewViewModel(dto)

**Frontend Pages/Components**:
- OrgOverviewPage.jsx, OrgCard.jsx, OrgSwitcher.jsx

**Permissions**: JWT (list/create), org.workspace.access (get), org.overview.write (update), Public (public-overview)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement organization CRUD, OrgName uniqueness check (service-level), TotalMembers cached count update

**Risks/Notes**: OrgName uniqueness is service-level check, not DB hard constraint

---

## Members Module

**Status**: CORE

**Domain Entities**:
- Member.cs (UserId, OrgId, DepartmentId, RoleId, JoinDate, Status, etc.)
- MemberStatus.cs enum (Active, Invited, Suspended, Left, Removed)
- MemberRole.cs enum (Member, Manager, VicePresident, President) - logic enum only

**Backend Feature Folder**: `backend/Org.Backend/Features/Members/`

**Backend Routes**:
- GET /api/organizations/{orgId}/members
- POST /api/organizations/{orgId}/members
- PUT /api/members/{id}/department
- DELETE /api/members/{id}

**Shared Contract File**: `backend/Org.Shared/Features/Members/MemberContracts.cs.TODO`
- AddMemberRequest, UpdateMemberDepartmentRequest
- MemberDto

**Frontend Service**: `frontend/src/services/memberService.js`
- getOrganizationMembers(orgId, params), addMember(orgId, payload), updateMemberDepartment(memberId, payload), removeMember(memberId)

**Frontend Adapter**: `frontend/src/adapters/memberAdapter.js`
- toMemberViewModel(dto), toMemberListViewModel(items)

**Frontend Pages/Components**:
- OrgMembersPage.jsx

**Permissions**: org.workspace.access (list), org.members.manage (add/update/delete)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement member CRUD, unique (UserId, OrgId) check, role assignment (belongs to roleService)

**Risks/Notes**: Role assignment belongs to RolesPermissions module, NOT Members module; RoleId is canonical, MemberRole is logic enum only

---

## RolesPermissions Module

**Status**: CORE

**Domain Entities**:
- Role.cs (OrgId, RoleName, Description, IsDefault, Level)
- Permission.cs (PermissionKey, DisplayName, ModuleGroup, Description)
- RolePermission.cs (RoleId, PermissionId) - join table, does NOT inherit BaseEntity

**Backend Feature Folder**: `backend/Org.Backend/Features/RolesPermissions/`

**Backend Routes**:
- GET /api/organizations/{orgId}/permissions/me
- GET /api/organizations/{orgId}/permissions
- GET /api/organizations/{orgId}/roles
- POST /api/organizations/{orgId}/roles
- PUT /api/organizations/roles/{roleId}
- DELETE /api/organizations/roles/{roleId}
- POST /api/organizations/{orgId}/members/{memberId}/role

**Shared Contract File**: `backend/Org.Shared/Features/RolesPermissions/RoleContracts.cs.TODO`
- CreateRoleRequest, UpdateRoleRequest, AssignRoleToMemberRequest
- PermissionDto, MyPermissionsResponse, RoleDto

**Frontend Service**: `frontend/src/services/roleService.js`
- getMyPermissions(orgId), normalizePermissionKeys(response), getOrganizationPermissions(orgId), getOrganizationRoles(orgId, params), createRole(orgId, payload), updateRole(roleId, payload), deleteRole(roleId), assignRoleToMember(orgId, memberId, payload)

**Frontend Adapter**: `frontend/src/adapters/roleAdapter.js`
- toPermissionViewModel(dto), toRoleViewModel(dto), toRoleListViewModel(items)

**Frontend Pages/Components**:
- OrgRolesPage.jsx

**Permissions**: JWT (permissions/me), org.roles.view (list), org.roles.create/update/delete/assign (CRUD/assign)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement role CRUD, permission seeding, normalizePermissionKeys safe fallback, assignRoleToMember

**Risks/Notes**: normalizePermissionKeys must return [] on failure (never grant org.workspace.access); assignRoleToMember belongs to roleService

---

## Departments Module

**Status**: CORE

**Domain Entities**:
- Department.cs (OrgId, DeptName, Code, Function, ManagerId, Status)
- DepartmentStatus.cs enum (Active, Inactive, Archived)

**Backend Feature Folder**: `backend/Org.Backend/Features/Departments/`

**Backend Routes**:
- GET /api/organizations/{orgId}/departments
- POST /api/organizations/{orgId}/departments
- GET /api/departments/{id}
- PUT /api/departments/{id}
- DELETE /api/departments/{id}

**Shared Contract File**: `backend/Org.Shared/Features/Departments/DepartmentContracts.cs.TODO`
- CreateDepartmentRequest, UpdateDepartmentRequest
- DepartmentDto

**Frontend Service**: `frontend/src/services/departmentService.js`
- getOrganizationDepartments(orgId, params), createDepartment(orgId, payload), getDepartmentById(id), updateDepartment(id, payload), deleteDepartment(id)

**Frontend Adapter**: `frontend/src/adapters/departmentAdapter.js`
- toDepartmentViewModel(dto), toDepartmentListViewModel(items)

**Frontend Pages/Components**:
- OrgDepartmentsPage.jsx

**Permissions**: org.workspace.access (list/get), org.departments.manage (create/update/delete)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement department CRUD, Code uniqueness check (service-level), ManagerId points to Member

**Risks/Notes**: Department.Code uniqueness is service-level check; ManagerId must point to Member, not User

---

## Events Module

**Status**: CORE

**Domain Entities**:
- Event.cs (OrgId, EventName, Description, StartDate, EndDate, Budget, Location, TargetParticipants, Tags, Status, Visibility, AverageRating, CreatedByMemberId)
- EventStatus.cs enum (Draft, Published, Ongoing, Completed, Cancelled, Archived)
- EventVisibility.cs enum (Public, OrganizationOnly, Private)

**Backend Feature Folder**: `backend/Org.Backend/Features/Events/`

**Backend Routes**:
- GET /api/organizations/{orgId}/events
- POST /api/organizations/{orgId}/events
- GET /api/events/{id}
- PUT /api/events/{id}
- DELETE /api/events/{id}
- GET /api/events/public
- GET /api/events/{id}/public

**Shared Contract File**: `backend/Org.Shared/Features/Events/EventContracts.cs.TODO`
- CreateEventRequest, UpdateEventRequest
- EventDto, EventSummaryDto, EventPublicDto

**Frontend Service**: `frontend/src/services/eventService.js`
- getOrganizationEvents(orgId, params), createEvent(orgId, payload), getEventById(id), updateEvent(id, payload), deleteEvent(id), getPublicEvents(params), getPublicEventById(id)

**Frontend Adapter**: `frontend/src/adapters/eventAdapter.js`
- toEventViewModel(dto), toEventSummaryViewModel(dto), toEventPublicViewModel(dto), toEventListViewModel(items)

**Frontend Pages/Components**:
- OrgEventsPage.jsx, OrgEventDetailPage.jsx, PublicEventsPage.jsx, PublicEventDetailPage.jsx, EventCard.jsx, EventStatusBadge.jsx

**Permissions**: org.workspace.access (list/get), org.events.create (create), org.events.manage (update/delete), Public (public)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement event CRUD, visibility control, AverageRating cached count update

**Risks/Notes**: Do not fake TargetParticipants/Budget/AverageRating if missing; AverageRating is cached value

---

## Milestones Module

**Status**: CORE

**Domain Entities**:
- Milestone.cs (EventId, Title, Description, OrderIndex, StartDate, EndDate, Status)
- MilestoneStatus.cs enum (Planned, InProgress, Completed, Archived)

**Backend Feature Folder**: `backend/Org.Backend/Features/Milestones/`

**Backend Routes**:
- GET /api/events/{eventId}/milestones
- POST /api/events/{eventId}/milestones
- GET /api/milestones/{id}
- PUT /api/milestones/{id}
- DELETE /api/milestones/{id}

**Shared Contract File**: `backend/Org.Shared/Features/Milestones/MilestoneContracts.cs.TODO`
- CreateMilestoneRequest, UpdateMilestoneRequest
- MilestoneDto

**Frontend Service**: `frontend/src/services/milestoneService.js`
- getEventMilestones(eventId), createMilestone(eventId, payload), getMilestoneById(id), updateMilestone(id, payload), deleteMilestone(id)

**Frontend Adapter**: `frontend/src/adapters/milestoneAdapter.js`
- toMilestoneViewModel(dto), toMilestoneListViewModel(items)

**Frontend Pages/Components**:
- OrgEventDetailPage.jsx (root), MilestonePanel.jsx, MilestoneFormModal.jsx

**Permissions**: org.workspace.access (list/get), org.events.manage (create/update/delete)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement milestone CRUD, OrderIndex management for timeline

**Risks/Notes**: OrderIndex should be maintained for stable timeline rendering

---

## EventCategories Module

**Status**: CORE

**Domain Entities**:
- EventCategory.cs (MilestoneId, CategoryName, Description, OrderIndex, OwnerDepartmentId)

**Backend Feature Folder**: `backend/Org.Backend/Features/EventCategories/`

**Backend Routes**:
- GET /api/milestones/{milestoneId}/categories
- POST /api/milestones/{milestoneId}/categories
- GET /api/categories/{id}
- PUT /api/categories/{id}
- DELETE /api/categories/{id}

**Shared Contract File**: `backend/Org.Shared/Features/EventCategories/CategoryContracts.cs.TODO`
- CreateEventCategoryRequest, UpdateEventCategoryRequest
- EventCategoryDto (may include optional tasks[] array)

**Frontend Service**: `frontend/src/services/categoryService.js`
- getMilestoneCategories(milestoneId), createCategory(milestoneId, payload), getCategoryById(id), updateCategory(id, payload), deleteCategory(id)

**Frontend Adapter**: `frontend/src/adapters/categoryAdapter.js`
- toCategoryViewModel(dto), toCategoryListViewModel(items)

**Frontend Pages/Components**:
- OrgEventDetailPage.jsx (root), CategoryPanel.jsx, CategoryFormModal.jsx

**Permissions**: org.workspace.access (list/get), org.events.manage (create/update/delete)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement category CRUD, optional tasks[] array in CategoryDto

**Risks/Notes**: CategoryDto may include optional tasks[] array; if absent, frontend initializes tasks: []; do NOT invent separate list-by-category task endpoint

---

## Tasks Module

**Status**: CORE (inside EventDetail tree)

**Domain Entities**:
- OrgTask.cs (EventCategoryId, TaskName, Description, AssigneeId, DeptId, Priority, Deadline, Status, Note, CreatedByMemberId, CompletedAt)
- TaskStatus.cs enum (Todo, InProgress, Blocked, Done, Cancelled)
- TaskPriority.cs enum (Low, Medium, High, Urgent)

**Backend Feature Folder**: `backend/Org.Backend/Features/Tasks/`

**Backend Routes**:
- POST /api/categories/{categoryId}/tasks
- GET /api/tasks/{taskId}
- PUT /api/tasks/{taskId}
- DELETE /api/tasks/{taskId}
- PUT /api/tasks/{taskId}/status
- PUT /api/tasks/{taskId}/assign

**Shared Contract File**: `backend/Org.Shared/Features/Tasks/TaskContracts.cs.TODO`
- CreateTaskRequest, UpdateTaskRequest, UpdateTaskStatusRequest, AssignTaskRequest
- TaskDto

**Frontend Service**: `frontend/src/services/taskService.js`
- createTask(categoryId, payload), getTaskById(taskId), updateTask(taskId, payload), deleteTask(taskId), updateTaskStatus(taskId, payload), assignTask(taskId, payload)

**Frontend Adapter**: `frontend/src/adapters/taskAdapter.js`
- toTaskViewModel(dto), toTaskListViewModel(items)

**Frontend Pages/Components**:
- OrgEventDetailPage.jsx (root), TaskCard.jsx, TaskStatusControl.jsx, TaskAssignControl.jsx, TaskFormModal.jsx
- OrgTasksPlaceholderPage.jsx (aggregate board placeholder)

**Permissions**: org.workspace.access (get), org.events.manage (create/update/delete/status/assign)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement task CRUD inside EventDetail tree, create task response returns TaskDto for local append

**Risks/Notes**: Task is CORE inside EventDetail tree; only /org/tasks aggregate board is PROTOTYPE_ONLY; no getOrgTasks() or aggregate board endpoint; single assignee only in v1

---

## Requests Module

**Status**: CORE

**Domain Entities**:
- Request.cs (SenderId, OrgId, RequestType, Title, Content, DesiredDepartmentId, DesiredPosition, Status, ReviewNote, ReviewedByMemberId, ReviewedAt)
- RequestType.cs enum (JoinOrganization, DepartmentChange, RoleChange, EventParticipation, Other)
- RequestStatus.cs enum (Pending, Approved, Rejected, Cancelled, Closed)

**Backend Feature Folder**: `backend/Org.Backend/Features/Requests/`

**Backend Routes**:
- GET /api/organizations/{orgId}/requests
- POST /api/organizations/{orgId}/requests
- GET /api/requests/{requestId}
- POST /api/organizations/requests/{requestId}/review

**Shared Contract File**: `backend/Org.Shared/Features/Requests/RequestContracts.cs.TODO`
- CreateRequestRequest, ReviewRequestRequest
- RequestDto

**Frontend Service**: `frontend/src/services/requestService.js`
- getOrganizationRequests(orgId, params), createOrganizationRequest(orgId, payload), getRequestById(requestId), reviewRequest(requestId, payload)

**Frontend Adapter**: `frontend/src/adapters/requestAdapter.js`
- toRequestViewModel(dto), toRequestListViewModel(items)

**Frontend Pages/Components**:
- OrgRequestsPage.jsx

**Permissions**: org.requests.view (list/get), JWT (create), org.requests.review/approve (review)

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement request CRUD, join organization workflow, review/approve logic

**Risks/Notes**: Supports join organization workflow; other request types can be added when DTO/API confirmed

---

## Notifications Module

**Status**: CORE

**Domain Entities**:
- Notification.cs (ReceiverId, ActorId, Title, Message, Type, RelatedEntityType, RelatedEntityId, ActionUrl, IsRead, ReadAt)
- NotificationType.cs enum (System, RequestSubmitted, RequestReviewed, FriendRequest, EventCreated, EventUpdated, EventReminder, TaskAssigned, TaskDue, ResourceChanged)

**Backend Feature Folder**: `backend/Org.Backend/Features/Notifications/`

**Backend Routes**:
- GET /api/notifications
- GET /api/notifications/unread-count
- POST /api/notifications/{id}/read
- POST /api/notifications/read-all

**Shared Contract File**: `backend/Org.Shared/Features/Notifications/NotificationContracts.cs.TODO`
- NotificationDto, UnreadCountResponse, MarkNotificationReadResponse

**Frontend Service**: `frontend/src/services/notificationService.js`
- getNotifications(params), getUnreadCount(), markNotificationRead(id), markAllNotificationsRead()

**Frontend Adapter**: `frontend/src/adapters/notificationAdapter.js`
- toNotificationViewModel(dto), toNotificationListViewModel(items)

**Frontend Pages/Components**:
- OrgNotificationsPage.jsx, NotificationBadge.jsx

**Permissions**: JWT

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement notification CRUD, unread count, mark read logic; REST API first, SignalR optional future

**Risks/Notes**: RelatedEntityType/RelatedEntityId are polymorphic references (text + uuid), no FK constraint

---

## Friends Module

**Status**: SUPPORTING

**Domain Entities**:
- FriendRequest.cs (SenderId, ReceiverId, Status, RespondedAt)
- FriendRequestStatus.cs enum (Pending, Accepted, Rejected, Cancelled, Blocked)

**Backend Feature Folder**: `backend/Org.Backend/Features/Friends/`

**Backend Routes**:
- GET /api/friends
- GET /api/friends/requests
- POST /api/friends/requests
- POST /api/friends/requests/{id}/accept
- POST /api/friends/requests/{id}/reject

**Shared Contract File**: `backend/Org.Shared/Features/Friends/FriendContracts.cs.TODO`
- SendFriendRequestRequest
- FriendDto, FriendRequestDto

**Frontend Service**: `frontend/src/services/friendService.js`
- getFriends(params), getFriendRequests(params), sendFriendRequest(payload), acceptFriendRequest(id), rejectFriendRequest(id)

**Frontend Adapter**: `frontend/src/adapters/friendAdapter.js`
- toFriendViewModel(dto), toFriendRequestViewModel(dto), toFriendRequestListViewModel(items)

**Frontend Pages/Components**:
- UserFriendsPage.jsx

**Permissions**: JWT

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement friend request CRUD, accept/reject logic, SenderId != ReceiverId validation

**Risks/Notes**: SenderId != ReceiverId enforced at service level

---

## Discover Module

**Status**: SUPPORTING

**Domain Entities**:
- Organization.cs (reused)
- Event.cs (reused)

**Backend Feature Folder**: `backend/Org.Backend/Features/Discover/`

**Backend Routes**:
- GET /api/discover/organizations
- GET /api/discover/events

**Shared Contract File**: `backend/Org.Shared/Features/Discover/DiscoverContracts.cs.TODO`
- DiscoverOrganizationDto, DiscoverEventDto

**Frontend Service**: `frontend/src/services/discoverService.js`
- discoverOrganizations(params), discoverEvents(params)

**Frontend Adapter**: `frontend/src/adapters/discoverAdapter.js`
- toDiscoverOrganizationViewModel(dto), toDiscoverEventViewModel(dto)

**Frontend Pages/Components**:
- UserDiscoverPage.jsx

**Permissions**: JWT

**Current Implementation Status**: ✅ Skeleton complete with TODO notes

**Next Implementation Step**: Implement discover queries for public/visible organizations and events

**Risks/Notes**: No mock fallback; uses existing Organization and Event entities

---

## DB_FOUNDATION_ONLY Modules

### EventMembers, Attendees, DigitalAssets, EventRatings, EventReports, Resources, ActivityHistory

**Status**: DB_FOUNDATION_ONLY

**Domain Entities**: ✅ Exist in database (Phase 3B.2 completed)

**Backend Feature Folder**: README.md notes only

**Backend Routes**: ❌ None in base prototype

**Shared Contract File**: README.md notes only

**Frontend Service**: ❌ None

**Frontend Adapter**: ❌ None

**Frontend Pages/Components**: ❌ None (or placeholder only for Reports/Resources)

**Permissions**: N/A

**Current Implementation Status**: 📝 DB foundation only, no working UI/API

**Next Implementation Step**: Wait for explicit user request before implementing

**Risks/Notes**: Domain exists to preserve concepts; no working UI/API in base prototype

---

## EXCLUDED Modules

### Posts, Comments, Messages/Chat, Finance

**Status**: EXCLUDED

**Domain Entities**: ❌ None

**Backend Feature Folder**: ❌ None

**Backend Routes**: ❌ None

**Shared Contract File**: ❌ None

**Frontend Service**: ❌ None

**Frontend Adapter**: ❌ None

**Frontend Pages/Components**: ❌ None (or placeholder only for Finance)

**Permissions**: N/A

**Current Implementation Status**: ❌ Excluded from rescue v1

**Next Implementation Step**: Confirm scope change before implementing

**Risks/Notes**: Hard-excluded from rescue v1; do not implement without explicit user confirmation

---

## Summary

### CORE Modules (12)
All have complete cross-layer traceability: Domain → Backend → Contract → Service → Adapter → Page → Permission

### SUPPORTING Modules (2)
All have complete cross-layer traceability: Domain → Backend → Contract → Service → Adapter → Page → Permission

### DB_FOUNDATION_ONLY Modules (7)
Domain exists, no working UI/API in base prototype

### PROTOTYPE_ONLY Pages (4)
Placeholder pages only, no working service/adapter/API

### EXCLUDED Modules (4)
No domain, no backend, no frontend

---

**End of CROSS_LAYER_TRACEABILITY.md**
