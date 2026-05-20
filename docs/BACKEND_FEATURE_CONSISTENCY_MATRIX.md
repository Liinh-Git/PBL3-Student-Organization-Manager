# BACKEND_FEATURE_CONSISTENCY_MATRIX

## Purpose
This file is the backend-side consistency checkpoint for later Shared Contract and Frontend skeleton tasks. It ensures all modules are properly mapped across all layers.

## CORE Modules

| Module | Status | Domain Entities | Expected Backend Routes | Required Permissions | Future Shared Contracts | Future Frontend Service | Future Adapter | Future Page/Component | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Auth | CORE | User, UserStatus | POST /api/auth/login, POST /api/auth/register, GET /api/auth/me | Public (login/register), JWT (me) | AuthContracts.cs.TODO | authService.js | userAdapter.js | LoginPage.jsx, RegisterPage.jsx | JWT implementation deferred |
| Users | CORE | User, Member, Organization, Event, UserStatus, ProfileVisibility | GET /api/users/me, PUT /api/users/me, PUT /api/users/me/change-password, GET /api/users/me/organizations, GET /api/users/me/events, GET /api/users/me/discover/organizations | JWT token | UserContracts.cs.TODO | userService.js | userAdapter.js | UserProfilePage.jsx, UserSettingsPage.jsx, UserOrganizationsPage.jsx, UserEventsPage.jsx | getMyOrganizations belongs to userService |
| Organizations | CORE | Organization, Member, User, OrgStatus | GET /api/organizations, POST /api/organizations, GET /api/organizations/default, GET /api/organizations/{id}, PUT /api/organizations/{id}, GET /api/organizations/{id}/public-overview | org.overview.read, org.overview.write, org.workspace.access | OrganizationContracts.cs.TODO | organizationService.js | organizationAdapter.js | OrgOverviewPage.jsx, OrgCard.jsx, OrgSwitcher.jsx | OrgName uniqueness is service-level |
| Members | CORE | Member, User, Organization, Department, Role, MemberStatus | GET /api/organizations/{orgId}/members, POST /api/organizations/{orgId}/members, PUT /api/members/{id}/department, DELETE /api/members/{id} | org.members.view, org.members.manage, org.workspace.access | MemberContracts.cs.TODO | memberService.js | memberAdapter.js | OrgMembersPage.jsx | Role assignment belongs to RolesPermissions module |
| Departments | CORE | Department, Organization, Member, DepartmentStatus | GET /api/organizations/{orgId}/departments, POST /api/organizations/{orgId}/departments, GET /api/departments/{id}, PUT /api/departments/{id}, DELETE /api/departments/{id} | org.departments.view, org.departments.manage | DepartmentContracts.cs.TODO | departmentService.js | departmentAdapter.js | OrgDepartmentsPage.jsx | ManagerId points to Member |
| Events | CORE | Event, Organization, Member, Milestone, EventCategory, OrgTask, EventStatus, EventVisibility | GET /api/organizations/{orgId}/events, POST /api/organizations/{orgId}/events, GET /api/events/{id}, PUT /api/events/{id}, DELETE /api/events/{id}, GET /api/events/public, GET /api/events/{id}/public | org.events.view, org.events.create, org.events.manage | EventContracts.cs.TODO | eventService.js | eventAdapter.js | OrgEventsPage.jsx, OrgEventDetailPage.jsx, EventCard.jsx | EventDto includes Location, TargetParticipants, Budget, AverageRating, Tags |
| Milestones | CORE | Milestone, Event, EventCategory, MilestoneStatus | GET /api/events/{eventId}/milestones, POST /api/events/{eventId}/milestones, GET /api/milestones/{id}, PUT /api/milestones/{id}, DELETE /api/milestones/{id} | org.events.view, org.events.manage | MilestoneContracts.cs.TODO | milestoneService.js | milestoneAdapter.js | MilestonePanel.jsx (inside EventDetail) | OrderIndex maintained for timeline |
| EventCategories | CORE | EventCategory, Milestone, Department, OrgTask | GET /api/milestones/{milestoneId}/categories, POST /api/milestones/{milestoneId}/categories, GET /api/categories/{id}, PUT /api/categories/{id}, DELETE /api/categories/{id} | org.events.view, org.events.manage | CategoryContracts.cs.TODO | categoryService.js | categoryAdapter.js | CategoryPanel.jsx (inside EventDetail) | CategoryDto may include tasks[] array |
| Tasks | CORE | OrgTask, EventCategory, Member, Department, TaskStatus, TaskPriority | POST /api/categories/{categoryId}/tasks, GET /api/tasks/{taskId}, PUT /api/tasks/{taskId}, DELETE /api/tasks/{taskId}, PUT /api/tasks/{taskId}/status, PUT /api/tasks/{taskId}/assign | org.events.view, org.events.manage | TaskContracts.cs.TODO | taskService.js | taskAdapter.js | TaskCard.jsx (inside EventDetail) | Task is CORE inside EventDetail, only /org/tasks board is PROTOTYPE_ONLY |
| Requests | CORE | Request, User, Organization, Department, Member, RequestType, RequestStatus | GET /api/organizations/{orgId}/requests, POST /api/organizations/{orgId}/requests, GET /api/requests/{requestId}, POST /api/organizations/requests/{requestId}/review | org.requests.view, org.requests.review, org.requests.approve | RequestContracts.cs.TODO | requestService.js | requestAdapter.js | OrgRequestsPage.jsx | Supports join organization workflow |
| Notifications | CORE | Notification, User, NotificationType | GET /api/notifications, GET /api/notifications/unread-count, POST /api/notifications/{id}/read, POST /api/notifications/read-all | JWT token | NotificationContracts.cs.TODO | notificationService.js | notificationAdapter.js | NotificationBadge.jsx | REST only, SignalR optional future |
| RolesPermissions | CORE | Role, Permission, RolePermission, Member, Organization, MemberRole | GET /api/organizations/{orgId}/permissions/me, GET /api/organizations/{orgId}/permissions, GET /api/organizations/{orgId}/roles, POST /api/organizations/{orgId}/roles, PUT /api/organizations/roles/{roleId}, DELETE /api/organizations/roles/{roleId}, POST /api/organizations/{orgId}/members/{memberId}/role | org.roles.view, org.roles.create, org.roles.update, org.roles.delete, org.roles.assign | RoleContracts.cs.TODO | roleService.js | roleAdapter.js | OrgRolesPage.jsx | RoleId is canonical, permissions/me must normalize to string[] |

