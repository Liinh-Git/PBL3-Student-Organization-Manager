# PBL3_SYSTEM_DESIGN_AND_PROTOTYPE_HANDOFF_FINAL_CLEAN

> Ngôn ngữ: Tiếng Việt  
> Phiên bản: FINAL CLEAN  
> Nguồn: bản FINAL trước đó + audit facts Phase 1 + các lỗi logic đã phát hiện khi review kỹ  
> Mục đích: blueprint tự chứa để agent dựng base prototype React + Vite, đồng thời giữ Backend C#/.NET + FastEndpoints + EF Core + PostgreSQL theo kiến trúc hiện tại  
> Không phải: implementation code, timeline, sprint plan, task assignment, bug-fix plan

---

## 1. Product Intent Lock

### 1.1 Tên hệ thống

**Student Organization Manager** — ứng dụng web quản lý câu lạc bộ/tổ chức sinh viên trong trường đại học.

Hệ thống tập trung vào việc giúp người dùng:

- tạo và tham gia tổ chức sinh viên;
- quản lý thành viên, phòng ban, vai trò và quyền hạn;
- lập kế hoạch sự kiện;
- chia nhỏ sự kiện thành milestone, hạng mục và task;
- xử lý yêu cầu tham gia;
- nhận thông báo;
- khám phá tổ chức/sự kiện công khai;
- quản lý hồ sơ cá nhân và quan hệ bạn bè.

### 1.2 Chuỗi domain cốt lõi

```text
Organization → Member → Event → Milestone → EventCategory → Task
```

Đây là chuỗi nghiệp vụ chính. Mọi thiết kế prototype phải bảo vệ chuỗi này.

### 1.3 Domain hỗ trợ

- **Departments**: phòng ban/ban trong tổ chức, có manager, member và overview task.
- **Roles & Permissions**: role tùy chỉnh trong organization, permission key, role-permission mapping.
- **Requests**: yêu cầu tham gia tổ chức và workflow review.
- **Notifications**: thông báo in-app, REST list/count và SignalR nếu xác nhận hoạt động.
- **Friends**: kết bạn, friend requests.
- **Discover**: khám phá tổ chức/sự kiện.
- **User Profile & Settings**: hồ sơ, cài đặt cá nhân, đổi mật khẩu.

### 1.4 Loại trừ cứng khỏi prototype

| Area | Quyết định | Lý do |
|---|---|---|
| Posts/Comments | EXCLUDED | FE hiện chỉ có mock/no ApiClient thực; không đưa vào prototype |
| Mock | EXCLUDED | Không dùng mock làm source of truth |
| `/org/tasks` aggregate board | PROTOTYPE_ONLY | Không có endpoint list-by-org tasks |
| Messages/Chat | PROTOTYPE_ONLY | Không có BE endpoint `/api/messages` confirmed |
| Finance | PROTOTYPE_ONLY | Không có BE endpoint/domain contract đủ |
| Reports | PROTOTYPE_ONLY | Có `EventReport` entity nhưng chưa có REST endpoint đủ |
| Resources | PROTOTYPE_ONLY | Có `Resource` entity nhưng chưa có REST endpoint |
| Admin migration endpoint | EXCLUDED | Endpoint nội bộ, không có FE |
| Restore screens | OUT OF BASE PROTOTYPE | BE có một số restore endpoints, nhưng không đưa UI restore vào base prototype |

### 1.5 Quy tắc quan trọng nhất

- **Task trong EventDetail là CORE.**
- **Chỉ `/org/tasks` aggregate board mới là PROTOTYPE_ONLY.**
- Không được hiểu “thiếu list-by-org task endpoint” thành “Task module không làm”.
- EventDetail vẫn phải có UI Task theo chuỗi `Event → Milestone → Category → Task`.

---

## 2. Locked Stack & Source of Truth

### 2.1 Backend Stack bất biến

| Thành phần | Giá trị |
|---|---|
| Runtime | .NET 10 |
| Ngôn ngữ | C# |
| API framework | FastEndpoints |
| ORM | EF Core + PostgreSQL/Npgsql |
| Auth | JWT Bearer |
| Real-time | SignalR hub `/hubs/notifications` |
| Architecture | Vertical Slice / Feature-based |
| API contracts | `Org.Shared` |
| Soft-delete | `IsDeleted` + global query filter |
| Timestamp | `CreatedAt` / `UpdatedAt` trong `SaveChangesAsync` |
| DB baseline | 7 migrations đã xác nhận |

### 2.2 Frontend Target Stack bất biến

| Thành phần | Giá trị |
|---|---|
| Framework | React + Vite + JavaScript |
| Router | React Router v6+ với nested routes và `<Outlet />` |
| Auth state | Browser-side `AuthContext` |
| Org state | `OrgContext` |
| HTTP client | Centralized `httpClient.js` |
| Token | JWT Bearer trong `Authorization` header |
| Service layer | 1 service file per module |
| Adapter layer | DTO → ViewModel, tách khỏi page |
| UI state | Shared Loading / Empty / Error / Forbidden / PrototypePlaceholder |
| Không dùng | Blazor Server target implementation, mock fallback, raw API call trong page |

### 2.3 Vai trò của Blazor frontend cũ

Blazor hiện tại chỉ được dùng để hiểu:

- route intent;
- page intent;
- UI interaction intent;
- current implementation facts.

Blazor hiện tại **không được dùng** để copy:

- auth lifecycle;
- token bridge;
- layout implementation;
- router guard;
- notification badge code;
- component markup;
- hardcoded avatar/mock image;
- mock data behavior.

### 2.4 Source of Truth Priority

Khi có mâu thuẫn, ưu tiên:

1. Audit facts từ source code non-mock và swagger-live.
2. Backend entities, AppDbContext, migrations, Org.Shared contracts.
3. FastEndpoints handlers/controllers/endpoints.
4. FE non-mock pages/services/routes.
5. V2 blueprint nếu không mâu thuẫn audit.
6. Unknown/Unverified items.
7. Repo docs/diagram chỉ là intent thấp.
8. Mock: không bao giờ là source of truth.

### 2.5 Nhãn sử dụng trong tài liệu

| Nhãn | Nghĩa |
|---|---|
| CONFIRMED | Đã xác nhận từ source code / swagger / audit facts |
| PARTIAL | Có một phần bằng chứng, nhưng còn thiếu đoạn nối |
| UNRESOLVED | Chưa đủ bằng chứng, không được giả định |
| DESIGN_ASSUMPTION | Giả định thiết kế, phải có rủi ro |
| BASE | Là phần của base prototype |
| PROTOTYPE_ONLY | Chỉ placeholder/spec, không gọi API working |
| EXCLUDED | Không có route/page/service trong prototype |
| CONTRACT_GAP | Thiếu contract/API/DTO để triển khai đúng |

---

## 3. Backend Architecture Lock

### 3.1 Cấu trúc Backend đã xác nhận

```text
src/Org.Backend/
├── Domain/
│   ├── Entities/
│   └── Enums/
├── Features/
│   ├── Auth/
│   ├── Organizations/
│   ├── Users/
│   ├── Events/
│   ├── Departments/
│   ├── Milestones/
│   ├── EventCategories/
│   ├── Tasks/
│   ├── Members/
│   ├── Requests/
│   ├── Notifications/
│   └── Common/
├── Infrastructure/
│   ├── Database/
│   ├── Startup/
│   └── Services/
└── Migrations/
```

### 3.2 Shared Contracts

```text
src/Org.Shared/
├── Enums.cs
├── Common/
│   └── ApiContracts.cs
└── Features/
    ├── Auth/
    ├── Users/
    ├── Organizations/
    ├── Events/
    ├── EventCategories/
    ├── Milestones/
    ├── Departments/
    ├── Members/
    ├── Notifications/
    ├── Requests/
    └── Tasks/
```

### 3.3 Backend VSA/FastEndpoints rules

