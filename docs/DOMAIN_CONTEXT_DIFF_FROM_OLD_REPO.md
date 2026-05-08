# Domain Context Diff - So Sánh với Old Repo

## Tổng quan
Tài liệu này so sánh domain model mới (theo DOMAIN_ENTITY_LOCK_V1) với old repo (PBL3-refactor) để làm rõ các thay đổi và quyết định thiết kế.

## Entities đã bao gồm trong New Domain Model

### 1. Core Auth & User Entities
| Entity | New Domain Model | Old Repo | Ghi chú |
|--------|-----------------|----------|---------|
| User | ✅ | ✅ | Đã chuẩn hóa fields: FullName, Email, PasswordHash, PhoneNumber, Dob, Gender, Address, AvatarUrl, Bio, SocialLinks, Status, ProfileVisibility, LastLoginAt, EmailConfirmed |
| Member | ✅ | ✅ | Đã thêm StudentCode, RoleId (thay vì MemberRole persist), navigation đầy đủ |
| FriendRequest | ✅ | ✅ | Đã chuẩn hóa với navigation Sender/Receiver |

### 2. Organization & Department Entities
| Entity | New Domain Model | Old Repo | Ghi chú |
|--------|-----------------|----------|---------|
| Organization | ✅ | ✅ | Đã thêm CreatedByUserId, TotalMembers, navigation đầy đủ |
| Department | ✅ | ✅ | Đã thêm ManagerId, Status (Active/Inactive/Archived), navigation ManagedDepartments |

### 3. Role & Permission Entities
| Entity | New Domain Model | Old Repo | Ghi chú |
|--------|-----------------|----------|---------|
| Role | ✅ | ✅ | Đã thêm IsDefault, Level, navigation RolePermissions |
| Permission | ✅ | ✅ | Đã thêm ModuleGroup, DisplayName, Description |
| RolePermission | ✅ | ✅ | Pure join table, KHÔNG inherit BaseEntity |

### 4. Event Core Entities
| Entity | New Domain Model | Old Repo | Ghi chú |
|--------|-----------------|----------|---------|
| Event | ✅ | ✅ | Đã thêm CreatedByMemberId, AverageRating, navigation đầy đủ |
| EventMember | ✅ | ❌ | Mới - Staff nội bộ của event |
| Attendee | ✅ | ❌ | Mới - Người tham dự/đăng ký event (có thể là User hoặc Guest) |
| Milestone | ✅ | ✅ | Đã thêm navigation đầy đủ với Categories |
| EventCategory | ✅ | ❌ | Mới - Hạng mục trong milestone để chứa task |
| DigitalAsset | ✅ | ❌ | Mới - File/asset của event |

### 5. Task Entities
| Entity | New Domain Model | Old Repo | Ghi chú |
|--------|-----------------|----------|---------|
| OrgTask | ✅ | ✅ | Đã thêm DeptId, CompletedAt, navigation EventCategory (thay vì trực thuộc Event) |

### 6. Request & Notification Entities
| Entity | New Domain Model | Old Repo | Ghi chú |
|--------|-----------------|----------|---------|
| Request | ✅ | ✅ | Đã thêm DesiredDepartmentId, DesiredPosition, navigation đầy đủ |
| Notification | ✅ | ✅ | Đã thêm ActorId, RelatedEntityType/Id, ActionUrl |

### 7. Analytics & Reporting Entities
| Entity | New Domain Model | Old Repo | Ghi chú |
|--------|-----------------|----------|---------|
| EventRating | ✅ | ❌ | Mới - Rating của user cho event theo Aspect |
| EventReport | ✅ | ❌ | Mới - Báo cáo tổng kết event (one-to-one với Event) |
| Resource | ✅ | ❌ | Mới - Tài nguyên của organization, có thể gắn với event |
| ActivityHistory | ✅ | ❌ | Mới - Feed log hoạt động của organization |

## Entities đã loại bỏ (KHÔNG implement)

### 1. Posts & Comments
- **OrganizationPost**: ❌ Loại bỏ - không trong MUST_HAVE_DB_V1
- **PostComment**: ❌ Loại bỏ - không trong MUST_HAVE_DB_V1
- **Lý do**: Không trong scope Phase 3, sẽ implement sau nếu cần

### 2. Messages & Chat
- **ChatThread**: ❌ Loại bỏ - không trong MUST_HAVE_DB_V1
- **Message**: ❌ Loại bỏ - không trong MUST_HAVE_DB_V1
- **Lý do**: Không trong scope Phase 3, sẽ implement sau nếu cần

### 3. Finance Tables
- **FinanceTransaction**: ❌ Loại bỏ - không trong MUST_HAVE_DB_V1
- **FinanceBudget**: ❌ Loại bỏ - không trong MUST_HAVE_DB_V1
- **Lý do**: Finance module không trong scope Phase 3

## Thay đổi chính trong Entity Structure

### 1. Member Entity
**Thay đổi quan trọng:**
- **Old Repo**: Persist `MemberRole` enum trực tiếp
- **New Domain Model**: KHÔNG persist `MemberRole`, dùng `RoleId` (foreign key đến Role entity)
- **Lý do**: Role tùy chỉnh (custom roles) là canonical source, MemberRole chỉ là logic enum cho mapping default

**Fields đã thêm:**
- `StudentCode`: Mã sinh viên (optional)
- Navigation đầy đủ: ManagedDepartments, AssignedTasks, CreatedTasks, EventMemberships, ReviewedRequests, EventReports

