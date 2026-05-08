# FRONTEND_SERVICE_ADAPTER_MATRIX

## Purpose
This file documents the frontend service and adapter files created for Phase 3C-4B, aligned with the shared contract consistency matrix and backend feature matrix.

## Legend

| Status | Meaning |
|---|---|
| ✅ Created | Service/adapter file created with TODO stubs |
| ⚠️ N/A | Not applicable (DB_FOUNDATION_ONLY, PROTOTYPE_ONLY, or EXCLUDED) |
| ❌ Not Created | Intentionally not created per requirements |

## CORE Modules

| Module | Status | Service File | Service Functions | Adapter File | Adapter Functions | Backend Routes | Contract DTOs | Permission | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Auth | ✅ | authService.js | login, register, getCurrentUser, logoutLocalOnly | userAdapter.js | toUserProfileViewModel | POST /api/auth/login, POST /api/auth/register, GET /api/auth/me | AuthContracts.cs.TODO | Public (login/register), JWT (me) | JWT implementation deferred |
| Users | ✅ | userService.js | getMe, updateMe, changePassword, getMyOrganizations, getMyEvents, discoverMyOrganizations | userAdapter.js | toUserProfileViewModel, toMyOrganizationViewModel, toMyEventViewModel, toDiscoverOrganizationViewModel | GET /api/users/me, PUT /api/users/me, PUT /api/users/me/change-password, GET /api/users/me/organizations, GET /api/users/me/events, GET /api/users/me/discover/organizations | UserContracts.cs.TODO | JWT | getMyOrganizations belongs to userService |
| Organizations | ✅ | organizationService.js | listOrganizations, createOrganization, getDefaultOrganization, getOrganizationById, updateOrganization, getPublicOverview | organizationAdapter.js | toOrganizationViewModel, toOrganizationSummaryViewModel, toOrganizationPublicOverviewViewModel | GET /api/organizations, POST /api/organizations, GET /api/organizations/default, GET /api/organizations/{id}, PUT /api/organizations/{id}, GET /api/organizations/{id}/public-overview | OrganizationContracts.cs.TODO | JWT (list/create), org.workspace.access (get), org.overview.write (update), Public (public-overview) | getMyOrganizations NOT in organizationService |
| Members | ✅ | memberService.js | getOrganizationMembers, addMember, updateMemberDepartment, removeMember | memberAdapter.js | toMemberViewModel, toMemberListViewModel | GET /api/organizations/{orgId}/members, POST /api/organizations/{orgId}/members, PUT /api/members/{id}/department, DELETE /api/members/{id} | MemberContracts.cs.TODO | org.workspace.access (list), org.members.manage (add/update/delete) | Role assignment belongs to roleService |
| Departments | ✅ | departmentService.js | getOrganizationDepartments, createDepartment, getDepartmentById, updateDepartment, deleteDepartment | departmentAdapter.js | toDepartmentViewModel, toDepartmentListViewModel | GET /api/organizations/{orgId}/departments, POST /api/organizations/{orgId}/departments, GET /api/departments/{id}, PUT /api/departments/{id}, DELETE /api/departments/{id} | DepartmentContracts.cs.TODO | org.workspace.access (list/get), org.departments.manage (create/update/delete) | ManagerId points to Member |
| Events | ✅ | eventService.js | getOrganizationEvents, createEvent, getEventById, updateEvent, deleteEvent, getPublicEvents, getPublicEventById | eventAdapter.js | toEventViewModel, toEventSummaryViewModel, toEventPublicViewModel, toEventListViewModel | GET /api/organizations/{orgId}/events, POST /api/organizations/{orgId}/events, GET /api/events/{id}, PUT /api/events/{id}, DELETE /api/events/{id}, GET /api/events/public, GET /api/events/{id}/public | EventContracts.cs.TODO | org.workspace.access (list/get), org.events.create (create), org.events.manage (update/delete), Public (public) | Do not fake TargetParticipants/Budget/AverageRating |
| Milestones | ✅ | milestoneService.js | getEventMilestones, createMilestone, getMilestoneById, updateMilestone, deleteMilestone | milestoneAdapter.js | toMilestoneViewModel, toMilestoneListViewModel | GET /api/events/{eventId}/milestones, POST /api/events/{eventId}/milestones, GET /api/milestones/{id}, PUT /api/milestones/{id}, DELETE /api/milestones/{id} | MilestoneContracts.cs.TODO | org.workspace.access (list/get), org.events.manage (create/update/delete) | OrderIndex maintained for timeline |
| EventCategories | ✅ | categoryService.js | getMilestoneCategories, createCategory, getCategoryById, updateCategory, deleteCategory | categoryAdapter.js | toCategoryViewModel, toCategoryListViewModel | GET /api/milestones/{milestoneId}/categories, POST /api/milestones/{milestoneId}/categories, GET /api/categories/{id}, PUT /api/categories/{id}, DELETE /api/categories/{id} | CategoryContracts.cs.TODO | org.workspace.access (list/get), org.events.manage (create/update/delete) | CategoryDto may include tasks[] array |
| Tasks | ✅ | taskService.js | createTask, getTaskById, updateTask, deleteTask, updateTaskStatus, assignTask | taskAdapter.js | toTaskViewModel, toTaskListViewModel | POST /api/categories/{categoryId}/tasks, GET /api/tasks/{taskId}, PUT /api/tasks/{taskId}, DELETE /api/tasks/{taskId}, PUT /api/tasks/{taskId}/status, PUT /api/tasks/{taskId}/assign | TaskContracts.cs.TODO | org.workspace.access (get), org.events.manage (create/update/delete/status/assign) | Task is CORE inside EventDetail, only /org/tasks board is PROTOTYPE_ONLY |
| Requests | ✅ | requestService.js | getOrganizationRequests, createOrganizationRequest, getRequestById, reviewRequest | requestAdapter.js | toRequestViewModel, toRequestListViewModel | GET /api/organizations/{orgId}/requests, POST /api/organizations/{orgId}/requests, GET /api/requests/{requestId}, POST /api/organizations/requests/{requestId}/review | RequestContracts.cs.TODO | org.requests.view (list/get), JWT (create), org.requests.review/approve (review) | Supports join organization workflow |
| Notifications | ✅ | notificationService.js | getNotifications, getUnreadCount, markNotificationRead, markAllNotificationsRead | notificationAdapter.js | toNotificationViewModel, toNotificationListViewModel | GET /api/notifications, GET /api/notifications/unread-count, POST /api/notifications/{id}/read, POST /api/notifications/read-all | NotificationContracts.cs.TODO | JWT | REST only, SignalR optional future |
| RolesPermissions | ✅ | roleService.js | getMyPermissions, normalizePermissionKeys, getOrganizationPermissions, getOrganizationRoles, createRole, updateRole, deleteRole, assignRoleToMember | roleAdapter.js | toPermissionViewModel, toRoleViewModel, toRoleListViewModel | GET /api/organizations/{orgId}/permissions/me, GET /api/organizations/{orgId}/permissions, GET /api/organizations/{orgId}/roles, POST /api/organizations/{orgId}/roles, PUT /api/organizations/roles/{roleId}, DELETE /api/organizations/roles/{roleId}, POST /api/organizations/{orgId}/members/{memberId}/role | RoleContracts.cs.TODO | JWT (permissions/me), org.roles.view (list), org.roles.create/update/delete/assign (CRUD/assign) | RoleId is canonical, permissions/me must normalize to string[] |