| Rule | Quy định |
|---|---|
| Feature slice | Mỗi module dùng slice riêng trong `Features/<Module>` |
| Endpoint handler | Mỗi endpoint qua FastEndpoints handler |
| DTO contract | Request/Response từ `Org.Shared`, không expose entity trực tiếp |
| Mapping | Entity → DTO qua mapping layer/common mapper |
| Validation | Validator chỉ validate input, không chứa business logic |
| Permission | Dùng backend authorization service/policy, FE chỉ là UI gating |
| DB access | Qua `AppDbContext` DI |
| Soft delete | DELETE là soft-delete nếu entity hỗ trợ |
| Restore | Có endpoint restore nhưng UI restore không thuộc base prototype |
| Transactions | Các action nhiều bước nên transaction ở BE, FE không tự bù dữ liệu |
| No endpoint invention | FE không tự bịa endpoint chưa có trong swagger/audit |

### 3.4 Permission Catalog

| Nhóm | Permission Keys |
|---|---|
| Overview | `org.overview.read`, `org.overview.write`, `org.workspace.access` |
| Members | `org.members.manage` |
| Roles | `org.roles.view`, `org.roles.create`, `org.roles.update`, `org.roles.delete`, `org.roles.assign` |
| Events | `org.events.create`, `org.events.manage` |
| Departments | `org.departments.manage` |
| Requests | `org.requests.view`, `org.requests.review`, `org.requests.approve` |

### 3.5 Role hierarchy

| Role | Level | Ý nghĩa |
|---|---:|---|
| Member | 0 | Thành viên thường |
| Manager | 1 | Có quyền lập kế hoạch/công việc ở mức thấp hơn |
| VicePresident | 2 | Có quyền quản lý cao hơn |
| President | 3 | Chủ tịch/quyền cao nhất |

Fine-grained permissions từ `RolePermission` có thể override logic role-based default. FE không được tự suy luận quyền từ role label nếu đã có permission keys.

---

## 4. React Frontend Prototype Architecture

### 4.1 VITE_API_BASE_URL convention

```env
VITE_API_BASE_URL=http://localhost:5000/api
```

`VITE_API_BASE_URL` **đã bao gồm `/api`**.

Vì vậy service path không được thêm `/api`.

```js
// Đúng
httpClient.get('/organizations');
httpClient.get(`/organizations/${orgId}/events`);
httpClient.post('/auth/login');
httpClient.get('/users/me/organizations');

// Sai
httpClient.get('/api/organizations');
```

### 4.2 httpClient contract

```js
// src/api/httpClient.js
const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 15000,
});
```

Quy tắc:

- Một instance duy nhất.
- Tất cả service import từ `src/api/httpClient.js`.
- Page/component không gọi `fetch`/`axios` trực tiếp.
- Request interceptor gắn Bearer token nếu có.
- Public endpoints vẫn có thể đi qua httpClient nhưng không yêu cầu token.
- 401: clear auth, redirect login.
- 403: không redirect global; trả error để page/route guard render `ForbiddenState`.
- Không tạo `/forbidden` route nếu blueprint chưa khai báo route đó.

### 4.3 AuthContext

`AuthContext` chịu trách nhiệm:

- `user`
- `token`
- `isAuthenticated`
- `isLoading`
- `initAuth()`
- `login(credentials)`
- `logout()`

LocalStorage keys:

```text
org.auth.accessToken
org.auth.accessTokenExpiryUtc
```

Luồng:

```text
App boot
→ AuthContext.initAuth()
→ đọc token + expiry
→ nếu không có token/hết hạn: anonymous state
→ nếu còn hạn: GET /auth/me
→ nếu 200: set user state
→ nếu 401: clear state
```

### 4.4 OrgContext

`OrgContext` chỉ dùng cho org workspace hoặc org-aware pages.

State:

```js
{
  orgId: string | null,
  organization: object | null,
  permissions: string[],
  isLoading: boolean,
  isMember: boolean,
  error: Error | null
}
```

Actions:

- `loadWorkspaceOrg(orgId)`
- `loadPermissions(orgId)`
- `clearOrg()`

### 4.5 OrgOverview public/member split

Không dùng một flow mơ hồ cho cả public overview và workspace.

#### Public/overview flow

`OrgOverviewPage` phải load public-safe data trước:

```text
GET /organizations/{id}/public-overview
```

Sau đó nếu user đã login, có thể thử load permission:

```text
GET /organizations/{id}/permissions/me
```

Nếu `/permissions/me` trả 403:

- không crash page;
- không render ForbiddenState cho overview;
- set `isMember = false`;
- render public overview + nút gửi yêu cầu tham gia nếu phù hợp.

#### Workspace flow

Các route `/org/*` nội bộ như members/departments/events/requests/roles cần:

- có token;
- có `orgId`;
- có membership/workspace access;
- `OrgMemberRoute` được pass.

### 4.6 Permission Normalizer

`GET /organizations/{id}/permissions/me` response shape chưa được xác nhận hoàn toàn.

`roleService.getMyPermissions(orgId)` phải normalize về `string[]`.

```js
function normalizePermissions(response) {
  if (Array.isArray(response)) return response;
  if (Array.isArray(response?.permissionKeys)) return response.permissionKeys;
  if (Array.isArray(response?.permissions)) return response.permissions;
  if (Array.isArray(response?.data)) return response.data;
  if (Array.isArray(response?.data?.permissionKeys)) return response.data.permissionKeys;
  if (Array.isArray(response?.data?.permissions)) return response.data.permissions;

  console.warn('[roleService] Cannot parse permissions, using safe fallback');
  return [];
}
```

Quy tắc bảo mật:

- Fallback không được cấp `org.workspace.access`.
- Fallback không được cấp write/manage permissions.
- Nếu permission parse fail, user chỉ được thấy public/readonly UI nếu data public cho phép.
- `isMember` không được suy ra từ fallback permissions.
- Workspace access chỉ được xác nhận khi backend trả permission/membership hợp lệ.

### 4.7 usePermission hook

```js
function usePermission() {
  const { permissions } = useOrg();

  const can = (key) => permissions.includes(key);
  const canAny = (keys) => keys.some((key) => permissions.includes(key));
  const canAll = (keys) => keys.every((key) => permissions.includes(key));

  return { can, canAny, canAll };
}
```

### 4.8 React Router guard

`ProtectedRoute` và `OrgMemberRoute` bắt buộc dùng `<Outlet />`.

```jsx
<Route element={<ProtectedRoute requireAuth />}>
  <Route element={<AppLayout />}>
    <Route path="user/organizations" element={<UserOrganizationsPage />} />
    <Route path="org-overview" element={<OrgOverviewPage />} />

    <Route element={<OrgMemberRoute />}>
      <Route path="org/members" element={<OrgMembersPage />} />
      <Route path="org/departments" element={<OrgDepartmentsPage />} />
      <Route path="org/events" element={<OrgEventsPage />} />
      <Route path="org/events/:id" element={<OrgEventDetailPage />} />
      <Route path="org/requests" element={<OrgRequestsPage />} />
      <Route path="org/roles" element={<OrgRolesPage />} />
      <Route path="org/tasks" element={<OrgTasksPlaceholderPage />} />
    </Route>
  </Route>
</Route>
```

### 4.9 Query string orgId rule

Tất cả `/org/*` routes dùng query string `?orgId=`.

- Không dùng `useParams()` để lấy `orgId`.
- Dùng `useSearchParams()`.

```js
const [searchParams] = useSearchParams();
const orgId = searchParams.get('orgId');
```

`useParams()` chỉ dùng cho resource id trong path, ví dụ:

```text
/org/events/:id?orgId=
```

- `id` lấy bằng `useParams()`;
- `orgId` lấy bằng `useSearchParams()`.

### 4.10 Layout

| Component | Trách nhiệm |
|---|---|
| `PublicLayout` | Public header + Outlet |
| `AppLayout` | Sidebar + TopBar + Outlet |
| `Sidebar` | User workspace + Org workspace nav |
| `TopBar` | Avatar, org switcher, notification badge |
| `NotificationBadge` | REST unread count/list; SignalR optional |

---

## 5. System Scope Lock

### 5.1 BASE modules

| Module | Status | Prototype treatment |
|---|---|---|
| Auth | BASE | Login/register/me/auth state |
| Users/Profile | BASE | Profile, settings, my organizations, my events |
| Organizations | BASE | CRUD + overview + default org + public overview |
| Members | BASE | List/add/remove/department update; role assignment theo canonical rule |
| Departments | BASE | List/create/update/delete/manager/members overview |
| Events | BASE | Org events, public events, detail, create/update/delete/visibility |
| Milestones | BASE | Trong EventDetail |
| EventCategories | BASE | Trong EventDetail |
| Tasks | BASE | Trong EventDetail, không phải `/org/tasks` aggregate |
| Requests | BASE | Submit/review/approve |
| Notifications | BASE | REST badge/dropdown; SignalR optional |
| Roles/Permissions | BASE | Role list/create/update/delete/assign, permission gating |
| Friends | BASE | Friends + friend requests |
| Discover | BASE | Discover orgs/events |
| User Profile/Settings | BASE | Profile update + change password |

