# FE Interface Service Map

- Source files: `src/Org.Frontend/Services/**/I*.cs`, `FrontendStartupExtensions.cs`, `*ApiClient.cs`, `*MockService.cs`
- UseMockServices current value (`appsettings.json`): `false`

## Interface Method Inventory

| Interface | Method | Params | Return Type | Domain | Expected Backend API | Notes |
|---|---|---|---|---|---|---|
| IAuthService | GetMeAsync | string accessToken, CancellationToken ct = default | Task<MeResponse> | Auth | GET /api/auth/me | Mapped from source; mock endpoints are non-canonical. |
| IAuthService | LoginAsync | LoginRequest request, CancellationToken ct = default | Task<LoginResponse> | Auth | POST /api/auth/login | Mapped from source; mock endpoints are non-canonical. |
| IAuthService | RegisterAsync | RegisterRequest request, CancellationToken ct = default | Task<RegisterResponse> | Auth | POST /api/auth/register | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | AssignManagerAsync | Guid departmentId, Guid? managerMemberId | Task<DepartmentDto> | Departments | PUT /api/departments/{id}/manager | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | AssignMemberAsync | Guid departmentId, Guid memberId | Task | Departments | POST /api/departments/{id}/members/{memberId} | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | CompleteDepartmentTaskAsync | Guid taskId | Task<DepartmentTaskDto> | Departments | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDepartmentService | CreateDepartment | CreateDepartmentRequest req | Task<DepartmentDto> | Departments | POST /api/departments | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | CreateDepartmentTaskAsync | Guid departmentId, CreateDepartmentTaskRequest request | Task<DepartmentTaskDto> | Departments | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDepartmentService | DeleteDepartment | Guid id | Task | Departments | DELETE /api/departments/{id} | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | DeleteDepartmentTaskAsync | Guid taskId | Task | Departments | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDepartmentService | GetDepartmentMembersAsync | Guid departmentId | Task<List<MemberDto>> | Departments | GET /api/departments/{id}/members | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | GetDepartments | Guid orgId | Task<List<DepartmentDto>> | Departments | GET /api/organizations/{orgId}/departments | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | GetDepartmentTasksAsync | Guid departmentId | Task<List<DepartmentTaskDto>> | Departments | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDepartmentService | GetTasksOverviewAsync | Guid departmentId | Task<DepartmentTasksOverviewViewModel> | Departments | GET /api/departments/{id}/tasks/overview | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | RemoveMemberAsync | Guid departmentId, Guid memberId | Task | Departments | DELETE /api/departments/{id}/members/{memberId} | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | UpdateDepartment | Guid id, UpdateDepartmentRequest req | Task<DepartmentDto> | Departments | PUT /api/departments/{id} | Mapped from source; mock endpoints are non-canonical. |
| IDepartmentService | UpdateDepartmentTaskAsync | Guid taskId, UpdateDepartmentTaskRequest request | Task<DepartmentTaskDto> | Departments | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDiscoverService | AcceptFriendRequestAsync | Guid requestId, CancellationToken ct = default | Task | Discover | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDiscoverService | DeclineFriendRequestAsync | Guid requestId, CancellationToken ct = default | Task | Discover | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDiscoverService | GetDiscoverFeedAsync | CancellationToken ct = default | Task<DiscoverFeedViewModel> | Discover | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDiscoverService | GetFriendRequestsAsync | CancellationToken ct = default | Task<IReadOnlyList<DiscoverFriendRequestItem>> | Discover | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDiscoverService | GetFriendsAsync | CancellationToken ct = default | Task<IReadOnlyList<DiscoverFriendItem>> | Discover | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDiscoverService | SearchDiscoverAsync | string? query, CancellationToken ct = default | Task<DiscoverSearchResultViewModel> | Discover | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IDiscoverService | SendFriendRequestAsync | Guid targetUserId, CancellationToken ct = default | Task | Discover | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IEventCategoryService | CreateCategoryAsync | CreateEventCategoryViewModel req | Task<EventCategoryViewModel> | Events/Milestones/Categories | POST /api/milestones/{milestoneId}/categories | Mapped from source; mock endpoints are non-canonical. |
| IEventCategoryService | DeleteCategoryAsync | Guid categoryId | Task | Events/Milestones/Categories | DELETE /api/categories/{id} | Mapped from source; mock endpoints are non-canonical. |
| IEventCategoryService | GetCategoriesAsync | Guid milestoneId | Task<List<EventCategoryViewModel>> | Events/Milestones/Categories | GET /api/milestones/{milestoneId}/categories | Mapped from source; mock endpoints are non-canonical. |
| IEventCategoryService | GetCategoryDetailAsync | Guid categoryId | Task<EventCategoryViewModel> | Events/Milestones/Categories | GET /api/categories/{id} | Mapped from source; mock endpoints are non-canonical. |
| IEventCategoryService | UpdateCategoryAsync | Guid categoryId, UpdateEventCategoryViewModel req | Task<EventCategoryViewModel> | Events/Milestones/Categories | PUT /api/categories/{id} | Mapped from source; mock endpoints are non-canonical. |
| IEventService | CanCreateEventAsync | Guid orgId | Task<bool> | Events/Milestones/Categories | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IEventService | CanManageEventAsync | Guid eventId | Task<bool> | Events/Milestones/Categories | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IEventService | CreateEventAsync | CreateEventViewModel request | Task<EventViewModel> | Events/Milestones/Categories | POST /api/events | Mapped from source; mock endpoints are non-canonical. |
| IEventService | DeleteEventAsync | Guid eventId | Task | Events/Milestones/Categories | DELETE /api/events/{id} | Mapped from source; mock endpoints are non-canonical. |
| IEventService | GetEventDetailAsync | Guid eventId | Task<EventViewModel?> | Events/Milestones/Categories | GET /api/events/{id} | Mapped from source; mock endpoints are non-canonical. |
| IEventService | GetEventsAsync | Guid orgId | Task<List<EventViewModel>> | Events/Milestones/Categories | GET /api/organizations/{orgId}/events | Mapped from source; mock endpoints are non-canonical. |
| IEventService | GetMyEventsAsync |  | Task<MyEventsViewModel> | Events/Milestones/Categories | GET /api/users/me/organizations + GET /api/users/me/events + GET /api/organizations/{orgId}/events | Mapped from source; mock endpoints are non-canonical. |
| IEventService | GetPublicEventDetailAsync | Guid eventId | Task<EventViewModel?> | Events/Milestones/Categories | GET /api/events/{id}/public | Mapped from source; mock endpoints are non-canonical. |
| IEventService | RegisterEventAsync | Guid eventId | Task | Events/Milestones/Categories | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IEventService | UnregisterEventAsync | Guid eventId | Task | Events/Milestones/Categories | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IEventService | UpdateEventAsync | Guid eventId, UpdateEventViewModel req | Task<EventViewModel> | Events/Milestones/Categories | PUT /api/events/{id} | Mapped from source; mock endpoints are non-canonical. |
| IFriendService | AcceptRequestAsync | Guid requestId, CancellationToken ct = default | Task | Friends | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IFriendService | GetDiscoverUsersAsync | int take = 12, CancellationToken ct = default | Task<IReadOnlyList<FriendProfileItem>> | Friends | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IFriendService | GetFriendsAsync | CancellationToken ct = default | Task<IReadOnlyList<FriendProfileItem>> | Friends | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IFriendService | GetIncomingRequestsAsync | CancellationToken ct = default | Task<IReadOnlyList<FriendRequestItem>> | Friends | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IFriendService | GetOutgoingRequestsAsync | CancellationToken ct = default | Task<IReadOnlyList<FriendRequestItem>> | Friends | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IFriendService | RejectRequestAsync | Guid requestId, CancellationToken ct = default | Task | Friends | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IFriendService | RemoveFriendAsync | Guid friendUserId, CancellationToken ct = default | Task | Friends | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IFriendService | SendRequestAsync | Guid receiverId, string? message = null, CancellationToken ct = default | Task | Friends | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IMemberService | AssignDepartment | Guid memberId, Guid departmentId | Task | Members | PUT /api/members/{id}/department | Mapped from source; mock endpoints are non-canonical. |
| IMemberService | AssignRole | Guid memberId, Guid roleId | Task | Members | PUT /api/members/{id}/role | Mapped from source; mock endpoints are non-canonical. |
| IMemberService | CanManageOrganizationMembersAsync | Guid orgId | Task<bool> | Members | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IMemberService | CreateMember | Guid orgId, FeatureCreateMemberRequest req | Task<MemberDto> | Members | POST /api/organizations/{orgId}/members | Mapped from source; mock endpoints are non-canonical. |
| IMemberService | DeleteMember | Guid memberId | Task | Members | DELETE /api/members/{id} | Mapped from source; mock endpoints are non-canonical. |
| IMemberService | GetMembers | Guid orgId | Task<List<MemberDto>> | Members | GET /api/organizations/{orgId}/members | Mapped from source; mock endpoints are non-canonical. |
| IMemberService | LeaveOrganizationAsync | Guid orgId | Task | Members | POST /api/organizations/{orgId}/leave | Mapped from source; mock endpoints are non-canonical. |
| IMessageService | GetConversationsAsync | CancellationToken ct = default | Task<IReadOnlyList<ConversationListItem>> | Messages | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IMessageService | GetMessageableUsersAsync | CancellationToken ct = default | Task<IReadOnlyList<MessageableUserItem>> | Messages | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IMessageService | GetMessagesAsync | Guid conversationId, CancellationToken ct = default | Task<IReadOnlyList<MessageItem>> | Messages | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IMessageService | GetOrCreateDirectConversationAsync | Guid otherUserId, CancellationToken ct = default | Task<ConversationOpenResult> | Messages | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IMessageService | SendMessageAsync | Guid conversationId, string content, CancellationToken ct = default | Task<MessageItem> | Messages | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IMilestoneService | CreateMilestoneAsync | CreateMilestoneViewModel req | Task<MilestoneViewModel> | Events/Milestones/Categories | POST /api/events/{eventId}/milestones | Mapped from source; mock endpoints are non-canonical. |
| IMilestoneService | DeleteMilestoneAsync | Guid milestoneId | Task | Events/Milestones/Categories | DELETE /api/milestones/{id} | Mapped from source; mock endpoints are non-canonical. |
| IMilestoneService | GetMilestonesAsync | Guid eventId | Task<List<MilestoneViewModel>> | Events/Milestones/Categories | GET /api/events/{eventId}/milestones | Mapped from source; mock endpoints are non-canonical. |
| IMilestoneService | UpdateMilestoneAsync | Guid milestoneId, UpdateMilestoneViewModel req | Task<MilestoneViewModel> | Events/Milestones/Categories | PUT /api/milestones/{id} | Mapped from source; mock endpoints are non-canonical. |
| INotificationService | ClearAllNotificationsAsync | bool onlyRead = false, CancellationToken ct = default | Task<int> | Notifications | DELETE /api/notifications/clear-all | Mapped from source; mock endpoints are non-canonical. |
| INotificationService | DeleteNotificationAsync | Guid id, CancellationToken ct = default | Task<int> | Notifications | DELETE /api/notifications/{id} | Mapped from source; mock endpoints are non-canonical. |
| INotificationService | GetNotificationByIdAsync | Guid id, CancellationToken ct = default | Task<NotificationDto> | Notifications | GET /api/notifications/{id} | Mapped from source; mock endpoints are non-canonical. |
| INotificationService | GetUnreadCountAsync | CancellationToken ct = default | Task<int> | Notifications | GET /api/notifications/unread-count | Mapped from source; mock endpoints are non-canonical. |
| INotificationService | MarkAllAsReadAsync | CancellationToken ct = default | Task<int> | Notifications | PUT /api/notifications/read-all | Mapped from source; mock endpoints are non-canonical. |
| INotificationService | MarkAsReadAsync | Guid id, CancellationToken ct = default | Task<NotificationDto> | Notifications | PUT /api/notifications/{id}/read | Mapped from source; mock endpoints are non-canonical. |
| INotificationService | StartRealtimeAsync | CancellationToken ct = default | Task | Notifications | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| INotificationService | StopRealtimeAsync | CancellationToken ct = default | Task | Notifications | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IOrganizationContext | GetOrganizationIdAsync | CancellationToken ct = default | Task<Guid> | Organizations | GET /api/organizations/default | Mapped from source; mock endpoints are non-canonical. |
| IOrganizationContext | ResetAsync | CancellationToken ct = default | Task | Organizations | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IOrganizationRoleService | AssignRoleToMemberAsync | Guid organizationId, Guid memberId, Guid roleId, CancellationToken ct = default | Task | Organizations | POST /api/organizations/{id}/members/{memberId}/role | Mapped from source; mock endpoints are non-canonical. |
| IOrganizationRoleService | CanManageRolesAsync | Guid organizationId, CancellationToken ct = default | Task<bool> | Organizations | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IOrganizationRoleService | CreateRoleAsync | Guid organizationId, UpsertOrganizationRoleRequest request, CancellationToken ct = default | Task<OrganizationRoleViewModel> | Organizations | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IOrganizationRoleService | DeleteRoleAsync | Guid roleId, CancellationToken ct = default | Task | Organizations | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IOrganizationRoleService | GetAvailablePermissionsAsync | Guid organizationId, CancellationToken ct = default | Task<IReadOnlyList<PermissionOptionViewModel>> | Organizations | GET /api/organizations/{id}/permissions | Mapped from source; mock endpoints are non-canonical. |
| IOrganizationRoleService | GetRolesAsync | Guid organizationId, CancellationToken ct = default | Task<IReadOnlyList<OrganizationRoleViewModel>> | Organizations | GET /api/organizations/{id}/roles | Mapped from source; mock endpoints are non-canonical. |
| IOrganizationRoleService | UpdateRoleAsync | Guid roleId, UpsertOrganizationRoleRequest request, CancellationToken ct = default | Task<OrganizationRoleViewModel> | Organizations | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IOrganizationService | CreateOrganizationAsync | CreateOrganizationViewModel model, CancellationToken ct = default | Task<OrganizationDetailViewModel> | Organizations | POST /api/organizations | Mapped from source; mock endpoints are non-canonical. |
| IOrganizationService | GetMyOrganizationsAsync | CancellationToken ct = default | Task<MyOrganizationsViewModel> | Organizations | GET /api/users/me/organizations | Mapped from source; mock endpoints are non-canonical. |
| IOrganizationService | GetOrganizationOverviewAsync | Guid organizationId, CancellationToken ct = default | Task<OrganizationOverviewViewModel> | Organizations | GET /api/organizations/{id}/public-overview + GET /api/organizations/{id}/permissions/me + GET /api/organizations/{orgId}/events | Mapped from source; mock endpoints are non-canonical. |
| IOrganizationService | GetOrganizationViewerPermissionAsync | Guid organizationId, CancellationToken ct = default | Task<OrganizationViewerPermissionViewModel> | Organizations | GET /api/organizations/{id}/permissions/me | Mapped from source; mock endpoints are non-canonical. |
| IOrganizationService | UpdateOrganizationOverviewAsync | Guid organizationId, UpdateOrganizationOverviewRequest request, CancellationToken ct = default | Task<OrganizationOverviewViewModel> | Organizations | GET /api/organizations/{id} + PUT /api/organizations/{id} | Mapped from source; mock endpoints are non-canonical. |
| IOverviewService | GetOverviewAsync | CancellationToken ct = default | Task<OverviewPageViewModel> | Other | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IPostService | AddCommentAsync | Guid postId, string content, CancellationToken ct = default | Task | Posts | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IPostService | CreatePostAsync | CreatePostInput input, CancellationToken ct = default | Task<PostFeedItem> | Posts | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IPostService | GetFeedAsync | int take = 20, CancellationToken ct = default | Task<IReadOnlyList<PostFeedItem>> | Posts | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IRequestService | ApproveRequestAsync | Guid requestId, CancellationToken ct = default | Task | Requests | POST /api/organizations/requests/{requestId}/review {decision=APPROVE} | Mapped from source; mock endpoints are non-canonical. |
| IRequestService | CanReviewOrganizationRequestsAsync | Guid orgId, CancellationToken ct = default | Task<bool> | Requests | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IRequestService | CanViewOrganizationRequestsAsync | Guid orgId, CancellationToken ct = default | Task<bool> | Requests | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IRequestService | GetPendingRequestsAsync | Guid orgId, CancellationToken ct = default | Task<List<RequestViewModel>> | Requests | GET /api/organizations/{id}/requests?status=PENDING | Mapped from source; mock endpoints are non-canonical. |
| IRequestService | GetRequestDetailAsync | Guid requestId, CancellationToken ct = default | Task<RequestDetailViewModel?> | Requests | GET /api/organizations/requests/{requestId} | Mapped from source; mock endpoints are non-canonical. |
| IRequestService | RejectRequestAsync | Guid requestId, CancellationToken ct = default | Task | Requests | POST /api/organizations/requests/{requestId}/review {decision=REJECT} | Mapped from source; mock endpoints are non-canonical. |
| IRequestService | SubmitJoinRequestAsync | JoinRequestFormViewModel form, CancellationToken ct = default | Task | Requests | POST /api/organizations/{id}/requests | Mapped from source; mock endpoints are non-canonical. |
| IRequestService | SubmitOrganizationRequestAsync | CreateOrganizationRequestViewModel form, CancellationToken ct = default | Task | Requests | POST /api/organizations/{id}/requests | Mapped from source; mock endpoints are non-canonical. |
| ISignalRService | StartAsync | CancellationToken ct = default | Task | Realtime | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| ISignalRService | StopAsync | CancellationToken ct = default | Task | Realtime | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| ITaskService | CanManageTasksAsync | Guid categoryId | Task<bool> | Tasks | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| ITaskService | CreateTaskAsync | Guid categoryId, CreateTaskViewModel req | Task<TaskViewModel> | Tasks | POST /api/categories/{categoryId}/tasks | Mapped from source; mock endpoints are non-canonical. |
| ITaskService | GetTasksAsync | Guid categoryId | Task<List<TaskViewModel>> | Tasks | GET /api/categories/{categoryId}/tasks | Mapped from source; mock endpoints are non-canonical. |
| ITaskService | UpdateTaskStatusAsync | Guid taskId, UpdateTaskStatusViewModel req | Task | Tasks | PUT /api/tasks/{taskId}/status | Mapped from source; mock endpoints are non-canonical. |
| ITokenStorage | ClearAsync | CancellationToken ct = default | Task | Auth | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| ITokenStorage | GetTokenAsync | CancellationToken ct = default | Task<string?> | Auth | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| ITokenStorage | GetTokenExpiryAsync | CancellationToken ct = default | Task<DateTime?> | Auth | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| ITokenStorage | SaveTokenAsync | string token, DateTime expiresAtUtc, CancellationToken ct = default | Task | Auth | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IUserDashboardService | GetDashboardAsync | CancellationToken ct = default | Task<UserDashboardViewModel> | Dashboard | GET /api/users/me/organizations + /events + /discover/* | Mapped from source; mock endpoints are non-canonical. |
| IUserProfileService | GetMyProfileVisibilityAsync | CancellationToken ct = default | Task<string> | Users | GET /api/users/me | Mapped from source; mock endpoints are non-canonical. |
| IUserProfileService | GetUserProfileAsync | Guid targetUserId, CancellationToken ct = default | Task<UserProfileViewModel> | Users | GET /api/users/{id} (+ fallback GET /api/users/me) | Mapped from source; mock endpoints are non-canonical. |
| IUserProfileService | UpdateMyProfileVisibilityAsync | string visibility, CancellationToken ct = default | Task | Users | PUT /api/users/me | Mapped from source; mock endpoints are non-canonical. |
| IUserSettingsService | ChangePasswordAsync | PasswordChangeFormViewModel request, CancellationToken ct = default | Task<UserSettingsOperationResult> | Users | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IUserSettingsService | DeleteAccountAsync | CancellationToken ct = default | Task<UserSettingsOperationResult> | Users | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IUserSettingsService | GetSettingsAsync | CancellationToken ct = default | Task<UserSettingsPageViewModel> | Users | GET /api/users/me | Mapped from source; mock endpoints are non-canonical. |
| IUserSettingsService | RevokeOtherSessionsAsync | CancellationToken ct = default | Task<UserSettingsOperationResult> | Users | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IUserSettingsService | SaveNotificationsAsync | NotificationPreferencesViewModel preferences, CancellationToken ct = default | Task<UserSettingsOperationResult> | Users | NEEDS_VERIFICATION | No direct backend endpoint mapping (client-side/state/realtime/unsupported). |
| IUserSettingsService | SaveProfileAsync | UserProfileFormViewModel profile, CancellationToken ct = default | Task<UserSettingsOperationResult> | Users | PUT /api/users/me | Mapped from source; mock endpoints are non-canonical. |

## DI Wiring (UseMockServices)

| Interface | Mock Implementation (mock=true) | Live Implementation (mock=false) | Evidence | Risk |
|---|---|---|---|---|
| IAuthService | AuthMockService | AuthApiClient | FrontendStartupExtensions.cs | Low |
| IOrganizationService | OrganizationMockService | OrganizationServiceApiClient | FrontendStartupExtensions.cs | Low |
| IOrganizationContext | MockOrganizationContext | OrganizationApiClient | FrontendStartupExtensions.cs | Low |
| IDepartmentService | DepartmentMockService | DepartmentApiClient | FrontendStartupExtensions.cs | Medium (department-task CRUD not supported in live impl) |
| IMemberService | MemberMockService | MemberApiClient | FrontendStartupExtensions.cs | Medium (legacy role GUID mapping path exists) |
| IEventService | EventMockService | EventApiClient | FrontendStartupExtensions.cs | Medium (register/unregister not supported; status label mapping issue) |
| IMilestoneService | MilestoneMockService | MilestoneApiClient | FrontendStartupExtensions.cs | Low |
| IEventCategoryService | EventCategoryMockService | EventCategoryApiClient | FrontendStartupExtensions.cs | Low |
| ITaskService | TaskMockService | TaskApiClient | FrontendStartupExtensions.cs | Medium (single assignee limit; UI string status assumptions) |
| IRequestService | RequestMockService | RequestApiClient | FrontendStartupExtensions.cs | Low/Medium (permission-dependent visibility) |
| INotificationService | NotificationMockService | NotificationService | FrontendStartupExtensions.cs | Low |
| IUserDashboardService | UserDashboardMockService | UserDashboardApiClient | FrontendStartupExtensions.cs | Low |
| IUserProfileService | UserProfileMockService | UserProfileApiClient | FrontendStartupExtensions.cs | Low |
| IUserSettingsService | UserSettingsMockService | UserSettingsApiClient | FrontendStartupExtensions.cs | Medium (several methods return Unsupported) |
| IFriendService | FriendMockService | FriendApiClient | FrontendStartupExtensions.cs | High (live impl throws NotSupportedException) |
| IDiscoverService | DiscoverMockService | DiscoverApiClient | FrontendStartupExtensions.cs | High (live impl throws NotSupportedException) |
| IOverviewService | OverviewMockService | OverviewApiClient | FrontendStartupExtensions.cs | High (live impl throws NotSupportedException) |
| IMessageService | MessageMockService | MessageApiClient | FrontendStartupExtensions.cs | High (live impl throws NotSupportedException) |
| IPostService | PostMockService | PostMockService (hard-wired) | FrontendStartupExtensions.cs | High (no live post service binding) |

## High-Risk Interface Methods

- `IEventService.RegisterEventAsync` and `IEventService.UnregisterEventAsync` throw `NotSupportedException` in `EventApiClient`.
- `IDepartmentService.GetDepartmentTasksAsync/CreateDepartmentTaskAsync/UpdateDepartmentTaskAsync/DeleteDepartmentTaskAsync/CompleteDepartmentTaskAsync` throw `NotSupportedException` in `DepartmentApiClient`.
- `IOverviewService.GetOverviewAsync` throws `NotSupportedException` in `OverviewApiClient`.
- All methods of `IMessageService`, `IFriendService`, and `IDiscoverService` throw `NotSupportedException` in live ApiClients.
- `IUserSettingsService.ChangePasswordAsync/SaveNotificationsAsync/RevokeOtherSessionsAsync/DeleteAccountAsync` return unsupported results in `UserSettingsApiClient`.

