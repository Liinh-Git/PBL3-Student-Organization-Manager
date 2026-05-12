# UI Layout Context for v0 (PBL3 Frontend)

## 1. Project frontend stack
- Framework: React 18 + Vite 5
- Language: JavaScript (ESM), JSX
- Router: `react-router-dom` v6 (`BrowserRouter`, nested routes with guards)
- Styling system: Global CSS (`src/index.css`) + page-scoped CSS files (no Tailwind)
- Component libraries: None (custom components only)
- Icon libraries: None dedicated; inline SVG + emoji icon usage
- State/auth approach:
  - Auth: `AuthContext` + `authService`, JWT in `localStorage` (`org.auth.accessToken`, `org.auth.accessTokenExpiryUtc`)
  - Org workspace: `OrgContext` (`organization`, `permissions`, `isMember`, `loadWorkspaceOrg`)
  - Notifications: `useNotifications` hook (REST)

## 2. Current app shell/layout
- Root app structure:
  - `src/main.jsx` renders `<App />`
  - `src/App.jsx` wraps `<AppRouter />` with `<AuthProvider>` and `<OrgProvider>`
- Protected route structure:
  - `ProtectedRoute`: checks `isAuthenticated`; redirects to `/login` if false
  - `OrgMemberRoute`: requires `?orgId=` query param and membership (`isMember`)
- Layout component:
  - `AppLayout` used for authenticated user/org workspaces
  - Structure: fixed left rail + fixed sidebar + top bar + content outlet
- Sidebar:
  - `Sidebar` has workspace switch pattern:
    - user workspace button (current user initials)
    - org workspace buttons (one per organization from `getMyOrganizations()`)
  - Secondary nav switches between user links and org links
  - Org navigation links always preserve `?orgId=...`
- Topbar/header:
  - `TopBar` includes app title, notifications panel, user name, logout button
  - Notifications open dropdown panel with unread count and mark-read actions
- Organization context/switcher:
  - Implemented in sidebar rail with org initials buttons
  - Active org resolved from query param + selected workspace
- Where page content renders:
  - `<Outlet />` inside `AppLayout` -> `<main class="app-content">`
- Current responsive behavior:
  - Desktop-first fixed rail/sidebar widths (`64px + 220px`)
  - Media query at `max-width: 920px` partially adapts old `.sidebar/.topbar` classes
  - Some mismatch exists because current layout classes are `app-rail` + `app-sidebar`

## 3. Route map

| Route | Page component | Layout used | Auth required | Params | Status |
|---|---|---|---|---|---|
| `/` | `HomePage` | none/public direct | No | none | Skeleton |
| `/events` | `PublicEventsPage` | none/public direct | No | query optional (`search,page,pageSize`) | Skeleton |
| `/events/:eventId` | `PublicEventDetailPage` | none/public direct | No | `eventId` | Skeleton |
| `/login` | `LoginPage` | none/public direct | No | none | Implemented |
| `/register` | `RegisterPage` | none/public direct | No | none | Implemented |
| `/user/organizations` | `UserOrganizationsPage` | `AppLayout` + `ProtectedRoute` | Yes | none | Implemented |
| `/user/events` | `UserEventsPage` | `AppLayout` + `ProtectedRoute` | Yes | none | Implemented |
| `/user/profile` | `UserProfilePage` | `AppLayout` + `ProtectedRoute` | Yes | none | Implemented |
| `/user/settings` | `UserSettingsPage` | `AppLayout` + `ProtectedRoute` | Yes | none | Implemented |
| `/user/friends` | `UserFriendsPage` | `AppLayout` + `ProtectedRoute` | Yes | none | Skeleton |
| `/user/discover` | `UserDiscoverPage` | `AppLayout` + `ProtectedRoute` | Yes | none | Implemented |
| `/org/overview` | `OrgOverviewPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Implemented |
| `/org/members` | `OrgMembersPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Implemented |
| `/org/departments` | `OrgDepartmentsPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Implemented |
| `/org/events` | `OrgEventsPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Implemented |
| `/org/events/:eventId` | `OrgEventDetailPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | `eventId`, query `orgId` | Implemented |
| `/org/requests` | `OrgRequestsPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Implemented |
| `/org/roles` | `OrgRolesPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Implemented |
| `/org/tasks` | `OrgTasksPlaceholderPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Placeholder |
| `/org/resources` | `OrgResourcesPlaceholderPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Placeholder |
| `/org/reports` | `OrgReportsPlaceholderPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Placeholder |
| `/org/finance` | `OrgFinancePlaceholderPage` | `AppLayout` + `OrgMemberRoute` | Yes + member | query `orgId` | Placeholder |
| (not routed) `/org/notifications` | `OrgNotificationsPage` exists | N/A | N/A | would need query `orgId` | Unrouted skeleton |

## 4. Page inventory