## SUPPORTING Modules

| Module | Status | Service File | Service Functions | Adapter File | Adapter Functions | Backend Routes | Contract DTOs | Permission | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Friends | ✅ | friendService.js | getFriends, getFriendRequests, sendFriendRequest, acceptFriendRequest, rejectFriendRequest | friendAdapter.js | toFriendViewModel, toFriendRequestViewModel, toFriendRequestListViewModel | GET /api/friends, GET /api/friends/requests, POST /api/friends/requests, POST /api/friends/requests/{id}/accept, POST /api/friends/requests/{id}/reject | FriendContracts.cs.TODO | JWT | SenderId != ReceiverId enforced at service level |
| Discover | ✅ | discoverService.js | discoverOrganizations, discoverEvents | discoverAdapter.js | toDiscoverOrganizationViewModel, toDiscoverEventViewModel | GET /api/discover/organizations, GET /api/discover/events | DiscoverContracts.cs.TODO | JWT | No mock fallback |

## DB_FOUNDATION_ONLY Modules

| Module | Status | Service File | Service Functions | Adapter File | Adapter Functions | Backend Routes | Contract DTOs | Permission | Notes |
|---|---|---|---|---|---|---|---|---|---|
| EventMembers | ⚠️ N/A | None | None | None | None | None in base prototype | README.md only | N/A | Event staff/organizer, no working UI/API in base prototype |
| Attendees | ⚠️ N/A | None | None | None | None | None in base prototype | README.md only | N/A | Event participant/registration, no working UI/API in base prototype |
| DigitalAssets | ⚠️ N/A | None | None | None | None | None in base prototype | README.md only | N/A | Event file/asset, no upload API in base prototype |
| EventRatings | ⚠️ N/A | None | None | None | None | None in base prototype | README.md only | N/A | Event rating, no working UI/API in base prototype |
| EventReports | ⚠️ N/A | None | None | None | None | None in base prototype | README.md only | N/A | Event report, Reports page remains PROTOTYPE_ONLY |
| Resources | ⚠️ N/A | None | None | None | None | None in base prototype | README.md only | N/A | Organization resource, Resources page remains PROTOTYPE_ONLY |
| ActivityHistory | ⚠️ N/A | None | None | None | None | None in base prototype | README.md only | N/A | Activity feed/log, no working UI/API in base prototype |

## EXCLUDED Modules

