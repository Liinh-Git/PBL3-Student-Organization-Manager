# FRONTEND_PAGE_COMPONENT_MATRIX

## Purpose
This file documents the frontend page and component files created for Phase 3C-4C, aligned with the service/adapter matrix and shared contract consistency matrix.

## Legend

| Status | Meaning |
|---|---|
| ✅ Created | Page/component file created with TODO stubs |
| ⚠️ Prototype Only | Placeholder page using PrototypePlaceholder component |
| ❌ Not Created | Intentionally not created per requirements |

## Public Pages

| Page | Status | Route | Uses Service Later | Uses Adapter Later | Permission | Notes |
|---|---|---|---|---|---|---|
| HomePage | ✅ | / | discoverService | discoverAdapter | Public | Landing page with featured events/orgs |
| PublicEventsPage | ✅ | /events | eventService.getPublicEvents | eventAdapter.toEventPublicViewModel | Public | Public events listing |
| PublicEventDetailPage | ✅ | /events/:eventId | eventService.getPublicEventById | eventAdapter.toEventPublicViewModel | Public | Public event detail view |

## Auth Pages

| Page | Status | Route | Uses Service Later | Uses Adapter Later | Permission | Notes |
|---|---|---|---|---|---|---|
| LoginPage | ✅ | /login | authService.login | userAdapter.toUserProfileViewModel | Public | User login form |
| RegisterPage | ✅ | /register | authService.register | None | Public | User registration form |

## User Workspace Pages

| Page | Status | Route | Uses Service Later | Uses Adapter Later | Permission | Notes |
|---|---|---|---|---|---|---|
| UserOrganizationsPage | ✅ | /user/organizations | userService.getMyOrganizations | userAdapter.toMyOrganizationViewModel | JWT | User's organizations list |
| UserEventsPage | ✅ | /user/events | userService.getMyEvents | userAdapter.toMyEventViewModel | JWT | User's events list |
| UserProfilePage | ✅ | /user/profile | userService.getMe | userAdapter.toUserProfileViewModel | JWT | User profile view |
| UserSettingsPage | ✅ | /user/settings | userService.getMe, userService.updateMe, userService.changePassword | userAdapter.toUserProfileViewModel | JWT | User settings and password change |
| UserFriendsPage | ✅ | /user/friends | friendService.getFriends, friendService.getFriendRequests, friendService.acceptFriendRequest, friendService.rejectFriendRequest | friendAdapter.toFriendViewModel, friendAdapter.toFriendRequestViewModel | JWT | Friends and friend requests |
| UserDiscoverPage | ✅ | /user/discover | discoverService.discoverOrganizations, discoverService.discoverEvents | discoverAdapter.toDiscoverOrganizationViewModel, discoverAdapter.toDiscoverEventViewModel | JWT | Discover organizations and events |

## Org Workspace Pages

| Page | Status | Route | Uses Service Later | Uses Adapter Later | Permission | Notes |
|---|---|---|---|---|---|---|
| OrgOverviewPage | ✅ | /org/overview?orgId= | organizationService.getOrganizationById | organizationAdapter.toOrganizationViewModel | org.workspace.access (read), org.overview.write (edit) | Organization overview and stats |
| OrgMembersPage | ✅ | /org/members?orgId= | memberService.getOrganizationMembers, memberService.addMember, memberService.updateMemberDepartment, memberService.removeMember, roleService.assignRoleToMember | memberAdapter.toMemberViewModel | org.workspace.access (read), org.members.manage (CRUD), org.roles.assign (assign role) | Members management |
| OrgDepartmentsPage | ✅ | /org/departments?orgId= | departmentService.getOrganizationDepartments, departmentService.createDepartment, departmentService.updateDepartment, departmentService.deleteDepartment | departmentAdapter.toDepartmentViewModel | org.workspace.access (read), org.departments.manage (CRUD) | Departments management |
| OrgEventsPage | ✅ | /org/events?orgId= | eventService.getOrganizationEvents, eventService.createEvent | eventAdapter.toEventSummaryViewModel | org.workspace.access (read), org.events.create (create) | Events listing |
| OrgEventDetailPage | ✅ | /org/events/:eventId?orgId= | eventService.getEventById, milestoneService.*, categoryService.*, taskService.* | eventAdapter.toEventViewModel, milestoneAdapter.*, categoryAdapter.*, taskAdapter.* | org.workspace.access (read), org.events.manage (CRUD) | EventDetail tree root |
| OrgRequestsPage | ✅ | /org/requests?orgId= | requestService.getOrganizationRequests, requestService.reviewRequest | requestAdapter.toRequestViewModel | org.requests.view (read), org.requests.review/approve (review) | Join requests management |
| OrgRolesPage | ✅ | /org/roles?orgId= | roleService.getOrganizationRoles, roleService.createRole, roleService.updateRole, roleService.deleteRole | roleAdapter.toRoleViewModel | org.roles.view (read), org.roles.create/update/delete (CRUD) | Roles and permissions management |
| OrgNotificationsPage | ✅ | /org/notifications?orgId= | notificationService.getNotifications, notificationService.markNotificationRead, notificationService.markAllNotificationsRead | notificationAdapter.toNotificationViewModel | JWT | Notifications list |