### Login
- File: `frontend/src/pages/auth/LoginPage.jsx`
- Purpose: User sign-in
- Data displayed: Email/password form + API error
- APIs: `AuthContext.login` -> `authService.login`
- Primary actions: Submit login, link to register
- States: submitting + error; no dedicated loading/empty
- UI status: implemented

### Organizations
- File: `frontend/src/pages/user/UserOrganizationsPage.jsx`
- Purpose: User org list + create organization modal
- Data displayed: org cards, org counts, create form fields
- APIs: `getMyOrganizations`, `createOrganization`
- Primary actions: open org, create org
- States: loading/error; empty handled implicitly by grid
- UI status: implemented

### Organization overview
- File: `frontend/src/pages/org/OrgOverviewPage.jsx`
- Purpose: Org profile overview + edit org info (permission-gated)
- Data displayed: name, description, location, totalMembers, foundingDate, contactEmail/Phone, createdAt
- APIs: `OrgContext.loadWorkspaceOrg`, `updateOrganization`
- Primary actions: edit org profile
- States: missing orgId, loading, forbidden; no explicit empty state card
- UI status: implemented

### Members
- File: `frontend/src/pages/org/OrgMembersPage.jsx`
- Purpose: Manage members, departments assignment, add/remove member, self leave
- Data displayed: member identity, role, department, status, counts
- APIs: `getOrganizationMembers`, `addMember`, `updateMemberDepartment`, `removeMember`, `getOrganizationRoles`, `getOrganizationDepartments`
- Primary actions: add member, assign department, remove member, leave organization
- States: loading/error/forbidden/empty implemented
- UI status: implemented

### Departments
- File: `frontend/src/pages/org/OrgDepartmentsPage.jsx`
- Purpose: Manage department list and assignments
- Data displayed: department name/description/manager/member count/task count
- APIs: `getOrganizationDepartments`, `createDepartment`, `updateDepartment`, `deleteDepartment`, `getOrganizationMembers`, `updateMemberDepartment`
- Primary actions: create/edit/delete department, add member to department
- States: loading/error/forbidden/empty implemented
- UI status: implemented

### Events
- File: `frontend/src/pages/org/OrgEventsPage.jsx`
- Purpose: Org event CRUD and navigation to detail
- Data displayed: eventName, description, startDate/time, targetParticipants, status, visibility
- APIs: `getOrganizationEvents`, `createEvent`, `updateEvent`, `deleteEvent`
- Primary actions: create/edit/delete/view event
- States: loading/error/forbidden/empty implemented
- UI status: implemented

### Event detail
- File: `frontend/src/pages/org/OrgEventDetailPage.jsx`
- Purpose: EventDetail tree root (event -> milestones -> categories -> tasks)
- Data displayed: event info + nested milestone/category/task tables/forms
- APIs:
  - Event: `getEventById`, `updateEvent`
  - Milestone: `getEventMilestones`, `createMilestone`, `updateMilestone`, `deleteMilestone`
  - Category: `getMilestoneCategories`, `createCategory`, `updateCategory`, `deleteCategory`
  - Task: `createTask`, `updateTask`, `updateTaskStatus`, `assignTask`, `deleteTask`
  - Members: `getOrganizationMembers`
- Primary actions: full CRUD for milestone/category/task, assign task, status update
- States: loading/error/forbidden/empty implemented at multiple levels
- UI status: implemented

### Milestones
- File: inside `OrgEventDetailPage.jsx`
- Purpose: event milestone management
- Data displayed: title, description, order
- APIs: milestone service functions above
- Primary actions: create/edit/delete
- States: per-section empty + per-action loading
- UI status: implemented (inside event detail)

### Event categories
- File: inside `OrgEventDetailPage.jsx`
- Purpose: category management under milestone
- Data displayed: categoryName, description, tasks[]
- APIs: category service functions above
- Primary actions: create/edit/delete category
- States: empty per milestone
- UI status: implemented (inside event detail)

### Tasks
- File:
  - core: inside `OrgEventDetailPage.jsx`
  - aggregate placeholder: `frontend/src/pages/org/OrgTasksPlaceholderPage.jsx`
- Purpose:
  - core: task CRUD/status/assignee in category tree
  - aggregate board route: prototype placeholder only
- Data displayed: taskName, description, priority, status, assignee, deadline
- APIs: task service functions above
- Primary actions: create/edit/delete, assign, status change
- States: core has loading/empty/error patterns; aggregate is placeholder
- UI status: implemented in event detail, placeholder for `/org/tasks`

### Requests
- File: `frontend/src/pages/org/OrgRequestsPage.jsx`
- Purpose: create/review org requests
- Data displayed: sender, type, title/content, desired dept/position, status, created/review metadata
- APIs: `getOrganizationRequests`, `createOrganizationRequest`, `reviewRequest`, `getOrganizationMembers`
- Primary actions: create request, approve/reject request
- States: loading/error/forbidden/empty implemented
- UI status: implemented