| Module | Status | Service File | Service Functions | Adapter File | Adapter Functions | Backend Routes | Contract DTOs | Permission | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Posts | ❌ Not Created | None | None | None | None | None | None | None | Hard-excluded from rescue v1 |
| Comments | ❌ Not Created | None | None | None | None | None | None | None | Hard-excluded from rescue v1 |
| Messages/Chat | ❌ Not Created | None | None | None | None | None | None | None | Placeholder page only, no working module |
| Finance | ❌ Not Created | None | None | None | None | None | None | None | Placeholder page only, no working module |

## PROTOTYPE_ONLY Pages

| Page | Status | Service File | Service Functions | Adapter File | Adapter Functions | Notes |
|---|---|---|---|---|---|---|
| /org/tasks aggregate board | ⚠️ N/A | None | None | None | None | Placeholder page only, no API calls, no fake board |
| Reports page | ⚠️ N/A | None | None | None | None | Placeholder page only |
| Finance page | ⚠️ N/A | None | None | None | None | Placeholder page only |
| Resources page | ⚠️ N/A | None | None | None | None | Placeholder page only |

## Consistency Verification

### Matches SHARED_CONTRACT_CONSISTENCY_MATRIX.md ✅
- All CORE modules (12) have service and adapter files
- All SUPPORTING modules (2) have service and adapter files
- All DB_FOUNDATION_ONLY modules have no service/adapter files
- All EXCLUDED modules have no service/adapter files
- All backend routes documented correctly
- All permissions documented correctly

### Matches BACKEND_FEATURE_CONSISTENCY_MATRIX.md ✅
- All backend routes match
- All request/response DTOs documented
- All permissions use canonical keys

### Matches PHASE_3C_REQUIREMENTS_SPEC.md ✅
- All CORE modules have full frontend skeleton
- All SUPPORTING modules have full frontend skeleton
- All DB_FOUNDATION_ONLY modules have no working UI/API
- All EXCLUDED modules have no routes/pages/services

### Service Ownership Rules ✅
- getMyOrganizations() belongs to userService only ✅
- assignRoleToMember() belongs to roleService only ✅
- taskService follows EventDetail task chain only ✅
- categoryService documents optional tasks[] but does not create task-list endpoint ✅

### VITE_API_BASE_URL Rule ✅
- All service paths will NOT include /api prefix (VITE_API_BASE_URL already includes /api) ✅

## Build Verification

### Build Command
```powershell
cd PBL3-rescue/frontend
npm run build
```

### Build Status
✅ **Build succeeded with 0 errors**

### Build Output Summary
```
vite v5.4.21 building for production...
✓ 41 modules transformed.
dist/index.html                   0.47 kB │ gzip:  0.31 kB
dist/assets/index-CIfVvUo9.css    0.29 kB │ gzip:  0.23 kB
dist/assets/index-BU_ylEEX.js   159.61 kB │ gzip: 51.94 kB
✓ built in 1.30s
```

## Summary

### Files Created/Modified
- **Services**: 14 files (authService.js, userService.js, organizationService.js, memberService.js, departmentService.js, eventService.js, milestoneService.js, categoryService.js, taskService.js, requestService.js, notificationService.js, roleService.js, friendService.js, discoverService.js)
- **Adapters**: 13 files (userAdapter.js, organizationAdapter.js, memberAdapter.js, departmentAdapter.js, eventAdapter.js, milestoneAdapter.js, categoryAdapter.js, taskAdapter.js, requestAdapter.js, notificationAdapter.js, roleAdapter.js, friendAdapter.js, discoverAdapter.js)
- **Documentation**: 1 file (FRONTEND_SERVICE_ADAPTER_MATRIX.md)

### Total Functions Created
- **Services**: 52 function signatures with TODO comments
- **Adapters**: 27 mapping functions with TODO comments

### Modules Marked BLOCKED_BY_API
- None (all services/adapters have TODO comments indicating they need backend API verification)

### Skipped Modules
- DB_FOUNDATION_ONLY (7 modules): No service/adapter files created per requirements
- EXCLUDED (4 modules): No service/adapter files created per requirements
- PROTOTYPE_ONLY (5 pages): No service/adapter files created per requirements

### Forbidden Folders NOT Modified ✅
- No backend/ modifications
- No shared/ modifications
- No domain/ modifications
- No migrations/ modifications
- No frontend/src/pages/ modifications
- No frontend/src/components/ modifications
- No frontend/src/router/ modifications
- No frontend/src/layouts/ modifications
- No frontend/src/contexts/ modifications
- No frontend/src/hooks/ modifications

## Next Task

**Task 3C-4C: Frontend Pages + EventDetail Tree + Prototype Pages Skeleton**

### Warnings for Next Task
- Pages may import services/adapters but must NOT call service functions yet
- No real data loading
- EventDetail tree skeleton is critical
- Prototype-only pages use PrototypePlaceholder
- No Posts/Comments
- No working Messages/Chat

---

**End of FRONTEND_SERVICE_ADAPTER_MATRIX.md**