### 5.2 PROTOTYPE_ONLY modules

| Module | Treatment |
|---|---|
| Messages/Chat | Placeholder page, no API call |
| Finance | Placeholder page |
| Reports | Placeholder page |
| Resources | Placeholder page |
| `/org/tasks` aggregate | Placeholder page, no fake board |
| Event Ratings | Future extension, no working UI in base prototype |

### 5.3 EXCLUDED modules

| Module | Treatment |
|---|---|
| Posts/Comments | No route, no page, no service |
| Admin migration | No frontend |
| ActivityHistory | No frontend |
| EventMember/Attendee/DigitalAsset UI | No UI until endpoints/contracts are defined |
| Restore screens | Not in base prototype |

### 5.4 Task scope correction

Task module is BASE only inside EventDetail:

```text
Event → Milestone → EventCategory → Task
```

`/org/tasks` is only a future aggregate board placeholder.

---

## 6. Canonical Domain Model

### 6.1 Core entities

| Entity | Key fields | Prototype role |
|---|---|---|
| User | Id, FullName, Email, Status, AvatarUrl, Bio | Auth/user/profile/friends |
| Organization | Id, OrgName, Description, AvatarUrl, CoverUrl, Status, Location | Org overview/workspace |
| Member | Id, UserId, OrgId, DepartmentId, RoleId, JoinDate | Membership and org access |
| Role | Id, RoleName, Description, OrgId, IsDefault | Custom role |
| Permission | Id, PermissionKey, DisplayName, ModuleGroup | Permission catalog |
| RolePermission | RoleId, PermissionId | Role permission mapping |
| Department | Id, OrgId, DeptName, Code, ManagerId, Function | Org departments |
| Event | Id, OrgId, EventName, StartDate, EndDate, Budget, Location, Status, Visibility | Events |
| Milestone | Id, EventId, Title, OrderIndex, StartDate, EndDate, Status | Event planning |
| EventCategory | Id, MilestoneId, CategoryName, OrderIndex, OwnerDepartmentId | Task grouping |
| OrgTask | Id, EventCategoryId, TaskName, AssigneeId, DeptId, Priority, Deadline, Status, Note | Atomic task |
| Request | Id, SenderId, OrgId, RequestType, Content, Status | Join/review flow |
| Notification | Id, ReceiverId, Title, Message, Type, IsRead, ActionUrl | Notification center |
| FriendRequest | Id, SenderId, ReceiverId, Status, RespondedAt | Social relation |
| EventRating | EventId, UserId, Rating, Aspect, Comment | Future/prototype-only |
| Resource | OrgId, EventId, ResourceName, Type, Quantity, Status | Prototype-only |
| EventReport | EventId, ActualAttendance, ActualBudget, RatingAverage | Prototype-only |
| OrganizationPost | OrgId, Title, Content, Visibility | Excluded |
| ActivityHistory | OrgId, Title, Type, ReferenceId, IsPublic | Excluded |

### 6.2 Optional field rule

Adapters must treat these fields as optional unless audit/runtime confirms response shape:

- `Event.location`
- `Event.budget`
- `Event.targetParticipants`
- `Event.averageRating`
- `User.avatarUrl`
- `Member.userId`
- `Member.roleId`
- `Department.managerId`
- `Category.tasks`
- `EventDetail.currentUserEventRole`
- `EventDetail.canManage`

No fake values. If absent:

- hide metric; or
- show “Chưa cập nhật”; or
- mark UNRESOLVED in component comment.

---

## 7. Unified API Contract Rules

### 7.1 Route notation

Because `VITE_API_BASE_URL` already contains `/api`, this document uses **FE service path** notation in service files.

| Backend route | FE service path |
|---|---|
| `/api/auth/login` | `/auth/login` |
| `/api/users/me` | `/users/me` |
| `/api/organizations/{id}` | `/organizations/{id}` |
| `/api/events/public` | `/events/public` |

### 7.2 Auth-free endpoints

Endpoints that do not require authenticated user:

```text
POST /auth/login
POST /auth/register
GET /events/public
GET /events/{id}/public
GET /organizations/{id}/public-overview if backend allows public/authenticated-public access
```

All other endpoints should be treated as requiring JWT unless audit/runtime proves otherwise.

### 7.3 API response rules

| Concern | Rule |
|---|---|
| List response | Use `ListResponse<T>` where backend returns list wrapper |
| Detail response | Raw DTO unless backend wraps it |
| Error response | Use `ErrorResponse` shape |
| Create response | Prefer created DTO |
| Update response | Prefer updated DTO |
| Delete response | No content or status response |
| 401 | Global auth handling |
| 403 | Page/route-level forbidden handling |
| Pagination | Page/pageSize if endpoint supports |
| Filter/sort | Use endpoint-specific query params |
| Optional fields | Adapter handles missing values safely |

### 7.4 Endpoint inventory

#### Auth

```text
POST /auth/login
POST /auth/register
GET  /auth/me
```

#### Users

```text
GET    /users/me
PUT    /users/me
GET    /users/me/organizations
GET    /users/me/events
PUT    /users/me/change-password
GET    /users/me/discover/organizations
GET    /users/me/discover/events
GET    /users/{id}
POST   /users/batch
POST   /users/{id}/friend-request
GET    /users/me/friend-requests
PUT    /users/me/friend-requests/{id}/accept
DELETE /users/me/friend-requests/{id}
GET    /users/me/friends
DELETE /users/me/friends/{id}
```

#### Organizations

```text
GET    /organizations
POST   /organizations
GET    /organizations/default
GET    /organizations/{id}
PUT    /organizations/{id}
DELETE /organizations/{id}
POST   /organizations/{id}/restore
GET    /organizations/{id}/public-overview
GET    /organizations/{id}/permissions/me
GET    /organizations/{id}/permissions
GET    /organizations/{id}/roles
POST   /organizations/{id}/roles
PUT    /organizations/roles/{roleId}
DELETE /organizations/roles/{roleId}
POST   /organizations/{id}/members/{memberId}/role
```

#### Members

```text
GET    /organizations/{orgId}/members
POST   /organizations/{orgId}/members
PUT    /members/{id}/role
PUT    /members/{id}/department
DELETE /members/{id}
POST   /organizations/{orgId}/leave
```

#### Events

```text
GET    /organizations/{orgId}/events
POST   /events
GET    /events/{id}
PUT    /events/{id}
DELETE /events/{id}
POST   /events/{id}/restore
PUT    /events/{id}/visibility
GET    /events/public
GET    /events/{id}/public
```

#### Milestones

```text
POST   /events/{eventId}/milestones
GET    /events/{eventId}/milestones
GET    /milestones/{id}
PUT    /milestones/{id}
DELETE /milestones/{id}
POST   /milestones/{id}/restore
```

#### EventCategories

```text
POST   /milestones/{milestoneId}/categories
GET    /milestones/{milestoneId}/categories
GET    /categories/{id}
PUT    /categories/{id}
DELETE /categories/{id}
POST   /categories/{id}/restore
```

#### Tasks

```text
POST   /categories/{categoryId}/tasks
GET    /tasks/{taskId}
PUT    /tasks/{taskId}
DELETE /tasks/{taskId}
PUT    /tasks/{taskId}/status
PUT    /tasks/{taskId}/assign
POST   /tasks/{taskId}/restore
```

#### Departments

```text
GET    /organizations/{orgId}/departments
POST   /departments
GET    /departments/{id}
PUT    /departments/{id}
DELETE /departments/{id}
POST   /departments/{id}/restore
PUT    /departments/{id}/manager
GET    /departments/{id}/members
POST   /departments/{id}/members/{memberId}
DELETE /departments/{id}/members/{memberId}
GET    /departments/{id}/tasks/overview
```

#### Notifications

```text
GET    /notifications
GET    /notifications/unread-count
GET    /notifications/{id}
DELETE /notifications/{id}
PUT    /notifications/{id}/read
PUT    /notifications/read-all
DELETE /notifications/clear-all
```