### Notifications
- File(s):
  - active UI in shell: `frontend/src/layouts/TopBar.jsx`
  - optional page file: `frontend/src/pages/org/OrgNotificationsPage.jsx` (unrouted)
- Purpose: unread badge and recent notification panel
- Data displayed: unreadCount, title, message, createdAtUtc, isRead
- APIs: via `useNotifications`: `getNotifications`, `getUnreadCount`, `markNotificationRead`, `markAllNotificationsRead`
- Primary actions: open panel, mark one/all read
- States: loading + empty in panel; org notifications page is static empty state
- UI status: implemented in topbar panel, org page skeleton/unrouted

### Roles/permissions
- File: `frontend/src/pages/org/OrgRolesPage.jsx`
- Purpose: role CRUD + assign role to members
- Data displayed: roleName, description, permissionKeys[], member role assignment
- APIs: `getOrganizationRoles`, `createRole`, `updateRole`, `deleteRole`, `assignRoleToMember`, `getOrganizationMembers`
- Primary actions: create/edit/delete role, assign role
- States: loading/error/forbidden/empty implemented
- UI status: implemented

### Resources
- File: `frontend/src/pages/org/OrgResourcesPlaceholderPage.jsx`
- Purpose: placeholder only
- Data displayed: prototype placeholder text
- APIs: none
- Primary actions: none
- States: invalid orgId error + placeholder view
- UI status: placeholder

### Finance
- File: `frontend/src/pages/org/OrgFinancePlaceholderPage.jsx`
- Purpose: placeholder only
- Data displayed: prototype placeholder text
- APIs: none
- Primary actions: none
- States: invalid orgId error + placeholder view
- UI status: placeholder

### Reports
- File: `frontend/src/pages/org/OrgReportsPlaceholderPage.jsx`
- Purpose: placeholder only
- Data displayed: prototype placeholder text
- APIs: none
- Primary actions: none
- States: invalid orgId error + placeholder view
- UI status: placeholder

## 5. Data fields needed by UI

### Organization
- IDs/context: `id`, `orgId`
- Identity: `name`/`orgName`/`organizationName`
- Content: `description`, `avatarUrl`, `coverUrl`
- Metadata: `foundingDate`, `createdAt`/`createdAtUtc`, `status`, `isActive`
- Contact: `location`, `contactEmail`, `contactPhone`
- Counts: `totalMembers`

### Member
- IDs: `id`, `userId`, `organizationId`, `departmentId`, `roleId`
- User info: `user.fullName`, `user.email`, fallback `fullName`, `email`
- Role: `role.roleName`, `roleName`
- Department: `department.departmentName`/`deptName`
- Other: `studentCode`, `status`

### Department
- IDs: `id`, `organizationId`, `managerId`
- Core: `departmentName`/`deptName`, `description`, `status`
- Manager info: `manager.user.fullName`, `manager.fullName`, `managerName`
- Derived display: member count, task count (`taskCount`/`tasksCount` or `tasks.length`)

### Event
- IDs: `id`/`eventId`, `organizationId`
- Core: `name`/`eventName`, `description`, `startDate`, `endDate`, `location`, `bannerUrl`
- Status/visibility: `status`, `visibility`
- Planning: `targetParticipants`, `budget`, `averageRating`, `tags`
- Optional relation display: `organizationName`, `orgName`, `participationRole`

### Milestone
- IDs: `id`, `eventId`
- Core: `title`, `description`, `orderIndex`
- Optional timeline/status: `startDate`, `endDate`, `status`

### EventCategory
- IDs: `id`, `milestoneId`, `ownerDepartmentId`
- Core: `categoryName`, `description`, `orderIndex`
- Nested: `tasks[]` (must exist or be initialized)

### Task
- IDs: `id`, `categoryId`
- Core: `taskName`, `description`, `priority`, `status`, `deadline`, `note`
- Assignment: `assigneeId`/`assignedMemberId`, `assignee.user.fullName`, `assigneeName`, `deptId`

### Request
- IDs: `id`, `senderId`, `organizationId`
- Sender: `senderName`, `senderEmail`
- Core: `requestType`, `title`, `content`, `status`
- Desired change: `desiredDepartmentId`, `desiredDepartmentName`, `desiredPosition`
- Review: `reviewNote`, `reviewedByMemberId`, `reviewedByMemberName`, `reviewedAt`
- Time: `createdAtUtc`, `updatedAtUtc`

### Notification
- IDs: `id`, `receiverId`, `actorId`
- Content: `title`, `message`, `type`
- Linking: `relatedEntityType`, `relatedEntityId`, `actionUrl`
- Read state: `isRead`, `readAt`
- Time: `createdAtUtc`
- Aggregates: unread count (`count`)

