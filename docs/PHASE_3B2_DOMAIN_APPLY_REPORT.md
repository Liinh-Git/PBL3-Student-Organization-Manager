# Báo Cáo Áp Dụng Domain Entity Lock - Phase 3B.2

## Tổng quan
Phase 3B.2 đã hoàn thành việc áp dụng `DOMAIN_ENTITY_LOCK_V1` vào backend PBL3-rescue.

## Công việc đã thực hiện

### 1. BaseEntity
- Đã tạo `BaseEntity` với các trường: `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt`
- Cấu hình EF Core cho BaseEntity với default values và gen_random_uuid()

### 2. Domain Enums (21 enums)
Đã tạo tất cả các enum theo DOMAIN_ENTITY_LOCK_V1.md:
- `UserStatus` (Active, Inactive, Suspended)
- `ProfileVisibility` (Public, OrganizationOnly, Private)
- `OrgStatus` (Active, Suspended, Archived)
- `MemberStatus` (Active, Invited, Suspended, Left, Removed)
- `DepartmentStatus` (Active, Inactive, Archived)
- `MemberRole` (Member, Manager, VicePresident, President) - lưu ý: logic enum, không persist trực tiếp trong Member
- `EventStatus` (Draft, Published, Ongoing, Completed, Cancelled, Archived)
- `EventVisibility` (Public, OrganizationOnly, Private)
- `EventRole` (Manager, CoManager, Staff, Volunteer, Support)
- `AttendeeStatus` (Registered, CheckedIn, Cancelled, NoShow, Waitlisted)
- `MilestoneStatus` (Planned, InProgress, Completed, Archived)
- `TaskStatus` (Todo, InProgress, Blocked, Done, Cancelled) - đã xử lý conflict với System.Threading.Tasks.TaskStatus bằng alias
- `TaskPriority` (Low, Medium, High, Urgent)
- `RequestType` (JoinOrganization, DepartmentChange, RoleChange, EventParticipation, Other)
- `RequestStatus` (Pending, Approved, Rejected, Cancelled, Closed)
- `NotificationType` (System, RequestSubmitted, RequestReviewed, FriendRequest, EventCreated, EventUpdated, EventReminder, TaskAssigned, TaskDue, ResourceChanged)
- `FriendRequestStatus` (Pending, Accepted, Rejected, Cancelled, Blocked)
- `FileType` (Image, Video, Audio, Document, Archive, Link, Other)
- `RatingAspect` (Overall, Content, Logistics, Staff, Experience)
- `ResourceStatus` (Available, Reserved, InUse, Maintenance, Unavailable, Lost)
- `ActivityType` (OrganizationCreated, MemberJoined, MemberLeft, EventCreated, EventUpdated, MilestoneCreated, CategoryCreated, TaskCreated, TaskUpdated, RequestSubmitted, RequestReviewed, NotificationSent, ResourceAdded, ReportGenerated, RoleChanged, DepartmentUpdated)

Tất cả enum được cấu hình lưu trữ dưới dạng string trong database.

### 3. Domain Entities (22 entities)

#### MUST_HAVE_DB_V1 (17 entities)
1. **User** - Tài khoản người dùng với navigation: Members, SentFriendRequests, ReceivedFriendRequests, Notifications, Attendees, EventRatings, UploadedDigitalAssets
2. **Organization** - Aggregate tổ chức với navigation: Members, Departments, Roles, Events, Requests, Resources, ActivityHistories
3. **Member** - Membership user-org với navigation: User, Organization, Department, Role, ManagedDepartments, AssignedTasks, CreatedTasks, EventMemberships, ReviewedRequests, EventReports
4. **Role** - Role tùy chỉnh trong organization với navigation: Organization, Members, RolePermissions
5. **Permission** - Permission key cho authorization với navigation: RolePermissions
6. **RolePermission** - Bảng nối role-permission (KHÔNG inherit BaseEntity)
7. **Department** - Phòng ban với navigation: Organization, Manager, Members, OwnedCategories, AssignedTasks
8. **Event** - Aggregate sự kiện với navigation: Organization, CreatedByMember, Milestones, EventMembers, Attendees, DigitalAssets, EventRatings, EventReport, Resources
9. **EventMember** - Staff nội bộ của event (DB foundation only, no working UI)
10. **Attendee** - Người tham dự/đăng ký event (DB foundation only, no working UI)
11. **Milestone** - Phân chặng event với navigation: Event, Categories
12. **EventCategory** - Hạng mục trong milestone với navigation: Milestone, OwnerDepartment, Tasks
13. **OrgTask** - Task của category với navigation: EventCategory, Assignee, Department, CreatedByMember
14. **Request** - Request join org với navigation: Sender, Organization, DesiredDepartment, ReviewedByMember
15. **Notification** - Notification in-app với navigation: Receiver, Actor
16. **FriendRequest** - Kết bạn với navigation: Sender, Receiver