### 2. Task Entity Structure
**Thay đổi quan trọng:**
- **Old Repo**: Task trực thuộc Event
- **New Domain Model**: Task thuộc EventCategory, EventCategory thuộc Milestone, Milestone thuộc Event
- **Lý do**: Cấu trúc 3-level (Event → Milestone → Category → Task) để quản lý task theo phân chặng và hạng mục

**Fields đã thêm:**
- `DeptId`: Department được assign task
- `CompletedAt`: Thời điểm hoàn thành
- Navigation: EventCategory (thay vì Event trực tiếp), Department, CreatedByMember

### 3. Event Entity
**Fields đã thêm:**
- `CreatedByMemberId`: Member tạo event
- `AverageRating`: Cache rating trung bình
- Navigation đầy đủ: EventMembers (staff nội bộ), EventReport (one-to-one), Resources

### 4. Attendee Entity (Mới)
**Fields:**
- `EventId`: Event được tham dự
- `UserId`: User tham dự (optional - có thể là guest)
- `GuestName`, `GuestEmail`, `GuestPhone`: Thông tin guest nếu không phải user
- `Status`: Registered, CheckedIn, Cancelled, NoShow, Waitlisted
- `RegisteredAt`, `CheckedInAt`: Timestamps

### 5. EventMember Entity (Mới)
**Fields:**
- `EventId`: Event được assign
- `MemberId`: Member làm staff
- `EventRole`: Manager, CoManager, Staff, Volunteer, Support
- `AssignedAt`, `Note`: Metadata

## Thay đổi trong Index Strategy

### 1. Organization.OrgName
- **Old Repo**: Unique constraint (có thể case-insensitive với citext)
- **New Domain Model**: Simple index (không unique)
- **Lý do**: Tránh citext dependency, case-insensitive uniqueness sẽ được xử lý ở application layer

### 2. Department.Code
- **Old Repo**: Unique constraint trên Code
- **New Domain Model**: Composite index trên (OrgId, Code) (không unique)
- **Lý do**: Code có thể trùng giữa các organization khác nhau

### 3. Member (UserId + OrgId)
- **Old Repo**: Unique constraint
- **New Domain Model**: Unique constraint (giữ nguyên)

### 4. FriendRequest (SenderId + ReceiverId)
- **Old Repo**: Unique constraint
- **New Domain Model**: Unique constraint (giữ nguyên)

## Thay đổi trong Delete Behavior

### Pattern áp dụng:
1. **Restrict/NoAction**: Cho các quan hệ quan trọng không nên xóa cascade
   - User → Member (xóa user không được xóa membership)
   - Organization → Member (xóa org không được xóa membership)
   - Event → Milestone/Category/Task (xóa event không được xóa task con)
   
2. **SetNull**: Cho các quan hệ optional có thể null
   - Department → Manager (xóa department thì managerId = null)
   - Member → Role (xóa role thì roleId = null)
   - Member → Department (xóa department thì departmentId = null)
   
3. **Cascade**: Chỉ cho join tables
   - RolePermission (xóa role thì xóa tất cả role-permission mappings)

## Enum Changes

### 1. MemberRole
- **Old Repo**: Persist trong Member entity
- **New Domain Model**: Logic enum only, không persist trực tiếp
- **Lý do**: RoleId là canonical source

### 2. TaskStatus
- **Vấn đề**: Conflict với System.Threading.Tasks.TaskStatus
- **Giải pháp**: Sử dụng alias `DomainTaskStatus` trong OrgTask entity

## Navigation Properties

### Pattern áp dụng:
- Tất cả navigation properties đều là `virtual` để enable lazy loading (nếu cần)
- Navigation collections được khởi tạo với `new List<T>()` để avoid null reference
- Navigation references để nullable (null) khi cần

## Soft-Delete Implementation

### New Domain Model:
- **BaseEntity**: `IsDeleted`, `DeletedAt`
- **AppDbContext**: Global query filter cho tất cả BaseEntity entities
- **SaveChangesAsync**: Auto-set timestamps (CreatedAt, UpdatedAt)

### Old Repo:
- Có thể có soft-delete nhưng không nhất quán

## Summary

### Entities Added: 6
- EventMember (staff nội bộ event)
- Attendee (người tham dự event)
- EventCategory (hạng mục task)
- DigitalAsset (file/asset event)
- EventRating (rating event)
- EventReport (báo cáo event)
- Resource (tài nguyên org)
- ActivityHistory (feed log)

### Entities Removed: 5
- OrganizationPost
- PostComment
- ChatThread
- Message
- Finance tables (Transaction, Budget)

### Major Structural Changes:
1. Task hierarchy: Event → Milestone → Category → Task (3-level)
2. MemberRole: Không persist, dùng RoleId
3. Attendee: Hỗ trợ guest (không phải user)
4. EventMember: Staff nội bộ riêng biệt với Attendee
5. Soft-delete: Global query filter
6. Index strategy: Simple index thay vì unique cho một số fields

### Compatibility Notes:
- Domain model mới không tương thích ngược với old repo
- Cần migration strategy rõ ràng khi chuyển từ old repo sang new domain model
- Data mapping cần xử lý:
  - MemberRole → Role mapping
  - Task trực thuộc Event → Task qua Category → Milestone → Event
  - Organization.OrgName uniqueness logic move sang application layer