#### Requests

```text
GET    /organizations/{id}/requests
POST   /organizations/{orgId}/requests
GET    /organizations/requests/{requestId}
POST   /organizations/requests/{requestId}/review
```

#### EventRatings

```text
POST   /events/{eventId}/ratings
GET    /events/{eventId}/ratings
GET    /events/{eventId}/ratings/stats
DELETE /ratings/{id}
```

EventRatings are not working UI in base prototype.

#### Posts/Admin

Posts and admin migration endpoints are out of frontend prototype scope.

---

## 8. React Prototype File Manifest

This is an artifact manifest, not code.

```text
.env
src/
├── main.jsx
├── App.jsx
├── api/
│   └── httpClient.js
├── contexts/
│   ├── AuthContext.jsx
│   └── OrgContext.jsx
├── hooks/
│   ├── useAuth.js
│   ├── useOrg.js
│   ├── usePermission.js
│   └── useNotifications.js
├── services/
│   ├── authService.js
│   ├── userService.js
│   ├── organizationService.js
│   ├── roleService.js
│   ├── memberService.js
│   ├── eventService.js
│   ├── milestoneService.js
│   ├── categoryService.js
│   ├── taskService.js
│   ├── departmentService.js
│   ├── notificationService.js
│   ├── requestService.js
│   ├── friendService.js
│   └── discoverService.js
├── adapters/
│   ├── userAdapter.js
│   ├── organizationAdapter.js
│   ├── eventAdapter.js
│   ├── milestoneAdapter.js
│   ├── categoryAdapter.js
│   ├── taskAdapter.js
│   ├── memberAdapter.js
│   ├── departmentAdapter.js
│   ├── notificationAdapter.js
│   └── requestAdapter.js
├── router/
│   ├── AppRouter.jsx
│   ├── ProtectedRoute.jsx
│   └── OrgMemberRoute.jsx
├── layouts/
│   ├── AppLayout.jsx
│   ├── PublicLayout.jsx
│   ├── Sidebar.jsx
│   └── TopBar.jsx
├── components/
│   ├── shared/
│   │   ├── LoadingSpinner.jsx
│   │   ├── EmptyState.jsx
│   │   ├── ErrorState.jsx
│   │   ├── ForbiddenState.jsx
│   │   ├── PrototypePlaceholder.jsx
│   │   ├── ConfirmDialog.jsx
│   │   └── Pagination.jsx
│   ├── notifications/
│   │   └── NotificationBadge.jsx
│   ├── org/
│   │   ├── OrgCard.jsx
│   │   └── OrgSwitcher.jsx
│   ├── event/
│   │   ├── EventCard.jsx
│   │   └── EventStatusBadge.jsx
│   └── event-detail/
│       ├── MilestonePanel.jsx
│       ├── CategoryPanel.jsx
│       ├── TaskCard.jsx
│       ├── TaskStatusControl.jsx
│       ├── TaskAssignControl.jsx
│       ├── MilestoneFormModal.jsx
│       ├── CategoryFormModal.jsx
│       └── TaskFormModal.jsx
└── pages/
    ├── public/
    ├── auth/
    ├── user/
    └── org/
```

No `postService.js`.  
No working `eventRatingService.js` in base prototype.  
If EventRating needs documentation, create `src/services/eventRatingContractNote.md`, not an API-calling service.

---

## 9. Frontend Route & Navigation Contract

### 9.1 Route table

| Route | Page | Auth | Org Member | Permission | Status |
|---|---|---|---|---|---|
| `/` | HomePage | No | No | — | BASE |
| `/login` | LoginPage | No | No | — | BASE |
| `/register` | RegisterPage | No | No | — | BASE |
| `/events` | PublicEventsPage | No | No | — | BASE |
| `/events/:id` | PublicEventDetailPage | No | No | — | BASE |
| `/user/organizations` | UserOrganizationsPage | Yes | No | — | BASE |
| `/user/events` | UserEventsPage | Yes | No | — | BASE |
| `/user/profile` | UserProfilePage | Yes | No | — | BASE |
| `/user/settings` | UserSettingsPage | Yes | No | — | BASE |
| `/user/friends` | UserFriendsPage | Yes | No | — | BASE |
| `/user/discover` | UserDiscoverPage | Yes | No | — | BASE |
| `/user/messages` | UserMessagesPage | Yes | No | — | PROTOTYPE_ONLY |
| `/org-overview?orgId=` | OrgOverviewPage | Yes | No | — | BASE |
| `/org/members?orgId=` | OrgMembersPage | Yes | Yes | — | BASE |
| `/org/departments?orgId=` | OrgDepartmentsPage | Yes | Yes | — | BASE |
| `/org/events?orgId=` | OrgEventsPage | Yes | Yes | — | BASE |
| `/org/events/:id?orgId=` | OrgEventDetailPage | Yes | Yes | — | BASE |
| `/org/requests?orgId=` | OrgRequestsPage | Yes | Yes | `org.requests.view` | BASE |
| `/org/roles?orgId=` | OrgRolesPage | Yes | Yes | `org.roles.view` | BASE |
| `/org/tasks?orgId=` | OrgTasksPlaceholderPage | Yes | Yes | — | PROTOTYPE_ONLY |
| `/org/finance?orgId=` | OrgFinancePage | Yes | Yes | — | PROTOTYPE_ONLY |
| `/org/reports?orgId=` | OrgReportsPage | Yes | Yes | — | PROTOTYPE_ONLY |
| `/org/resources?orgId=` | OrgResourcesPage | Yes | Yes | — | PROTOTYPE_ONLY |

### 9.2 Public navigation

| Item | Route | Visible when |
|---|---|---|
| Home | `/` | Always |
| Events | `/events` | Always |
| Login | `/login` | Anonymous |
| Register | `/register` | Anonymous |

### 9.3 User workspace navigation

| Item | Route | Required |
|---|---|---|
| My Organizations | `/user/organizations` | Auth |
| My Events | `/user/events` | Auth |
| Friends | `/user/friends` | Auth |
| Discover | `/user/discover` | Auth |
| Profile | `/user/profile` | Auth |
| Settings | `/user/settings` | Auth |
| Messages | `/user/messages` | Auth, PROTOTYPE_ONLY if visible |

### 9.4 Org workspace navigation

| Item | Route | Required | Status |
|---|---|---|---|
| Overview | `/org-overview?orgId=` | Auth | BASE |
| Members | `/org/members?orgId=` | Member | BASE |
| Departments | `/org/departments?orgId=` | Member | BASE |
| Events | `/org/events?orgId=` | Member | BASE |
| Requests | `/org/requests?orgId=` | Member + `org.requests.view` | BASE |
| Roles | `/org/roles?orgId=` | Member + `org.roles.view` | BASE |
| Tasks aggregate | `/org/tasks?orgId=` | Member | PROTOTYPE_ONLY or hidden |
| Finance | `/org/finance?orgId=` | Member | PROTOTYPE_ONLY if visible |
| Reports | `/org/reports?orgId=` | Member | PROTOTYPE_ONLY if visible |
| Resources | `/org/resources?orgId=` | Member | PROTOTYPE_ONLY if visible |

Nav rules:

- Posts/Comments never visible.
- Permission-locked nav items are hidden.
- Prototype-only nav items should be labeled “Sắp ra mắt” if visible.
- `/org/tasks` aggregate can be hidden to reduce confusion.

---

## 10. Module Prototype Artifact Matrix

