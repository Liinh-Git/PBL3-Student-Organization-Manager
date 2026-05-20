# MODULE_FILE_MANIFEST

## Purpose
Complete file inventory per module across all layers (Domain, Backend, Shared Contracts, Frontend Services, Frontend Adapters, Frontend Pages, Frontend Components).

## Legend
- ✅ = File exists
- ⚠️ = Placeholder/prototype only
- ❌ = Intentionally not created
- 📝 = README/notes only

---

## CORE Modules (12 modules)

### 1. Auth Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | User.cs, UserStatus.cs | ✅ | User entity + status enum |
| **Backend Feature** | Auth/README.md, Auth/Endpoints/README.md, Auth/Services/README.md, Auth/Validators/README.md, Auth/Mappings/README.md, Auth/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Auth/README.md, Auth/AuthContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | authService.js | ✅ | login, register, getCurrentUser, logoutLocalOnly |
| **Frontend Adapter** | userAdapter.js | ✅ | toUserProfileViewModel (shared with Users module) |
| **Frontend Pages** | LoginPage.jsx, RegisterPage.jsx | ✅ | Auth pages |
| **Frontend Components** | None | ❌ | Uses shared components only |

### 2. Users Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | User.cs, ProfileVisibility.cs | ✅ | User entity + visibility enum |
| **Backend Feature** | Users/README.md, Users/Endpoints/README.md, Users/Services/README.md, Users/Validators/README.md, Users/Mappings/README.md, Users/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Users/README.md, Users/UserContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | userService.js | ✅ | getMe, updateMe, changePassword, getMyOrganizations, getMyEvents, discoverMyOrganizations |
| **Frontend Adapter** | userAdapter.js | ✅ | toUserProfileViewModel, toMyOrganizationViewModel, toMyEventViewModel, toDiscoverOrganizationViewModel |
| **Frontend Pages** | UserProfilePage.jsx, UserSettingsPage.jsx, UserOrganizationsPage.jsx, UserEventsPage.jsx | ✅ | User workspace pages |
| **Frontend Components** | None | ❌ | Uses shared components only |

### 3. Organizations Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Organization.cs, OrgStatus.cs | ✅ | Organization entity + status enum |
| **Backend Feature** | Organizations/README.md, Organizations/Endpoints/README.md, Organizations/Services/README.md, Organizations/Validators/README.md, Organizations/Mappings/README.md, Organizations/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Organizations/README.md, Organizations/OrganizationContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | organizationService.js | ✅ | listOrganizations, createOrganization, getDefaultOrganization, getOrganizationById, updateOrganization, getPublicOverview |
| **Frontend Adapter** | organizationAdapter.js | ✅ | toOrganizationViewModel, toOrganizationSummaryViewModel, toOrganizationPublicOverviewViewModel |
| **Frontend Pages** | OrgOverviewPage.jsx | ✅ | Organization overview page |
| **Frontend Components** | OrgCard.jsx, OrgSwitcher.jsx | ✅ | Organization components |

### 4. Members Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Member.cs, MemberStatus.cs, MemberRole.cs | ✅ | Member entity + status/role enums |
| **Backend Feature** | Members/README.md, Members/Endpoints/README.md, Members/Services/README.md, Members/Validators/README.md, Members/Mappings/README.md, Members/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Members/README.md, Members/MemberContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | memberService.js | ✅ | getOrganizationMembers, addMember, updateMemberDepartment, removeMember |
| **Frontend Adapter** | memberAdapter.js | ✅ | toMemberViewModel, toMemberListViewModel |
| **Frontend Pages** | OrgMembersPage.jsx | ✅ | Members management page |
| **Frontend Components** | None | ❌ | Uses shared components only |

### 5. Departments Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Department.cs, DepartmentStatus.cs | ✅ | Department entity + status enum |
| **Backend Feature** | Departments/README.md, Departments/Endpoints/README.md, Departments/Services/README.md, Departments/Validators/README.md, Departments/Mappings/README.md, Departments/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Departments/README.md, Departments/DepartmentContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | departmentService.js | ✅ | getOrganizationDepartments, createDepartment, getDepartmentById, updateDepartment, deleteDepartment |
| **Frontend Adapter** | departmentAdapter.js | ✅ | toDepartmentViewModel, toDepartmentListViewModel |
| **Frontend Pages** | OrgDepartmentsPage.jsx | ✅ | Departments management page |
| **Frontend Components** | None | ❌ | Uses shared components only |