## SUPPORTING Modules

| Module | Status | Domain Entities | Expected Backend Routes | Required Permissions | Future Shared Contracts | Future Frontend Service | Future Adapter | Future Page/Component | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Friends | SUPPORTING | FriendRequest, User, FriendRequestStatus | GET /api/friends, GET /api/friends/requests, POST /api/friends/requests, POST /api/friends/requests/{id}/accept, POST /api/friends/requests/{id}/reject | JWT token | FriendContracts.cs.TODO | friendService.js | friendAdapter.js | UserFriendsPage.jsx | SenderId != ReceiverId enforced at service level |
| Discover | SUPPORTING | Organization, Event, OrgStatus, EventStatus, EventVisibility | GET /api/discover/organizations, GET /api/discover/events | JWT token | DiscoverContracts.cs.TODO | discoverService.js | discoverAdapter.js | UserDiscoverPage.jsx | No mock fallback |

## DB_FOUNDATION_ONLY Modules

| Module | Status | Domain Entities | Expected Backend Routes | Required Permissions | Future Shared Contracts | Future Frontend Service | Future Adapter | Future Page/Component | Notes |
|---|---|---|---|---|---|---|---|---|---|
| EventMembers | DB_FOUNDATION_ONLY | EventMember, Event, Member, EventRole | None in base prototype | N/A | None in base prototype | None | None | None | Event staff/organizer, no working UI/API in base prototype |
| Attendees | DB_FOUNDATION_ONLY | Attendee, Event, User, AttendeeStatus | None in base prototype | N/A | None in base prototype | None | None | None | Event participant/registration, no working UI/API in base prototype |
| DigitalAssets | DB_FOUNDATION_ONLY | DigitalAsset, Event, User, FileType | None in base prototype | N/A | None in base prototype | None | None | None | Event file/asset, no upload API in base prototype |
| EventRatings | DB_FOUNDATION_ONLY | EventRating, Event, User, RatingAspect | None in base prototype | N/A | None in base prototype | None | None | None | Event rating, no working UI/API in base prototype |
| EventReports | DB_FOUNDATION_ONLY | EventReport, Event, Member | None in base prototype | N/A | None in base prototype | None | None | None | Event report, Reports page remains PROTOTYPE_ONLY |
| Resources | DB_FOUNDATION_ONLY | Resource, Organization, Event, ResourceStatus | None in base prototype | N/A | None in base prototype | None | None | None | Organization resource, Resources page remains PROTOTYPE_ONLY |
| ActivityHistory | DB_FOUNDATION_ONLY | ActivityHistory, Organization, ActivityType | None in base prototype | N/A | None in base prototype | None | None | None | Activity feed/log, no working UI/API in base prototype |