| Module | Route(s) | Page Shell(s) | Service | Adapter | Key Components | Status |
|---|---|---|---|---|---|---|
| Auth | `/login`, `/register` | LoginPage, RegisterPage | authService | — | LoginForm, RegisterForm | BASE |
| User Profile | `/user/profile` | UserProfilePage | userService | userAdapter | ProfileForm | BASE |
| User Settings | `/user/settings` | UserSettingsPage | userService | — | PasswordForm | BASE |
| User Orgs | `/user/organizations` | UserOrganizationsPage | userService, organizationService | organizationAdapter | OrgCard, CreateOrgModal | BASE |
| User Events | `/user/events` | UserEventsPage | userService, eventService | eventAdapter | EventCard | BASE |
| Friends | `/user/friends` | UserFriendsPage | friendService | optional | FriendList, FriendRequestList | BASE |
| Discover | `/user/discover` | UserDiscoverPage | discoverService | optional | OrgCard, EventCard | BASE |
| Messages | `/user/messages` | UserMessagesPage | — | — | PrototypePlaceholder | PROTOTYPE_ONLY |
| Org Overview | `/org-overview` | OrgOverviewPage | organizationService, roleService | organizationAdapter | OrgInfoCard, EditOrgModal | BASE |
| Members | `/org/members` | OrgMembersPage | memberService, roleService | memberAdapter | MemberTable, AddMemberModal, AssignRoleModal | BASE |
| Departments | `/org/departments` | OrgDepartmentsPage | departmentService | departmentAdapter | DeptList, DeptFormModal | BASE |
| Events | `/org/events` | OrgEventsPage | eventService | eventAdapter | EventTable, CreateEventModal | BASE |
| EventDetail | `/org/events/:id` | OrgEventDetailPage | eventService, milestoneService, categoryService, taskService | eventAdapter, milestoneAdapter, categoryAdapter, taskAdapter | MilestonePanel, CategoryPanel, TaskCard | BASE |
| Requests | `/org/requests` | OrgRequestsPage | requestService | requestAdapter | RequestTable, ReviewModal | BASE |
| Roles | `/org/roles` | OrgRolesPage | roleService | optional | RoleList, RoleFormModal | BASE |
| Notifications | component | NotificationBadge | notificationService | notificationAdapter | Badge, Dropdown | BASE |
| EventRatings | none in base | none | — | — | none | PROTOTYPE_ONLY/FUTURE |
| Tasks aggregate | `/org/tasks` | OrgTasksPlaceholderPage | — | — | PrototypePlaceholder | PROTOTYPE_ONLY |
| Finance | `/org/finance` | OrgFinancePage | — | — | PrototypePlaceholder | PROTOTYPE_ONLY |
| Reports | `/org/reports` | OrgReportsPage | — | — | PrototypePlaceholder | PROTOTYPE_ONLY |
| Resources | `/org/resources` | OrgResourcesPage | — | — | PrototypePlaceholder | PROTOTYPE_ONLY |
| Posts/Comments | — | — | — | — | — | EXCLUDED |

---

## 11. Module Prototype Specs

### 11.1 Auth

Service: `authService.js`

- `login(credentials)` → POST `/auth/login`
- `register(data)` → POST `/auth/register`
- `getMe()` → GET `/auth/me`

Flow:

```text
Login → save token → GET /auth/me → set AuthContext → navigate /user/organizations
```

Unresolved:

- exact token expiry config;
- whether register should auto-login or redirect login.

### 11.2 User

Service: `userService.js`

- `getMyProfile()` → GET `/users/me`
- `updateMyProfile(data)` → PUT `/users/me`
- `getMyOrganizations()` → GET `/users/me/organizations`
- `getMyEvents()` → GET `/users/me/events`
- `changePassword(data)` → PUT `/users/me/change-password`

Do not put `getMyOrganizations()` in `organizationService`.

### 11.3 Organizations

Service: `organizationService.js`

- `getOrganizations(params)` → GET `/organizations`
- `createOrganization(data)` → POST `/organizations`
- `getOrganizationById(id)` → GET `/organizations/{id}`
- `updateOrganization(id, data)` → PUT `/organizations/{id}`
- `getDefaultOrganization()` → GET `/organizations/default`
- `getPublicOverview(id)` → GET `/organizations/{id}/public-overview`

Service: `roleService.js`

- `getMyPermissions(orgId)` → GET `/organizations/{id}/permissions/me`
- `getPermissionCatalog(orgId)` → GET `/organizations/{id}/permissions`
- `getRoles(orgId)` → GET `/organizations/{id}/roles`
- `createRole(orgId, data)` → POST `/organizations/{id}/roles`
- `updateRole(roleId, data)` → PUT `/organizations/roles/{roleId}`
- `deleteRole(roleId)` → DELETE `/organizations/roles/{roleId}`
- `assignRoleToMember(orgId, memberId, data)` → POST `/organizations/{id}/members/{memberId}/role`

OrgOverview rules:

- load public overview first;
- member actions only if membership/permissions confirm;
- non-member can submit join request if allowed.

### 11.4 Members

Service: `memberService.js`

- `getMembers(orgId)` → GET `/organizations/{orgId}/members`
- `addMember(orgId, data)` → POST `/organizations/{orgId}/members`
- `updateMemberDept(memberId, data)` → PUT `/members/{id}/department`
- `removeMember(memberId)` → DELETE `/members/{id}`
- `leaveOrg(orgId)` → POST `/organizations/{orgId}/leave`

Role assignment rule:

- Use `roleService.assignRoleToMember()` as canonical role assignment for custom DB roles.
- Permission: `org.roles.assign`.
- `PUT /members/{id}/role` must be treated as legacy/ambiguous until DTO semantics are confirmed.
- Do not expose two role assignment flows in UI.

Member row optional fields:

- If `MemberDto` does not include `userId`, do not create profile link.
- If `MemberDto` does not include `roleId`, role assignment modal must use selected role id from `roleService.getRoles()`, not current row role id.

### 11.5 Departments

Service: `departmentService.js`

- `getDepartments(orgId)` → GET `/organizations/{orgId}/departments`
- `createDept(data)` → POST `/departments`
- `updateDept(id, data)` → PUT `/departments/{id}`
- `deleteDept(id)` → DELETE `/departments/{id}`
- `setManager(id, data)` → PUT `/departments/{id}/manager`
- `getDeptMembers(id)` → GET `/departments/{id}/members`
- `addDeptMember(id, memberId)` → POST `/departments/{id}/members/{memberId}`
- `removeDeptMember(id, memberId)` → DELETE `/departments/{id}/members/{memberId}`
- `getDeptTasksOverview(id)` → GET `/departments/{id}/tasks/overview`

Department manager local event override is not implemented in base prototype unless API returns required fields.

### 11.6 Events

Service: `eventService.js`

- `getOrgEvents(orgId, params)` → GET `/organizations/{orgId}/events`
- `createEvent(data)` → POST `/events`
- `getEventById(id)` → GET `/events/{id}`
- `updateEvent(id, data)` → PUT `/events/{id}`
- `deleteEvent(id)` → DELETE `/events/{id}`
- `changeVisibility(id, data)` → PUT `/events/{id}/visibility`
- `getPublicEvents(params)` → GET `/events/public`
- `getPublicEventById(id)` → GET `/events/{id}/public`

Optional event fields must be handled safely:

- location;
- budget;
- targetParticipants;
- averageRating;
- tags.

No fake metrics.

### 11.7 Milestones

Service: `milestoneService.js`

- `getMilestones(eventId)` → GET `/events/{eventId}/milestones`
- `createMilestone(eventId, data)` → POST `/events/{eventId}/milestones`
- `getMilestoneById(id)` → GET `/milestones/{id}`
- `updateMilestone(id, data)` → PUT `/milestones/{id}`
- `deleteMilestone(id)` → DELETE `/milestones/{id}`

Restore is out of base prototype.

### 11.8 EventCategories

Service: `categoryService.js`

- `getCategories(milestoneId)` → GET `/milestones/{milestoneId}/categories`
- `createCategory(milestoneId, data)` → POST `/milestones/{milestoneId}/categories`
- `getCategoryById(id)` → GET `/categories/{id}`
- `updateCategory(id, data)` → PUT `/categories/{id}`
- `deleteCategory(id)` → DELETE `/categories/{id}`

Category DTO may or may not contain `tasks[]`.

### 11.9 Tasks

Service: `taskService.js`

- `createTask(categoryId, data)` → POST `/categories/{categoryId}/tasks`
- `getTaskById(taskId)` → GET `/tasks/{taskId}`
- `updateTask(taskId, data)` → PUT `/tasks/{taskId}`
- `deleteTask(taskId)` → DELETE `/tasks/{taskId}`
- `updateTaskStatus(taskId, data)` → PUT `/tasks/{taskId}/status`
- `assignTask(taskId, data)` → PUT `/tasks/{taskId}/assign`

Task list rules:

- Task list in EventDetail only comes from `category.tasks[]` if returned by API, or from local mutation after create.
- No list-by-category endpoint confirmed.
- No list-by-org endpoint confirmed.
- Do not fake task data.