### 6. Events Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Event.cs, EventStatus.cs, EventVisibility.cs | ✅ | Event entity + status/visibility enums |
| **Backend Feature** | Events/README.md, Events/Endpoints/README.md, Events/Services/README.md, Events/Validators/README.md, Events/Mappings/README.md, Events/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Events/README.md, Events/EventContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | eventService.js | ✅ | getOrganizationEvents, createEvent, getEventById, updateEvent, deleteEvent, getPublicEvents, getPublicEventById |
| **Frontend Adapter** | eventAdapter.js | ✅ | toEventViewModel, toEventSummaryViewModel, toEventPublicViewModel, toEventListViewModel |
| **Frontend Pages** | OrgEventsPage.jsx, OrgEventDetailPage.jsx, PublicEventsPage.jsx, PublicEventDetailPage.jsx | ✅ | Event pages |
| **Frontend Components** | EventCard.jsx, EventStatusBadge.jsx | ✅ | Event components |

### 7. Milestones Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Milestone.cs, MilestoneStatus.cs | ✅ | Milestone entity + status enum |
| **Backend Feature** | Milestones/README.md, Milestones/Endpoints/README.md, Milestones/Services/README.md, Milestones/Validators/README.md, Milestones/Mappings/README.md, Milestones/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Milestones/README.md, Milestones/MilestoneContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | milestoneService.js | ✅ | getEventMilestones, createMilestone, getMilestoneById, updateMilestone, deleteMilestone |
| **Frontend Adapter** | milestoneAdapter.js | ✅ | toMilestoneViewModel, toMilestoneListViewModel |
| **Frontend Pages** | None (inside EventDetail) | ✅ | Part of OrgEventDetailPage |
| **Frontend Components** | MilestonePanel.jsx, MilestoneFormModal.jsx | ✅ | EventDetail tree components |

### 8. EventCategories Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | EventCategory.cs | ✅ | EventCategory entity |
| **Backend Feature** | EventCategories/README.md, EventCategories/Endpoints/README.md, EventCategories/Services/README.md, EventCategories/Validators/README.md, EventCategories/Mappings/README.md, EventCategories/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | EventCategories/README.md, EventCategories/CategoryContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | categoryService.js | ✅ | getMilestoneCategories, createCategory, getCategoryById, updateCategory, deleteCategory |
| **Frontend Adapter** | categoryAdapter.js | ✅ | toCategoryViewModel, toCategoryListViewModel |
| **Frontend Pages** | None (inside EventDetail) | ✅ | Part of OrgEventDetailPage |
| **Frontend Components** | CategoryPanel.jsx, CategoryFormModal.jsx | ✅ | EventDetail tree components |

### 9. Tasks Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | OrgTask.cs, TaskStatus.cs, TaskPriority.cs | ✅ | OrgTask entity + status/priority enums |
| **Backend Feature** | Tasks/README.md, Tasks/Endpoints/README.md, Tasks/Services/README.md, Tasks/Validators/README.md, Tasks/Mappings/README.md, Tasks/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Tasks/README.md, Tasks/TaskContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | taskService.js | ✅ | createTask, getTaskById, updateTask, deleteTask, updateTaskStatus, assignTask |
| **Frontend Adapter** | taskAdapter.js | ✅ | toTaskViewModel, toTaskListViewModel |
| **Frontend Pages** | None (inside EventDetail), OrgTasksPlaceholderPage.jsx | ✅ | Task CRUD inside EventDetail; aggregate board is placeholder |
| **Frontend Components** | TaskCard.jsx, TaskStatusControl.jsx, TaskAssignControl.jsx, TaskFormModal.jsx | ✅ | EventDetail tree components |

### 10. Requests Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Request.cs, RequestType.cs, RequestStatus.cs | ✅ | Request entity + type/status enums |
| **Backend Feature** | Requests/README.md, Requests/Endpoints/README.md, Requests/Services/README.md, Requests/Validators/README.md, Requests/Mappings/README.md, Requests/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Requests/README.md, Requests/RequestContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | requestService.js | ✅ | getOrganizationRequests, createOrganizationRequest, getRequestById, reviewRequest |
| **Frontend Adapter** | requestAdapter.js | ✅ | toRequestViewModel, toRequestListViewModel |
| **Frontend Pages** | OrgRequestsPage.jsx | ✅ | Requests management page |
| **Frontend Components** | None | ❌ | Uses shared components only |