#### SHOULD_HAVE_DB_V1_NO_WORKING_UI_YET (5 entities)
17. **DigitalAsset** - File/asset của event
18. **EventRating** - Rating của user cho event
19. **EventReport** - Báo cáo tổng kết event (one-to-one với Event)
20. **Resource** - Tài nguyên của organization
21. **ActivityHistory** - Feed log hoạt động org

### 4. AppDbContext
- Đã tạo 22 DbSets cho tất cả entities
- Đã implement `OnModelCreating`:
  - Apply tất cả configurations từ assembly
  - Global query filter cho soft-delete trên BaseEntity entities
- Đã override `SaveChangesAsync`:
  - Set `CreatedAt` = DateTime.UtcNow cho Added
  - Set `UpdatedAt` = DateTime.UtcNow cho Modified

### 5. EF Core Configurations (22 configuration classes)
Đã tạo configuration class cho mỗi entity với:
- Table names (PascalCase: Users, Organizations, Members, Roles, Permissions, RolePermissions, Departments, Events, EventMembers, Attendees, Milestones, EventCategories, OrgTasks, Requests, Notifications, FriendRequests, DigitalAssets, EventRatings, EventReports, Resources, ActivityHistories)
- Primary keys với default value `gen_random_uuid()`
- Required fields và MaxLengths theo DOMAIN_ENTITY_LOCK_V1
- Enum string conversions
- Relationships với delete behaviors:
  - Restrict/NoAction cho các quan hệ quan trọng (User->Member, Org->Member, etc.)
  - SetNull cho các quan hệ optional (Department->Manager, Member->Role, etc.)
  - Cascade cho RolePermission (join table)
- Indexes theo DOMAIN_ENTITY_LOCK_V1:
  - Unique indexes: User.Email, Member(UserId+OrgId), RolePermission(RoleId+PermissionId), Department(OrgId+Code), FriendRequest(SenderId+ReceiverId), EventReport(EventId)
  - Simple indexes: Status fields, foreign key fields, date fields, etc.

### 6. NuGet Packages
Đã thêm các package EF Core:
- `Microsoft.EntityFrameworkCore` v10.0.7
- `Microsoft.EntityFrameworkCore.Design` v10.0.7
- `Microsoft.EntityFrameworkCore.Tools` v10.0.7
- `Npgsql.EntityFrameworkCore.PostgreSQL` v10.0.1
- `Microsoft.EntityFrameworkCore.Relational` v10.0.7

### 7. Build Status
✅ Build thành công không có lỗi

## Quyết định áp dụng

### 1. Organization.OrgName uniqueness
- Quyết định: Simple index (không unique constraint)
- Lý do: Tránh sử dụng citext extension, case-insensitive unique sẽ được xử lý ở application layer

### 2. Department.Code uniqueness
- Quyết định: Composite index trên (OrgId, Code) (không unique constraint)
- Lý do: Code có thể trùng giữa các organization khác nhau

### 3. MemberRole persistence
- Quyết định: KHÔNG persist MemberRole enum trực tiếp trong Member
- Lý do: RoleId là canonical source, MemberRole chỉ là logic enum cho mapping default

## Giới hạn & Lưu ý

1. **Không tạo migrations**: Phase 3B.2 chỉ tạo domain model và EF configurations, không tạo hay chạy migrations
2. **Không update database**: Không thực hiện database update hay seeding
3. **Không implement business logic**: Không có endpoints, services, hay business logic
4. **EventMember & Attendee**: DB foundation only, không có working UI/API trong phase này
5. **TaskStatus conflict**: Đã xử lý conflict với System.Threading.Tasks.TaskStatus bằng alias `DomainTaskStatus`

## Tệp tin đã tạo/modified