### 11.10 Requests

Service: `requestService.js`

- `getRequests(orgId)` → GET `/organizations/{id}/requests`
- `submitRequest(orgId, data)` → POST `/organizations/{orgId}/requests`
- `getRequestById(requestId)` → GET `/organizations/requests/{requestId}`
- `reviewRequest(requestId, data)` → POST `/organizations/requests/{requestId}/review`

### 11.11 Notifications

Service: `notificationService.js`

- `getNotifications(params)` → GET `/notifications`
- `getUnreadCount()` → GET `/notifications/unread-count`
- `markRead(id)` → PUT `/notifications/{id}/read`
- `markAllRead()` → PUT `/notifications/read-all`
- `deleteNotification(id)` → DELETE `/notifications/{id}`
- `clearAll()` → DELETE `/notifications/clear-all`

Base prototype requirement:

- REST unread count;
- REST dropdown/list.

SignalR:

- optional enhancement;
- not required to mark base prototype complete if payload/auth is unresolved.

### 11.12 Friends

Service: `friendService.js`

- `sendFriendRequest(userId)` → POST `/users/{id}/friend-request`
- `getFriendRequests()` → GET `/users/me/friend-requests`
- `acceptRequest(id)` → PUT `/users/me/friend-requests/{id}/accept`
- `rejectRequest(id)` → DELETE `/users/me/friend-requests/{id}`
- `getFriends()` → GET `/users/me/friends`
- `removeFriend(id)` → DELETE `/users/me/friends/{id}`

### 11.13 Discover

Service: `discoverService.js`

- `discoverOrgs(params)` → GET `/users/me/discover/organizations`
- `discoverEvents(params)` → GET `/users/me/discover/events`

### 11.14 EventRating

EventRating endpoints exist, but base prototype has no working EventRating UI.

Rules:

- no `RatingForm`;
- no `RatingList`;
- no import in EventDetail;
- no API-calling eventRatingService in base prototype;
- future extension only.

---

## 12. EventDetail Tree Prototype Spec

### 12.1 Scope

EventDetail tree is a BASE/core flow.

```text
Event → Milestone → EventCategory → Task
```

Route:

```text
/org/events/:id?orgId=
```

Page:

```text
OrgEventDetailPage.jsx
```

### 12.2 Data loading order

1. Read `eventId` from `useParams()`.
2. Read `orgId` from `useSearchParams()`.
3. Load event detail:

```text
GET /events/{id}
```

4. Load milestones:

```text
GET /events/{eventId}/milestones
```

5. For each milestone, load categories:

```text
GET /milestones/{milestoneId}/categories
```

6. For each category:

- if response contains `tasks[]`, normalize it;
- if response does not contain `tasks[]`, initialize `tasks: []`;
- do not invent task list endpoint;
- do not fake task data.

### 12.3 EventDetailViewModel

```js
const EventDetailViewModel = {
  event: {
    id,
    orgId,
    eventName,
    startDate,
    endDate,
    budget,
    location,
    targetParticipants,
    tags,
    status,
    visibility,
    averageRating,
  },
  milestones: [
    {
      id,
      title,
      orderIndex,
      startDate,
      endDate,
      status,
      uiState: { isExpanded: true, isLoading: false },
      categories: [
        {
          id,
          categoryName,
          orderIndex,
          ownerDepartmentId,
          uiState: { isExpanded: true, isLoading: false },
          tasks: [],
        },
      ],
    },
  ],
  uiState: {
    eventLoading: false,
    milestonesLoading: false,
    error: null,
    forbidden: false,
  },
};
```

Rules:

- `category.tasks` is always an array.
- Never leave `tasks` as undefined.
- No fake tasks.
- Existing tasks only display if API returns them.
- Newly created tasks display if POST create task returns TaskDto.

### 12.4 UI structure

```text
OrgEventDetailPage
├── EventInfoSection
│   ├── event name/date/status/location
│   └── edit/delete/visibility controls if permitted
└── MilestonesSection
    ├── Add Milestone button
    └── MilestonePanel
        ├── Edit/Delete milestone controls
        ├── Add Category button
        └── CategoryPanel
            ├── Edit/Delete category controls
            ├── Add Task button
            └── TaskList
                └── TaskCard
                    ├── TaskStatusControl
                    ├── TaskAssignControl
                    └── Edit/Delete task controls
```

### 12.5 CRUD modals

| Modal | Trigger | Endpoint | Fields |
|---|---|---|---|
| MilestoneFormModal | Add/Edit Milestone | POST `/events/{eventId}/milestones`, PUT `/milestones/{id}` | title, orderIndex, startDate, endDate, status |
| CategoryFormModal | Add/Edit Category | POST `/milestones/{milestoneId}/categories`, PUT `/categories/{id}` | categoryName, orderIndex, ownerDepartmentId |
| TaskFormModal | Add/Edit Task | POST `/categories/{categoryId}/tasks`, PUT `/tasks/{taskId}` | taskName, assigneeId, deptId, priority, deadline, status, note |

### 12.6 Refresh / State Sync Strategy

| Action | Strategy |
|---|---|
| Create milestone | API success → reload milestones list |
| Update milestone | API success → update milestone in tree or reload milestones |
| Delete milestone | API success → remove milestone from tree or reload milestones |
| Create category | API success → append category to milestone or reload categories |
| Update category | API success → update category in tree or reload categories |
| Delete category | API success → remove category from milestone |
| Create task | API success + TaskDto → append to category.tasks |
| Create task but no TaskDto | Do not fake row; mark response shape UNRESOLVED |
| Update task | API success + TaskDto → update task in tree; if incomplete, call GET `/tasks/{taskId}` |
| Update task status | API success → update task status in tree |
| Assign task | API success → update assignee/dept fields in tree; if incomplete, call GET `/tasks/{taskId}` |
| Delete task | API success → remove task from category.tasks |

Rules:

- Source of EventDetail tree state lives in `OrgEventDetailPage` or `useEventDetailTree`.
- Child components emit callbacks; they do not own source-of-truth state.
- Do not call `eventService.getEventById()` alone to reload the full tree because event detail endpoint may not include milestones/categories/tasks.
- Reload the narrowest reliable branch: milestones, categories for a milestone, or task by id.
- Task list is BASE but conditional on actual category response/local mutation.

### 12.7 UI states

| Level | Loading | Empty | Error |
|---|---|---|---|
| Event | Full page spinner | — | Error + retry |
| Milestones | Section spinner | “Chưa có milestone” | Error + retry |
| Categories | Inline spinner | “Chưa có hạng mục” | Error + retry |
| Tasks | Inline spinner if loading known data | “Chưa có task” | Error + retry |

### 12.8 Permission rules

Base prototype uses organization-level permission only:

| Action | Required |
|---|---|
| View EventDetail | member + workspace access |
| Edit event | `org.events.manage` |
| Delete event | `org.events.manage` |
| Change visibility | `org.events.manage` |
| Create/Edit/Delete milestone | `org.events.manage` |
| Create/Edit/Delete category | `org.events.manage` |
| Create/Edit/Delete task | `org.events.manage` |
| Update task status | `org.events.manage` |
| Assign task | `org.events.manage` |

### 12.9 Local event permission override

Local event-level permission is **UNRESOLVED / future extension**.

It is not implemented in base prototype unless API returns explicit fields such as:

- `event.currentUserEventRole`;
- `event.canManage`;
- `event.permissions`.

Rules:

- Do not invent `event.managerId`.
- Do not infer EventRole from missing DTO fields.
- Do not use `Department.ManagerId` for event-level permission unless API explicitly supports it.
- Backend authorization remains final authority.

### 12.10 EventDetail unresolved pieces

| Issue | Status |
|---|---|
| Category response includes `tasks[]`? | UNRESOLVED |
| GET list tasks by category exists? | Not confirmed |
| GET list tasks by org exists? | Not confirmed |
| Task restore UI | Not in base prototype |
| Existing historical tasks if API does not return `tasks[]` | May not render; contract gap |
| Create task response shape | Must verify if TaskDto returned |

---

## 13. BE-FE Mapping Blueprint

### 13.1 Service ownership