### 11. Notifications Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Notification.cs, NotificationType.cs | ✅ | Notification entity + type enum |
| **Backend Feature** | Notifications/README.md, Notifications/Endpoints/README.md, Notifications/Services/README.md, Notifications/Validators/README.md, Notifications/Mappings/README.md, Notifications/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Notifications/README.md, Notifications/NotificationContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | notificationService.js | ✅ | getNotifications, getUnreadCount, markNotificationRead, markAllNotificationsRead |
| **Frontend Adapter** | notificationAdapter.js | ✅ | toNotificationViewModel, toNotificationListViewModel |
| **Frontend Pages** | OrgNotificationsPage.jsx | ✅ | Notifications list page |
| **Frontend Components** | NotificationBadge.jsx | ✅ | Notification badge component |

### 12. RolesPermissions Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Role.cs, Permission.cs, RolePermission.cs | ✅ | Role/Permission entities |
| **Backend Feature** | RolesPermissions/README.md, RolesPermissions/Endpoints/README.md, RolesPermissions/Services/README.md, RolesPermissions/Validators/README.md, RolesPermissions/Mappings/README.md, RolesPermissions/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | RolesPermissions/README.md, RolesPermissions/RoleContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | roleService.js | ✅ | getMyPermissions, normalizePermissionKeys, getOrganizationPermissions, getOrganizationRoles, createRole, updateRole, deleteRole, assignRoleToMember |
| **Frontend Adapter** | roleAdapter.js | ✅ | toPermissionViewModel, toRoleViewModel, toRoleListViewModel |
| **Frontend Pages** | OrgRolesPage.jsx | ✅ | Roles management page |
| **Frontend Components** | None | ❌ | Uses shared components only |

---

## SUPPORTING Modules (2 modules)

### 13. Friends Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | FriendRequest.cs, FriendRequestStatus.cs | ✅ | FriendRequest entity + status enum |
| **Backend Feature** | Friends/README.md, Friends/Endpoints/README.md, Friends/Services/README.md, Friends/Validators/README.md, Friends/Mappings/README.md, Friends/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Friends/README.md, Friends/FriendContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | friendService.js | ✅ | getFriends, getFriendRequests, sendFriendRequest, acceptFriendRequest, rejectFriendRequest |
| **Frontend Adapter** | friendAdapter.js | ✅ | toFriendViewModel, toFriendRequestViewModel, toFriendRequestListViewModel |
| **Frontend Pages** | UserFriendsPage.jsx | ✅ | Friends management page |
| **Frontend Components** | None | ❌ | Uses shared components only |

### 14. Discover Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Organization.cs, Event.cs (reused) | ✅ | Uses existing entities |
| **Backend Feature** | Discover/README.md, Discover/Endpoints/README.md, Discover/Services/README.md, Discover/Validators/README.md, Discover/Mappings/README.md, Discover/Permissions.TODO.md | ✅ | Full skeleton with TODO notes |
| **Shared Contract** | Discover/README.md, Discover/DiscoverContracts.cs.TODO | ✅ | Contract skeleton |
| **Frontend Service** | discoverService.js | ✅ | discoverOrganizations, discoverEvents |
| **Frontend Adapter** | discoverAdapter.js | ✅ | toDiscoverOrganizationViewModel, toDiscoverEventViewModel |
| **Frontend Pages** | UserDiscoverPage.jsx | ✅ | Discover page |
| **Frontend Components** | None | ❌ | Uses shared components only |

---

## DB_FOUNDATION_ONLY Modules (7 modules)

### 15. EventMembers Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | EventMember.cs, EventRole.cs | ✅ | EventMember entity + role enum |
| **Backend Feature** | EventMembers/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Shared Contract** | EventMembers/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Frontend Service** | None | ❌ | No working UI/API in base prototype |
| **Frontend Adapter** | None | ❌ | No working UI/API in base prototype |
| **Frontend Pages** | None | ❌ | No working UI/API in base prototype |
| **Frontend Components** | None | ❌ | No working UI/API in base prototype |