## Prototype-Only Pages

| Page | Status | Route | Uses Service Later | Uses Adapter Later | Permission | Notes |
|---|---|---|---|---|---|---|
| OrgTasksPlaceholderPage | ⚠️ Prototype Only | /org/tasks?orgId= | None | None | org.workspace.access | Aggregate task board placeholder; Task CRUD is CORE inside EventDetail tree |
| OrgResourcesPlaceholderPage | ⚠️ Prototype Only | /org/resources?orgId= | None | None | org.workspace.access | Resources entity exists in DB foundation but no working UI |
| OrgReportsPlaceholderPage | ⚠️ Prototype Only | /org/reports?orgId= | None | None | org.workspace.access | EventReports entity exists in DB foundation but no working UI |
| OrgFinancePlaceholderPage | ⚠️ Prototype Only | /org/finance?orgId= | None | None | org.workspace.access | Finance-specific module excluded from base prototype |

## EventDetail Tree Components

| Component | Status | Parent | Uses Service Later | Uses Adapter Later | Props | Notes |
|---|---|---|---|---|---|---|---|
| MilestonePanel | ✅ | OrgEventDetailPage | milestoneService.* | milestoneAdapter.* | milestone, categories, tasks, callbacks, canManage | Displays milestone and its categories |
| CategoryPanel | ✅ | MilestonePanel | categoryService.* | categoryAdapter.* | category, tasks, callbacks, canManage | Displays category and its tasks |
| TaskCard | ✅ | CategoryPanel | taskService.* | taskAdapter.* | task, callbacks, canManage | Displays single task |
| TaskStatusControl | ✅ | TaskCard | taskService.updateTaskStatus | None | task, onUpdateStatus, canManage | Task status dropdown |
| TaskAssignControl | ✅ | TaskCard | taskService.assignTask | None | task, onAssign, canManage | Task assignment controls |
| MilestoneFormModal | ✅ | OrgEventDetailPage | milestoneService.createMilestone, milestoneService.updateMilestone | None | isOpen, onClose, onSubmit, milestone | Create/edit milestone modal |
| CategoryFormModal | ✅ | OrgEventDetailPage | categoryService.createCategory, categoryService.updateCategory | None | isOpen, onClose, onSubmit, category | Create/edit category modal |
| TaskFormModal | ✅ | OrgEventDetailPage | taskService.createTask, taskService.updateTask | None | isOpen, onClose, onSubmit, task | Create/edit task modal |

## Supporting Components

| Component | Status | Used By | Props | Notes |
|---|---|---|---|---|
| EventCard | ✅ | PublicEventsPage, UserEventsPage, OrgEventsPage, UserDiscoverPage | event, onClick | Event summary card |
| EventStatusBadge | ✅ | EventCard | status | Event status badge (Draft, Published, InProgress, Completed, Cancelled) |
| OrgCard | ✅ | UserOrganizationsPage, UserDiscoverPage | organization, onClick | Organization summary card |
| OrgSwitcher | ✅ | TopBar (future) | currentOrgId, organizations, onSwitch | Organization switcher dropdown |
| NotificationBadge | ✅ | TopBar (future) | unreadCount, onClick | Notification badge with unread count |

## Excluded Pages (Not Created)

| Page | Status | Reason |
|---|---|---|
| Posts page | ❌ Not Created | Hard-excluded from rescue v1 |
| Comments page | ❌ Not Created | Hard-excluded from rescue v1 |
| Messages/Chat working page | ❌ Not Created | Placeholder only if needed in nav |
| Finance working module page | ❌ Not Created | Finance-specific module excluded; only placeholder page created |
| EventRating working page | ❌ Not Created | DB_FOUNDATION_ONLY, no working UI in base prototype |

## Consistency Verification

### Matches FRONTEND_SERVICE_ADAPTER_MATRIX.md ✅
- All CORE modules (12) have page skeletons
- All SUPPORTING modules (2) have page skeletons
- All service/adapter references documented correctly
- All permissions documented correctly