| Service | Owns |
|---|---|
| `authService` | login/register/me |
| `userService` | current user, profile, my organizations, my events, change password |
| `organizationService` | org CRUD/default/public overview |
| `roleService` | permissions, roles, role assignment |
| `memberService` | member list/add/remove/department update/leave |
| `departmentService` | department CRUD/manager/members/task overview |
| `eventService` | event CRUD/public events/visibility |
| `milestoneService` | milestone CRUD |
| `categoryService` | category CRUD |
| `taskService` | task create/get/update/delete/status/assign |
| `requestService` | submit/review requests |
| `notificationService` | notification REST |
| `friendService` | friends/friend requests |
| `discoverService` | discover orgs/events |

### 13.2 Core flow mapping

| Flow | Page | Service | Endpoint | Status |
|---|---|---|---|---|
| Login | LoginPage | authService.login | POST `/auth/login` | CONFIRMED |
| Get current user | AuthContext | authService.getMe | GET `/auth/me` | CONFIRMED |
| My organizations | UserOrganizationsPage | userService.getMyOrganizations | GET `/users/me/organizations` | CONFIRMED |
| Org default | OrgContext | organizationService.getDefaultOrganization | GET `/organizations/default` | CONFIRMED |
| Org public overview | OrgOverviewPage | organizationService.getPublicOverview | GET `/organizations/{id}/public-overview` | CONFIRMED endpoint, access semantics verify |
| Org details | OrgOverviewPage | organizationService.getOrganizationById | GET `/organizations/{id}` | CONFIRMED |
| My permissions | OrgContext | roleService.getMyPermissions | GET `/organizations/{id}/permissions/me` | PARTIAL response shape |
| Members list | OrgMembersPage | memberService.getMembers | GET `/organizations/{orgId}/members` | CONFIRMED |
| Departments list | OrgDepartmentsPage | departmentService.getDepartments | GET `/organizations/{orgId}/departments` | CONFIRMED |
| Events list | OrgEventsPage | eventService.getOrgEvents | GET `/organizations/{orgId}/events` | CONFIRMED |
| Event detail | OrgEventDetailPage | eventService.getEventById | GET `/events/{id}` | CONFIRMED |
| Milestones list | OrgEventDetailPage | milestoneService.getMilestones | GET `/events/{eventId}/milestones` | CONFIRMED |
| Categories list | OrgEventDetailPage | categoryService.getCategories | GET `/milestones/{milestoneId}/categories` | CONFIRMED |
| Create task | TaskFormModal | taskService.createTask | POST `/categories/{categoryId}/tasks` | CONFIRMED |
| Update task status | TaskStatusControl | taskService.updateTaskStatus | PUT `/tasks/{taskId}/status` | CONFIRMED |
| Assign task | TaskAssignControl | taskService.assignTask | PUT `/tasks/{taskId}/assign` | CONFIRMED |
| Requests list | OrgRequestsPage | requestService.getRequests | GET `/organizations/{id}/requests` | CONFIRMED |
| Review request | ReviewModal | requestService.reviewRequest | POST `/organizations/requests/{requestId}/review` | CONFIRMED |
| Notifications list | NotificationBadge | notificationService.getNotifications | GET `/notifications` | CONFIRMED |
| Unread count | NotificationBadge | notificationService.getUnreadCount | GET `/notifications/unread-count` | CONFIRMED |
| Roles list | OrgRolesPage | roleService.getRoles | GET `/organizations/{id}/roles` | CONFIRMED |
| Assign role | AssignRoleModal | roleService.assignRoleToMember | POST `/organizations/{id}/members/{memberId}/role` | CONFIRMED endpoint, DTO verify |
| Messages | UserMessagesPage | — | — | PROTOTYPE_ONLY |
| Finance/Reports/Resources | Placeholder pages | — | — | PROTOTYPE_ONLY |

### 13.3 Missing or out-of-scope mapping

| Item | Status | Treatment |
|---|---|---|
| EventRatings UI | MISSING_FRONTEND | Future extension only |
| Restore endpoints | MISSING_FRONTEND | Not in base prototype |
| GET `/users/{id}` profile page | MISSING_FRONTEND | Not in base prototype |
| Posts endpoints | EXCLUDED | No FE |
| Admin migration | EXCLUDED | No FE |
| EventMember/Attendee/DigitalAsset UI | CONTRACT_GAP | No FE |

---

## 14. Auth & Permission Prototype Design

### 14.1 Auth flow

```text
App boot
→ AuthContext.initAuth()
→ read accessToken + expiry
→ if token absent/expired: anonymous state
→ if token exists: GET /auth/me
→ success: set user
→ failure 401: clear token
```

### 14.2 401 and 403

| Status | Handling |
|---|---|
| 401 | Global: clear auth, redirect `/login?returnUrl=` |
| 403 | No global redirect. Let page/guard show `ForbiddenState` or inline forbidden message |

No `/forbidden` route unless explicitly added later.

### 14.3 Org workspace access

`OrgMemberRoute` checks:

- auth;
- `orgId` from query string;
- workspace context loaded;
- member/workspace access confirmed by backend.

If not member:

```text
redirect /org-overview?orgId={orgId}
```

### 14.4 Permission fallback

If `permissions/me` fails or cannot parse:

- permissions = [];
- isMember is not inferred from permissions;
- no workspace/action permission granted;
- page may still render public overview if public overview endpoint succeeds.

### 14.5 Permission gating

- Hide permission-locked actions.
- Do not render disabled buttons for missing permission.
- Disable only for state-based unavailability, not permission.

---

## 15. Action → Permission Matrix

| Area | UI Action | Required Permission | If missing |
|---|---|---|---|
| Org Overview | Edit organization | `org.overview.write` | Hide |
| Org Workspace | Access workspace | backend-confirmed membership / `org.workspace.access` | Redirect public overview |
| Members | Add/remove member | `org.members.manage` | Hide |
| Members | Update member department | `org.members.manage` | Hide |
| Members/Roles | Assign role | `org.roles.assign` | Hide |
| Roles | View roles | `org.roles.view` | Hide nav/page |
| Roles | Create role | `org.roles.create` | Hide |
| Roles | Update role | `org.roles.update` | Hide |
| Roles | Delete role | `org.roles.delete` | Hide |
| Events | Create event | `org.events.create` | Hide |
| Events | Edit/delete/change visibility | `org.events.manage` | Hide |
| Milestones | Create/edit/delete | `org.events.manage` | Hide |
| Categories | Create/edit/delete | `org.events.manage` | Hide |
| Tasks | Create/edit/delete | `org.events.manage` | Hide |
| Tasks | Update status | `org.events.manage` | Disable control or hide control |
| Tasks | Assign task | `org.events.manage` | Disable control or hide control |
| Departments | Create/edit/delete | `org.departments.manage` | Hide |
| Departments | Assign manager | `org.departments.manage` | Hide |
| Requests | View requests | `org.requests.view` | Hide nav/page |
| Requests | Review request | `org.requests.review` | Hide |
| Requests | Approve request | `org.requests.approve` | Hide |

Note:

- Role assignment uses `org.roles.assign`, not `org.members.manage`.
- Update member department uses member/department management, not role assignment.

---

## 16. Page-Level UI State Contract

| Page | Loading | Empty | Error | Forbidden | Actions |
|---|---|---|---|---|---|
| LoginPage | Button spinner | — | Inline error | — | Login |
| RegisterPage | Button spinner | — | Inline error | — | Register |
| UserOrganizationsPage | List spinner | No orgs | ErrorState | — | Create org, select org |
| UserEventsPage | List spinner | No events | ErrorState | — | View detail |
| UserProfilePage | Form spinner | — | ErrorState | — | Save |
| UserSettingsPage | Form spinner | — | ErrorState | — | Change password |
| UserFriendsPage | List spinner | No friends | ErrorState | — | Friend actions |
| UserDiscoverPage | List spinner | No results | ErrorState | — | Browse/join |
| UserMessagesPage | — | Prototype placeholder | — | — | None |
| OrgOverviewPage | Spinner | — | ErrorState | Public fallback if non-member | Edit/request join |
| OrgMembersPage | Table spinner | No members | ErrorState | ForbiddenState | Member actions |
| OrgDepartmentsPage | List spinner | No departments | ErrorState | ForbiddenState | Dept actions |
| OrgEventsPage | List spinner | No events | ErrorState | ForbiddenState | Event actions |
| OrgEventDetailPage | Full spinner | No milestones/categories/tasks states | ErrorState | ForbiddenState | Event tree actions |
| OrgRequestsPage | Table spinner | No requests | ErrorState | ForbiddenState | Review/approve |
| OrgRolesPage | List spinner | No roles | ErrorState | ForbiddenState | Role actions |
| Prototype pages | — | PrototypePlaceholder | — | — | None |
| NotificationBadge | Icon spinner | Hide badge if 0 | Silent/inline error | — | Mark read/clear |