### Domain Entities
- `Domain/Entities/BaseEntity.cs`
- `Domain/Entities/User.cs`
- `Domain/Entities/Organization.cs`
- `Domain/Entities/Member.cs`
- `Domain/Entities/Role.cs`
- `Domain/Entities/Permission.cs`
- `Domain/Entities/RolePermission.cs`
- `Domain/Entities/Department.cs`
- `Domain/Entities/Event.cs`
- `Domain/Entities/EventMember.cs`
- `Domain/Entities/Attendee.cs`
- `Domain/Entities/Milestone.cs`
- `Domain/Entities/EventCategory.cs`
- `Domain/Entities/OrgTask.cs`
- `Domain/Entities/Request.cs`
- `Domain/Entities/Notification.cs`
- `Domain/Entities/FriendRequest.cs`
- `Domain/Entities/DigitalAsset.cs`
- `Domain/Entities/EventRating.cs`
- `Domain/Entities/EventReport.cs`
- `Domain/Entities/Resource.cs`
- `Domain/Entities/ActivityHistory.cs`

### Domain Enums
- `Domain/Enums/UserStatus.cs`
- `Domain/Enums/ProfileVisibility.cs`
- `Domain/Enums/OrgStatus.cs`
- `Domain/Enums/MemberStatus.cs`
- `Domain/Enums/DepartmentStatus.cs`
- `Domain/Enums/MemberRole.cs`
- `Domain/Enums/EventStatus.cs`
- `Domain/Enums/EventVisibility.cs`
- `Domain/Enums/EventRole.cs`
- `Domain/Enums/AttendeeStatus.cs`
- `Domain/Enums/MilestoneStatus.cs`
- `Domain/Enums/TaskStatus.cs`
- `Domain/Enums/TaskPriority.cs`
- `Domain/Enums/RequestType.cs`
- `Domain/Enums/RequestStatus.cs`
- `Domain/Enums/NotificationType.cs`
- `Domain/Enums/FriendRequestStatus.cs`
- `Domain/Enums/FileType.cs`
- `Domain/Enums/RatingAspect.cs`
- `Domain/Enums/ResourceStatus.cs`
- `Domain/Enums/ActivityType.cs`

### Infrastructure
- `Infrastructure/Persistence/AppDbContext.cs` (updated)
- `Infrastructure/Persistence/Configurations/BaseEntityConfiguration.cs`
- `Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- `Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs`
- `Infrastructure/Persistence/Configurations/MemberConfiguration.cs`
- `Infrastructure/Persistence/Configurations/RoleConfiguration.cs`
- `Infrastructure/Persistence/Configurations/PermissionConfiguration.cs`
- `Infrastructure/Persistence/Configurations/RolePermissionConfiguration.cs`
- `Infrastructure/Persistence/Configurations/DepartmentConfiguration.cs`
- `Infrastructure/Persistence/Configurations/EventConfiguration.cs`
- `Infrastructure/Persistence/Configurations/EventMemberConfiguration.cs`
- `Infrastructure/Persistence/Configurations/AttendeeConfiguration.cs`
- `Infrastructure/Persistence/Configurations/MilestoneConfiguration.cs`
- `Infrastructure/Persistence/Configurations/EventCategoryConfiguration.cs`
- `Infrastructure/Persistence/Configurations/OrgTaskConfiguration.cs`
- `Infrastructure/Persistence/Configurations/RequestConfiguration.cs`
- `Infrastructure/Persistence/Configurations/NotificationConfiguration.cs`
- `Infrastructure/Persistence/Configurations/FriendRequestConfiguration.cs`
- `Infrastructure/Persistence/Configurations/DigitalAssetConfiguration.cs`
- `Infrastructure/Persistence/Configurations/EventRatingConfiguration.cs`
- `Infrastructure/Persistence/Configurations/EventReportConfiguration.cs`
- `Infrastructure/Persistence/Configurations/ResourceConfiguration.cs`
- `Infrastructure/Persistence/Configurations/ActivityHistoryConfiguration.cs`

### Project
- `Org.Backend.csproj` (updated với EF Core packages)

## Kết quả
✅ Domain model hoàn chỉnh theo DOMAIN_ENTITY_LOCK_V1
✅ EF Core configurations đầy đủ
✅ Build thành công
✅ Sẵn sàng cho Phase 3B.3 (migrations)