### Matches SHARED_CONTRACT_CONSISTENCY_MATRIX.md ✅
- All backend routes align with page requirements
- All request/response DTOs align with service/adapter usage
- All permissions use canonical keys

### Matches PHASE_3C_REQUIREMENTS_SPEC.md ✅
- All CORE modules have full page skeleton
- All SUPPORTING modules have full page skeleton
- All DB_FOUNDATION_ONLY modules have no working UI/page
- All EXCLUDED modules have no routes/pages
- All PROTOTYPE_ONLY pages use PrototypePlaceholder

### EventDetail Tree Rules ✅
- OrgEventDetailPage owns source-of-truth tree state
- MilestonePanel receives milestones/categories and callbacks
- CategoryPanel receives categories/tasks and callbacks
- TaskCard receives task and callbacks only
- TaskCard does NOT own source-of-truth state
- State management logic documented in TODO comments

### Route Rules ✅
- orgId from useSearchParams() for all /org/* routes
- useParams() ONLY for resource IDs in path (e.g., /events/:eventId)
- No global /forbidden redirect
- ForbiddenState rendered at page level

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
✓ 70 modules transformed.
dist/index.html                   0.47 kB │ gzip:  0.30 kB
dist/assets/index-CIfVvUo9.css    0.29 kB │ gzip:  0.23 kB
dist/assets/index-DdqgWZty.js   181.62 kB │ gzip: 56.70 kB
✓ built in 2.23s
```

## Summary

### Files Created/Modified
- **Public Pages**: 3 files (HomePage, PublicEventsPage, PublicEventDetailPage)
- **Auth Pages**: 2 files (LoginPage, RegisterPage)
- **User Pages**: 6 files (UserOrganizationsPage, UserEventsPage, UserProfilePage, UserSettingsPage, UserFriendsPage, UserDiscoverPage)
- **Org Pages**: 8 files (OrgOverviewPage, OrgMembersPage, OrgDepartmentsPage, OrgEventsPage, OrgEventDetailPage, OrgRequestsPage, OrgRolesPage, OrgNotificationsPage)
- **Prototype-Only Pages**: 4 files (OrgTasksPlaceholderPage, OrgResourcesPlaceholderPage, OrgReportsPlaceholderPage, OrgFinancePlaceholderPage)
- **EventDetail Components**: 8 files (MilestonePanel, CategoryPanel, TaskCard, TaskStatusControl, TaskAssignControl, MilestoneFormModal, CategoryFormModal, TaskFormModal)
- **Supporting Components**: 5 files (EventCard, EventStatusBadge, OrgCard, OrgSwitcher, NotificationBadge)
- **Router**: 1 file modified (AppRouter.jsx)

**Total Files Created/Modified: 37**

### Total Pages Created
- **Public**: 3 pages
- **Auth**: 2 pages
- **User Workspace**: 6 pages
- **Org Workspace**: 8 pages
- **Prototype-Only**: 4 pages

**Total: 23 pages**

### Total Components Created
- **EventDetail Tree**: 8 components
- **Supporting**: 5 components

**Total: 13 components**

### Modules Marked PROTOTYPE_ONLY
- /org/tasks aggregate board (Task CRUD is CORE inside EventDetail tree)
- Resources page (entity exists in DB foundation)
- Reports page (entity exists in DB foundation)
- Finance page (module excluded from base prototype)

### Skipped Pages
- Posts page (hard-excluded)
- Comments page (hard-excluded)
- Messages/Chat working page (placeholder only if needed)
- EventRating working page (DB_FOUNDATION_ONLY)
- EventMembers working page (DB_FOUNDATION_ONLY)
- Attendees working page (DB_FOUNDATION_ONLY)
- DigitalAssets working page (DB_FOUNDATION_ONLY)
- ActivityHistory working page (DB_FOUNDATION_ONLY)

### Forbidden Folders NOT Modified ✅
- No backend/ modifications
- No shared/ modifications
- No services/ modifications (completed in 3C-4B)
- No adapters/ modifications (completed in 3C-4B)

## Next Task

**Task 3C-5: Cross-layer Docs + Full Build Verification**

### Warnings for Next Task
- Verify route/page/service/adapter/contract/backend mapping
- Verify no fake data/API calls in pages/components
- Verify prototype-only boundaries
- Verify EventDetail tree state management documentation
- Create cross-layer traceability documentation
- Run full build verification (backend + frontend)

---

**End of FRONTEND_PAGE_COMPONENT_MATRIX.md**