### Role/Permission
- Role: `id`, `roleName`, `description`, `permissionKeys[]`
- Permission context (me): `permissionKeys[]`, `roleId`, `roleName`, `memberId`, `organizationId`

### Resource/Finance/Report placeholders
- No active data contracts in frontend pages
- Placeholder hints only:
  - Resource module expected for equipment/facilities management
  - Finance module excluded in prototype (event budget field exists in Event)
  - Reports module exists as concept/entity but no working page/API flow

## 6. Current visual style
- Colors:
  - Global theme uses blue-gray + orange accent (`--accent-500: #ff9b51`)
  - Sidebar dark (`#0f1f29` / `#25343f`), light content surfaces
- Typography:
  - Inter imported globally; auth page overrides to `system-ui`
  - Strong weight usage on headings
- Spacing:
  - Consistent page padding via `.app-page` (`2rem`), grid sections/cells
- Card/table/form styles:
  - Reusable `.app-card`, basic bordered tables, soft shadows
  - Form controls unified with `.form-input/.form-select/.form-textarea`
- Button styles:
  - Shared variants `.app-button--primary/secondary/ghost/danger`
- Weaknesses of current UI:
  - Style inconsistency: some pages use custom CSS systems, others use shared app primitives
  - Heavy inline styles in `TopBar` and some pages
  - Responsive rules partly target outdated class names
  - Mixed language labels and occasional text encoding artifacts in rendered strings
  - Some components exist but are bypassed by monolithic page JSX (especially event detail)
- Inconsistencies to fix:
  - unify spacing/typography/button language across org/user pages
  - align modal/table patterns
  - harmonize status badges and form layout behavior
  - ensure all pages have explicit loading/empty/error patterns

## 7. Redesign target for v0
- Modern SaaS dashboard for student organization management
- Light theme
- Primary palette: indigo/violet
- Accent palette: emerald/cyan
- Rounded cards
- Soft shadows
- Clear, readable tables
- Consistent empty/loading/error states
- Desktop-first with tablet-friendly behavior

## 8. Hard constraints for v0
- Do not invent routes.
- Do not invent API endpoints.
- Do not include post/comment/social feed modules.
- Do not design chat/messages unless present and required.
- Use placeholder data only for visual preview.
- Keep route names and page responsibilities unchanged.
- Generate UI components that integrate into React + Vite.
- Prefer reusable components over page-specific one-offs.
- Every page must have loading, empty, error, and data states.
- Unknown or backend-blocked modules must be polished placeholders.
- Preserve query-param org context pattern: org workspace routes require `?orgId=`.

## 9. Recommended v0 prompt
Redesign the frontend UI for an existing React + Vite student organization management app. Keep ALL existing routes and page responsibilities exactly as-is. Do not invent routes, APIs, or new modules.

Tech/layout context:
- React 18, Vite, JavaScript, react-router-dom v6.
- Authenticated pages use a shell layout: left rail workspace switcher, secondary sidebar nav, topbar, main outlet.
- Organization workspace routes are query-param based (`?orgId=`), and event detail uses path `:eventId` + query `orgId`.

Must-cover pages/modules:
- Auth: `/login`, `/register`
- User workspace: `/user/organizations`, `/user/events`, `/user/profile`, `/user/settings`, `/user/discover`, `/user/friends` (skeleton today)
- Org workspace: `/org/overview`, `/org/members`, `/org/departments`, `/org/events`, `/org/events/:eventId`, `/org/requests`, `/org/roles`
- Placeholder modules: `/org/tasks`, `/org/resources`, `/org/reports`, `/org/finance`
- Public skeletons: `/`, `/events`, `/events/:eventId`
- Notifications are primarily in topbar panel (badge + dropdown list)

Data/UI entities to support visually (no backend invention):
- Organization, Member, Department, Event, Milestone, EventCategory, Task, Request, Notification, Role/Permission.
- Tasks are core inside Event Detail tree (Event -> Milestone -> Category -> Task). `/org/tasks` is placeholder aggregate board.

Visual direction:
- Modern SaaS dashboard, light theme.
- Primary indigo/violet, accents emerald/cyan.
- Rounded cards, soft shadows, clear table hierarchy.
- Clean forms, filters, and action rows.
- Strong reusable component system.
- Desktop-first, tablet-friendly responsive behavior.

Hard constraints:
- Exclude posts/comments/social feed completely.
- No chat/messages unless core-required (not required here).
- Keep existing route names and responsibilities.
- Add explicit loading, empty, error, and data states to every page.
- For backend-blocked pages (`/org/resources`, `/org/reports`, `/org/finance`, `/org/tasks`, skeleton pages), create polished placeholder UIs only.
- Output UI architecture and components suitable for direct integration into React + Vite codebase.