Rules:

- Loading is explicit.
- Empty is not an error.
- API error never crashes the page.
- Modal does not close on API error.
- Prototype pages do not call API.

---

## 17. Extension / Prototype Boundary

### 17.1 BASE includes

- route + page shell + service + adapter + UI states for BASE modules;
- auth init/login/logout;
- org context;
- permission normalizer;
- EventDetail tree;
- notification REST badge/dropdown;
- placeholders for prototype-only modules if visible.

### 17.2 PROTOTYPE_ONLY means

- placeholder page only;
- no fake data;
- no fake success;
- no API call unless explicitly confirmed and allowed;
- no working action;
- text explains the feature is not available.

### 17.3 Out of scope

- Posts/Comments;
- admin migration;
- restore screens;
- public profile page from `GET /users/{id}`;
- EventMember/Attendee/DigitalAsset UI;
- working EventRatings UI;
- working Finance/Reports/Resources/Messages.

---

## 18. Agent Handoff Guardrails

### 18.1 Invariants

1. `VITE_API_BASE_URL` includes `/api`.
2. Service paths do not include `/api`.
3. No fake data.
4. No fake success.
5. No invented endpoints.
6. No invented DTO fields.
7. UNRESOLVED stays UNRESOLVED.
8. No Blazor code copied.
9. No mock import.
10. `orgId` comes from `useSearchParams()`.
11. `useParams()` is only for resource IDs like `eventId`.
12. Permission fallback is `[]`, not `org.workspace.access`.
13. 403 is not a global redirect.
14. EventDetail task list does not invent list endpoints.
15. `/org/tasks` aggregate remains placeholder.
16. Posts/Comments are absent.

### 18.2 Handling missing information

| Situation | Handling |
|---|---|
| Response shape unknown | Adapter/normalizer + safe fallback |
| Missing list endpoint | Do not invent; show only available data |
| Feature has no BE endpoint | PrototypePlaceholder |
| Category has no tasks[] | initialize `tasks: []` |
| Permission parse fails | permissions = [] |
| Event local role missing | do not use local override |
| Create task response lacks TaskDto | do not fake row; mark unresolved |

### 18.3 Service convention

```js
export const eventService = {
  getOrgEvents: (orgId, params) => httpClient.get(`/organizations/${orgId}/events`, { params }),
  getEventById: (id) => httpClient.get(`/events/${id}`),
};
```

No `/api` prefix in service paths.

### 18.4 Adapter convention

```js
export function mapListResponse(response, mapper) {
  const items = response?.data?.items ?? response?.items ?? [];
  const totalCount = response?.data?.totalCount ?? response?.totalCount ?? items.length;
  return { items: items.map(mapper), totalCount };
}
```

Only map confirmed fields. Optional fields stay optional.

### 18.5 Design assumptions

| # | Assumption | Risk |
|---|---|---|
| DA-01 | `permissions/me` shape is one of the normalizer-supported shapes | If not, permissions = [] |
| DA-02 | Category response may not include `tasks[]`; FE initializes `tasks: []` | Existing tasks may not render |
| DA-03 | `orgId` convention is query string | If future route changes, router must update |
| DA-04 | SignalR can attach Bearer token | If not, use REST notifications only |
| DA-05 | ListResponse shape is `{ items, totalCount }` or wrapped under data | Adapter may need runtime tweak |
| DA-06 | Token localStorage key is `org.auth.accessToken` | Auth init fails if key changes |
| DA-07 | `GET /auth/me` returns enough user info | User state mapping may need adapter |
| DA-08 | Non-member can view public overview | If backend blocks it, overview needs auth/member fallback |
| DA-09 | `GET /users/me/organizations` returns orgs user belongs to | UserOrganizations adapter may need runtime tweak |
| DA-10 | No list-by-category/list-by-org tasks endpoint confirmed | Task old data may not render until BE adds list source |
| DA-11 | Role assignment endpoint shape may need runtime verification | AssignRoleModal must be conservative |

---

## 19. Final Base Prototype Readiness Checklist

### 19.1 Infrastructure

- [ ] App boots without crash.
- [ ] React Router nested routes work.
- [ ] `ProtectedRoute` uses `<Outlet />`.
- [ ] `OrgMemberRoute` uses `<Outlet />`.
- [ ] `.env` has `VITE_API_BASE_URL` with `/api`.
- [ ] Service paths do not include `/api`.
- [ ] No hardcoded backend URL in service files.
- [ ] No mock imports.

### 19.2 Auth

- [ ] Login works.
- [ ] Register renders and calls real API.
- [ ] Auth init reads token and calls `/auth/me`.
- [ ] Logout clears token and contexts.
- [ ] 401 redirects login.
- [ ] 403 renders ForbiddenState/inline error, not global redirect.

### 19.3 Org context

- [ ] `orgId` read from `useSearchParams()`.
- [ ] Public overview flow works without workspace permission.
- [ ] Workspace routes require member/workspace access.
- [ ] Permission normalizer works.
- [ ] Permission fallback is `[]`.
- [ ] Actions hidden according to permission matrix.

### 19.4 Core pages

- [ ] LoginPage.
- [ ] RegisterPage.
- [ ] UserOrganizationsPage.
- [ ] UserEventsPage.
- [ ] UserProfilePage.
- [ ] UserSettingsPage.
- [ ] UserFriendsPage.
- [ ] UserDiscoverPage.
- [ ] OrgOverviewPage.
- [ ] OrgMembersPage.
- [ ] OrgDepartmentsPage.
- [ ] OrgEventsPage.
- [ ] OrgEventDetailPage.
- [ ] OrgRequestsPage.
- [ ] OrgRolesPage.
- [ ] NotificationBadge with REST.

### 19.5 EventDetail tree

- [ ] Event detail loads.
- [ ] Milestones load.
- [ ] Categories load per milestone.
- [ ] Every category has `tasks: []`.
- [ ] Existing tasks render if API returns `tasks[]`.
- [ ] Create task appends TaskDto if response returns TaskDto.
- [ ] Update/status/assign task mutates tree state at page/hook level.
- [ ] No list task endpoint invented.
- [ ] No fake task data.
- [ ] `/org/tasks` remains placeholder.
- [ ] Local event role override not implemented unless API supports it.

### 19.6 Service layer

- [ ] All BASE service files exist.
- [ ] All services use centralized httpClient.
- [ ] `userService.getMyOrganizations()` calls `/users/me/organizations`.
- [ ] Role assignment uses `roleService.assignRoleToMember()`.
- [ ] No working eventRating service imported in base pages.
- [ ] No post/comment service exists.

### 19.7 Prototype boundary

- [ ] Messages placeholder only.
- [ ] Finance placeholder only.
- [ ] Reports placeholder only.
- [ ] Resources placeholder only.
- [ ] EventRatings future only.
- [ ] Posts/Comments absent.
- [ ] Restore UI absent.

### 19.8 Adapters

- [ ] Adapter files exist for modules that need DTO mapping.
- [ ] Adapters do not fake fields.
- [ ] Optional fields handled safely.
- [ ] ListResponse mapping handles wrapped/unwrapped shape.

---

## Appendix A. PrototypePlaceholder contract

`PrototypePlaceholder` should render:

- feature name;
- reason why unavailable;
- no action button that simulates success;
- no API call;
- no fake data.

Example text:

```text
Tính năng này chưa khả dụng trong base prototype vì backend contract/API chưa được xác nhận.
```

## Appendix B. OrgTasksPlaceholder contract

Text:

```text
Task board tổng hợp theo tổ chức chưa khả dụng vì chưa có endpoint list-by-org tasks được xác nhận.
Quản lý task hiện nằm trong Sự kiện → Milestone → Hạng mục → Task.
```

No API call. No fake task cards.

---

Blueprint này là thiết kế contract/prototype tự chứa và handoff cho agent dựng base prototype React + Vite, không phải implementation plan.