### 16. Attendees Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Attendee.cs, AttendeeStatus.cs | ✅ | Attendee entity + status enum |
| **Backend Feature** | Attendees/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Shared Contract** | Attendees/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Frontend Service** | None | ❌ | No working UI/API in base prototype |
| **Frontend Adapter** | None | ❌ | No working UI/API in base prototype |
| **Frontend Pages** | None | ❌ | No working UI/API in base prototype |
| **Frontend Components** | None | ❌ | No working UI/API in base prototype |

### 17. DigitalAssets Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | DigitalAsset.cs, FileType.cs | ✅ | DigitalAsset entity + file type enum |
| **Backend Feature** | DigitalAssets/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Shared Contract** | DigitalAssets/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Frontend Service** | None | ❌ | No working UI/API in base prototype |
| **Frontend Adapter** | None | ❌ | No working UI/API in base prototype |
| **Frontend Pages** | None | ❌ | No working UI/API in base prototype |
| **Frontend Components** | None | ❌ | No working UI/API in base prototype |

### 18. EventRatings Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | EventRating.cs, RatingAspect.cs | ✅ | EventRating entity + aspect enum |
| **Backend Feature** | EventRatings/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Shared Contract** | EventRatings/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Frontend Service** | None | ❌ | No working UI/API in base prototype |
| **Frontend Adapter** | None | ❌ | No working UI/API in base prototype |
| **Frontend Pages** | None | ❌ | No working UI/API in base prototype |
| **Frontend Components** | None | ❌ | No working UI/API in base prototype |

### 19. EventReports Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | EventReport.cs | ✅ | EventReport entity |
| **Backend Feature** | EventReports/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Shared Contract** | EventReports/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Frontend Service** | None | ❌ | No working UI/API in base prototype |
| **Frontend Adapter** | None | ❌ | No working UI/API in base prototype |
| **Frontend Pages** | OrgReportsPlaceholderPage.jsx | ⚠️ | Placeholder only |
| **Frontend Components** | None | ❌ | No working UI/API in base prototype |

### 20. Resources Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | Resource.cs, ResourceStatus.cs | ✅ | Resource entity + status enum |
| **Backend Feature** | Resources/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Shared Contract** | Resources/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Frontend Service** | None | ❌ | No working UI/API in base prototype |
| **Frontend Adapter** | None | ❌ | No working UI/API in base prototype |
| **Frontend Pages** | OrgResourcesPlaceholderPage.jsx | ⚠️ | Placeholder only |
| **Frontend Components** | None | ❌ | No working UI/API in base prototype |

### 21. ActivityHistory Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | ActivityHistory.cs, ActivityType.cs | ✅ | ActivityHistory entity + type enum |
| **Backend Feature** | ActivityHistory/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Shared Contract** | ActivityHistory/README.md | 📝 | DB_FOUNDATION_ONLY notes only |
| **Frontend Service** | None | ❌ | No working UI/API in base prototype |
| **Frontend Adapter** | None | ❌ | No working UI/API in base prototype |
| **Frontend Pages** | None | ❌ | No working UI/API in base prototype |
| **Frontend Components** | None | ❌ | No working UI/API in base prototype |

---

## PROTOTYPE_ONLY Pages (4 pages)

### 22. Aggregate Task Board

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | None (uses OrgTask) | ✅ | Task CRUD is CORE inside EventDetail tree |
| **Backend Feature** | None | ❌ | No aggregate board endpoint |
| **Shared Contract** | None | ❌ | No aggregate board contract |
| **Frontend Service** | None | ❌ | No aggregate board service |
| **Frontend Adapter** | None | ❌ | No aggregate board adapter |
| **Frontend Pages** | OrgTasksPlaceholderPage.jsx | ⚠️ | Placeholder only |
| **Frontend Components** | None | ❌ | Uses PrototypePlaceholder component |

### 23. Finance Page

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | None | ❌ | Finance-specific module excluded |
| **Backend Feature** | None | ❌ | No finance endpoint |
| **Shared Contract** | None | ❌ | No finance contract |
| **Frontend Service** | None | ❌ | No finance service |
| **Frontend Adapter** | None | ❌ | No finance adapter |
| **Frontend Pages** | OrgFinancePlaceholderPage.jsx | ⚠️ | Placeholder only |
| **Frontend Components** | None | ❌ | Uses PrototypePlaceholder component |

