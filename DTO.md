# DTO v1 Change Summary (BE Top-Down)

## 1) Bảng tóm tắt thay đổi

| Nhóm | File | Thay đổi chính | Trạng thái |
|---|---|---|---|
| Shared/Common | `src/Org.Shared/Common/ApiContracts.cs` | Tạo `ErrorResponse` (chuẩn lỗi dùng chung). | Done |
| Shared/Departments | `src/Org.Shared/Features/Departments/DepartmentContracts.cs` | Định nghĩa `DepartmentDto`, request create/update, list response chuẩn `{ items: [...] }`. | Done |
| Shared/Members | `src/Org.Shared/Features/Members/MemberContracts.cs` | Định nghĩa `MemberDto`, `MemberRole`, request update role/department, list response `{ items: [...] }`. | Done |
| Shared/Events | `src/Org.Shared/Features/Events/EventContracts.cs` | Định nghĩa `EventDto`, `EventTreeNodeDto`, create/get event, list response `{ items: [...] }`. | Done |
| Shared/Milestones | `src/Org.Shared/Features/Milestones/MilestoneContracts.cs` | Định nghĩa `MilestoneDto`, create milestone, list response `{ items: [...] }`. | Done |
| Shared/EventCategories | `src/Org.Shared/Features/EventCategories/EventCategoryContracts.cs` | Định nghĩa `EventCategoryDto`, create category, list response `{ items: [...] }`. | Done |
| Shared/Tasks | `src/Org.Shared/Features/Tasks/TaskContracts.cs` | Định nghĩa `TaskDto`, `OrgTaskStatus`, create/update status/assign, list response `{ items: [...] }`. | Done |
| Backend/Domain | `src/Org.Backend/Domain/Entities/EventCategory.cs` | Thêm entity `EventCategory : BaseEntity` theo mô hình phẳng theo từng milestone. | Done |
| Backend/Domain | `src/Org.Backend/Domain/Entities/Milestone.cs` | Bổ sung navigation `Categories`, TODO chuyển ownership task theo category. | Done |
| Backend/Domain | `src/Org.Backend/Domain/Entities/OrgTask.cs` | Đồng bộ ownership task theo `EventCategoryId` + navigation `EventCategory`, TODO transition/status policy. | Done |
| Backend/Infra | `src/Org.Backend/Infrastructure/Database/AppDbContext.cs` | Thêm `DbSet<EventCategory>` + mapping quan hệ Milestone-Category-Task. | Done |
| Backend/Migration | `src/Org.Backend/Migrations/20260402103306_AddEventCategoryHierarchy.cs` | Migration thật cho EventCategory + FK Task theo chain Event -> Milestone -> Category -> Task. | Done |
| Backend/Feature TODO | `src/Org.Backend/Features/*/*.Todos.cs` | Tạo skeleton TODO chi tiết cho Departments/Members/Events/Milestones/Categories/Tasks. | Done |

## 2) DTO contracts đã chốt (để FE mock)

### Departments
- `GET /api/organizations/{orgId}/departments`
  - Response: `GetDepartmentsResponse(IReadOnlyList<DepartmentDto> Items)`
- `POST /api/departments`
  - Request: `CreateDepartmentRequest`
  - Response (201): `DepartmentDto`
- `PUT /api/departments/{id}`
  - Request: `UpdateDepartmentRequest`
  - Response (200): `DepartmentDto`

### Members
- `GET /api/organizations/{orgId}/members`
  - Response: `GetMembersResponse(IReadOnlyList<MemberDto> Items)`
- `PUT /api/members/{id}/role`
  - Request: `UpdateMemberRoleRequest`
  - Response (200): `MemberDto`
- `PUT /api/members/{id}/department`
  - Request: `UpdateMemberDepartmentRequest`
  - Response (200): `MemberDto`

### Events + Milestones + Categories
- `GET /api/organizations/{orgId}/events`
  - Response: `GetOrganizationEventsResponse(IReadOnlyList<EventTreeNodeDto> Items)`
- `POST /api/events`
  - Request: `CreateEventRequest`
  - Response (201): `EventDto`
- `GET /api/events/{id}`
  - Response: `GetEventByIdResponse(EventDto Data)`

- `POST /api/events/{eventId}/milestones`
  - Request: `CreateMilestoneRequest`
  - Response (201): `MilestoneDto`
- `GET /api/events/{eventId}/milestones`
  - Response: `GetMilestonesResponse(IReadOnlyList<MilestoneDto> Items)`

- `POST /api/milestones/{milestoneId}/categories`
  - Request: `CreateEventCategoryRequest`
  - Response (201): `EventCategoryDto`
- `GET /api/milestones/{milestoneId}/categories`
  - Response: `GetEventCategoriesResponse(IReadOnlyList<EventCategoryDto> Items)`

### Tasks
- `POST /api/categories/{categoryId}/tasks`
  - Request: `CreateTaskRequest`
  - Response (201): `TaskDto`
- `GET /api/categories/{categoryId}/tasks`
  - Response: `GetTasksResponse(IReadOnlyList<TaskDto> Items)`
- `PUT /api/tasks/{taskId}/status`
  - Request: `UpdateTaskStatusRequest`
  - Response (200): `TaskDto`
- `PUT /api/tasks/{taskId}/assign`
  - Request: `AssignTaskRequest`
  - Response (200): `TaskDto`

## 3) Quy ước response đã áp dụng
- List response: `{ items: [...] }`
- Create response: object DTO tạo mới (201)
- Error response dùng chung (có thể map theo middleware): `ErrorResponse(Code, Message, Details)`

## 4) Ghi chú tiếp theo cho BE implement
1. Wire FastEndpoints cho toàn bộ route theo skeleton TODO trong `src/Org.Backend/Features`.
2. Giữ migration chain sạch: dùng `AddEventCategoryHierarchy`, không dùng placeholder migration.
3. Thêm mapping giữa Domain Entity ↔ DTO (manual mapper hoặc profile).
4. Seed data demo (1 org, 2 departments, 4 members, 1 event tree).
5. Cập nhật tài liệu handoff endpoint cho FE (sample request/response thực tế).