## EXCLUDED Modules (No Route, No Page, No Service)

| Module | Status | Reason |
|---|---|---|
| Posts | EXCLUDED | Hard-excluded from rescue v1 |
| Comments | EXCLUDED | Hard-excluded from rescue v1 |
| Messages/Chat | EXCLUDED | Placeholder page only, no working module |
| Finance | EXCLUDED | Placeholder page only, no working module |
| FinanceTransaction | EXCLUDED | Not in scope for base prototype |
| FinanceBudget | EXCLUDED | Not in scope for base prototype |

## PROTOTYPE_ONLY Pages (Placeholder Only)

| Page | Status | Notes |
|---|---|---|
| /org/tasks aggregate board | PROTOTYPE_ONLY | Placeholder page, no API calls, no fake board |
| Reports page | PROTOTYPE_ONLY | Placeholder page |
| Finance page | PROTOTYPE_ONLY | Placeholder page |
| Resources page | PROTOTYPE_ONLY | Placeholder page |
| Messages/Chat page | PROTOTYPE_ONLY | Placeholder page if visible in nav |

## Consistency Verification

### Matches PHASE_3C_REQUIREMENTS_SPEC.md
✅ All CORE modules (12) are present with full skeleton
✅ All SUPPORTING modules (2) are present with full skeleton
✅ All DB_FOUNDATION_ONLY modules (7) are present with README only
✅ All EXCLUDED modules are not created
✅ All PROTOTYPE_ONLY pages are documented

### Matches DOMAIN_ENTITY_LOCK_V1.md
✅ All domain entities are correctly mapped to modules
✅ All enums are correctly referenced
✅ All relationships are preserved

### No Invented Routes
✅ All routes match approved route list from PHASE_3C_REQUIREMENTS_SPEC.md
✅ No /org/tasks aggregate endpoint invented
✅ No list-by-category task endpoint invented

### No Excluded Modules Created
✅ Posts module not created
✅ Comments module not created
✅ Messages/Chat working module not created
✅ Finance working module not created

### Task Module Clarity
✅ Task module is CORE inside EventDetail tree
✅ Only /org/tasks aggregate board is PROTOTYPE_ONLY
✅ Clear distinction documented

### EventMember and Attendee Treatment
✅ EventMember is DB_FOUNDATION_ONLY
✅ Attendee is DB_FOUNDATION_ONLY
✅ No working UI/API in base prototype
✅ Database foundation preserved

### Resources/EventRatings/EventReports/ActivityHistory Treatment
✅ All are DB_FOUNDATION_ONLY or PROTOTYPE_ONLY
✅ Not working modules
✅ Database foundation preserved

## End of BACKEND_FEATURE_CONSISTENCY_MATRIX.md