---

## EXCLUDED Modules (4 modules)

### 24. Posts Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | None | ❌ | Hard-excluded from rescue v1 |
| **Backend Feature** | None | ❌ | Hard-excluded from rescue v1 |
| **Shared Contract** | None | ❌ | Hard-excluded from rescue v1 |
| **Frontend Service** | None | ❌ | Hard-excluded from rescue v1 |
| **Frontend Adapter** | None | ❌ | Hard-excluded from rescue v1 |
| **Frontend Pages** | None | ❌ | Hard-excluded from rescue v1 |
| **Frontend Components** | None | ❌ | Hard-excluded from rescue v1 |

### 25. Comments Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | None | ❌ | Hard-excluded from rescue v1 |
| **Backend Feature** | None | ❌ | Hard-excluded from rescue v1 |
| **Shared Contract** | None | ❌ | Hard-excluded from rescue v1 |
| **Frontend Service** | None | ❌ | Hard-excluded from rescue v1 |
| **Frontend Adapter** | None | ❌ | Hard-excluded from rescue v1 |
| **Frontend Pages** | None | ❌ | Hard-excluded from rescue v1 |
| **Frontend Components** | None | ❌ | Hard-excluded from rescue v1 |

### 26. Messages/Chat Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | None | ❌ | Placeholder only, no working module |
| **Backend Feature** | None | ❌ | Placeholder only, no working module |
| **Shared Contract** | None | ❌ | Placeholder only, no working module |
| **Frontend Service** | None | ❌ | Placeholder only, no working module |
| **Frontend Adapter** | None | ❌ | Placeholder only, no working module |
| **Frontend Pages** | None (could add placeholder if needed) | ❌ | Placeholder only, no working module |
| **Frontend Components** | None | ❌ | Placeholder only, no working module |

### 27. Finance Working Module

| Layer | Files | Status | Notes |
|---|---|---|---|
| **Domain** | None | ❌ | Finance-specific module excluded |
| **Backend Feature** | None | ❌ | Finance-specific module excluded |
| **Shared Contract** | None | ❌ | Finance-specific module excluded |
| **Frontend Service** | None | ❌ | Finance-specific module excluded |
| **Frontend Adapter** | None | ❌ | Finance-specific module excluded |
| **Frontend Pages** | OrgFinancePlaceholderPage.jsx | ⚠️ | Placeholder only |
| **Frontend Components** | None | ❌ | Finance-specific module excluded |

---

## Summary Statistics

### Module Count by Status
- **CORE modules**: 12
- **SUPPORTING modules**: 2
- **DB_FOUNDATION_ONLY modules**: 7
- **PROTOTYPE_ONLY pages**: 4 (2 unique: aggregate task board, finance)
- **EXCLUDED modules**: 4

**Total modules**: 27

### File Count by Layer
- **Domain entities**: 22 entities + 21 enums = 43 files
- **Backend feature folders**: 14 CORE/SUPPORTING + 7 DB_FOUNDATION_ONLY = 21 folders
- **Shared contract folders**: 14 CORE/SUPPORTING + 7 DB_FOUNDATION_ONLY = 21 folders
- **Frontend services**: 14 files
- **Frontend adapters**: 13 files
- **Frontend pages**: 23 pages (3 public + 2 auth + 6 user + 8 org + 4 prototype)
- **Frontend components**: 13 components (8 EventDetail tree + 5 supporting)

**Total files created in Phase 3C**: ~150+ files (excluding domain/infrastructure from Phase 3B)

---

## Cross-Layer Consistency Verification

✅ All CORE modules (12) have complete cross-layer mapping  
✅ All SUPPORTING modules (2) have complete cross-layer mapping  
✅ All DB_FOUNDATION_ONLY modules (7) have domain + README notes only  
✅ All PROTOTYPE_ONLY pages (4) use PrototypePlaceholder component  
✅ All EXCLUDED modules (4) have no files created  
✅ No invented modules outside approved list  
✅ Task module clarity: CORE inside EventDetail tree, aggregate board is PROTOTYPE_ONLY  
✅ EventMember and Attendee: DB foundation only, no working UI/API  

---

**End of MODULE_FILE_MANIFEST.md**
