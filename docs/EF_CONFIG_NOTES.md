# EF Core Configuration Notes - Phase 3B.2

## Tổng quan
Tài liệu này ghi chú các quyết định và pattern đã sử dụng trong EF Core configurations cho domain model theo DOMAIN_ENTITY_LOCK_V1.

## Cấu hình chung

### 1. BaseEntity Configuration
- **Table**: Không có table riêng (abstract class)
- **Primary Key**: `Id` với default value `gen_random_uuid()` (PostgreSQL)
- **Timestamps**:
  - `CreatedAt`: Required, default `now() AT TIME ZONE 'UTC'`
  - `UpdatedAt`: Nullable, set trong `SaveChangesAsync`
  - `DeletedAt`: Nullable, set khi soft-delete
- **Soft-Delete**: `IsDeleted` (bool, default false)

### 2. Soft-Delete Query Filter
- **Location**: `AppDbContext.OnModelCreating`
- **Implementation**: Global query filter cho tất cả BaseEntity entities
```csharp
modelBuilder.Entity(entityType.ClrType)
    .HasQueryFilter((BaseEntity e) => !e.IsDeleted);
```
- **Scope**: Tự động áp dụng cho tất cả queries (trừ khi explicitly IgnoreQueryFilter)

### 3. Timestamp Handling
- **Location**: `AppDbContext.SaveChangesAsync`
- **Logic**:
  - Added entities: Set `CreatedAt = DateTime.UtcNow`
  - Modified entities: Set `UpdatedAt = DateTime.UtcNow`

## Table Naming Convention
- **Pattern**: PascalCase (Users, Organizations, Members, Roles, Permissions, RolePermissions, Departments, Events, EventMembers, Attendees, Milestones, EventCategories, OrgTasks, Requests, Notifications, FriendRequests, DigitalAssets, EventRatings, EventReports, Resources, ActivityHistories)
- **Rationale**: Đơn giản, dễ đọc, phù hợp PostgreSQL convention

## Primary Key Configuration
- **Type**: Guid
- **Default Value**: `gen_random_uuid()` (PostgreSQL extension)
- **Rationale**: UUID tốt cho distributed system, không lộ thông tin sequential

## Enum Storage
- **Type**: String (không phải int)
- **Configuration**: `.HasConversion<string>()`
- **Rationale**: 
  - Human-readable trong database
  - Dễ debug
  - Không cần mapping int → enum
  - Giảm rủi ro khi enum value thay đổi

## MaxLength Constraints

### Common Patterns:
- **Names**: 200-300 characters
- **Emails**: 256 characters
- **Phone numbers**: 20 characters
- **URLs**: 500 characters
- **Descriptions**: 1000-2000 characters
- **Codes**: 50 characters
- **Text fields**: 500 characters (notes, short descriptions)

### Specific Examples:
| Field | MaxLength | Entity |
|-------|-----------|--------|
| FullName | 200 | User |
| Email | 256 | User |
| PasswordHash | 512 | User |
| OrgName | 200 | Organization |
| EventName | 300 | Event |
| Description | 2000 | Event |
| Bio | 1000 | User |
| Code | 50 | Department |

## Delete Behavior Strategy

### 1. Restrict/NoAction (Default cho quan hệ quan trọng)
**Sử dụng khi:**
- Xóa parent không được phép xóa children
- Data integrity quan trọng
- Phải xóa/hoặc sửa children trước khi xóa parent

**Ví dụ:**
- User → Member (xóa user không được xóa membership)
- Organization → Member/Department/Role/Event (xóa org không được xóa data con)
- Event → Milestone/EventMember/Attendee (xóa event không được xóa task con)
- Milestone → EventCategory (xóa milestone không được xóa categories)
- EventCategory → OrgTask (xóa category không được xóa tasks)

### 2. SetNull (Cho quan hệ optional)
**Sử dụng khi:**
- Relationship là optional (nullable foreign key)
- Xóa parent thì children vẫn tồn tại nhưng không còn liên kết

**Ví dụ:**
- Department → Manager (xóa department thì managerId = null)
- Member → Role (xóa role thì roleId = null)
- Member → Department (xóa department thì departmentId = null)
- Organization → CreatedByUser (xóa user thì CreatedByUserId = null)
- Event → CreatedByMember (xóa member thì CreatedByMemberId = null)
- Notification → Actor (xóa user thì ActorId = null)
- Attendee → User (xóa user thì UserId = null - guest vẫn tồn tại)

### 3. Cascade (Chỉ cho join tables)
**Sử dụng khi:**
- Pure join table (không có data độc lập)
- Xóa parent thì phải xóa tất cả mappings

**Ví dụ:**
- Role → RolePermission (xóa role thì xóa tất cả role-permission mappings)
- Permission → RolePermission (xóa permission thì xóa tất cả role-permission mappings)

## Index Strategy

### 1. Unique Indexes
| Table | Columns | Rationale |
|-------|---------|-----------|
| Users | Email | Mỗi email chỉ 1 user |
| Members | UserId + OrgId | Mỗi user chỉ 1 membership per org |
| RolePermissions | RoleId + PermissionId | Không trùng mapping |
| Departments | OrgId + Code | Code unique trong scope org (composite index, không unique constraint) |
| FriendRequests | SenderId + ReceiverId | Không gửi request trùng |
| EventReports | EventId | Mỗi event chỉ 1 report |

### 2. Simple Indexes (Performance)
**Status fields:**
- User.Status
- Organization.Status
- Member.Status
- Department.Status
- Event.Status, Event.Visibility
- Milestone.Status
- OrgTask.Status, OrgTask.Priority
- Request.Status, Request.Type
- Notification.Type, Notification.CreatedAt
- FriendRequest.Status
- Resource.Status

**Foreign Key fields:**
- Tất cả foreign key fields (UserId, OrgId, DepartmentId, RoleId, EventId, v.v.)

**Date fields:**
- Event.StartDate, Event.EndDate
- OrgTask.Deadline
- Notification.CreatedAt
- ActivityHistory.CreatedAt

**Searchable fields:**
- Organization.OrgName (simple index, không unique)
- Department.Code (composite với OrgId)

## Column Type Mapping

### Decimal/Money
```csharp
builder.Property(e => e.Budget)
    .HasColumnType("numeric(18,2)")
    .IsRequired(false);
```
- **Type**: numeric(18,2)
- **Rationale**: Chính xác cho financial calculations

### JSONB
```csharp
builder.Property(e => e.Tags)
    .HasColumnType("jsonb");
```
- **Type**: jsonb
- **Rationale**: Flexible JSON storage, hỗ trợ querying trong PostgreSQL

### Text (Long content)
```csharp
builder.Property(e => e.Summary)
    .HasColumnType("text");
```
- **Type**: text
- **Rationale**: Unlimited length cho long descriptions

## Relationship Configuration Patterns

### 1. One-to-Many
```csharp
builder.HasOne(e => e.Organization)
    .WithMany(e => e.Members)
    .HasForeignKey(e => e.OrgId)
    .OnDelete(DeleteBehavior.Restrict);
```

### 2. Many-to-One
```csharp
builder.HasMany(e => e.Members)
    .WithOne(e => e.Department)
    .HasForeignKey(e => e.DepartmentId)
    .OnDelete(DeleteBehavior.SetNull);
```

### 3. One-to-One
```csharp
builder.HasOne(e => e.EventReport)
    .WithOne(e => e.Event)
    .HasForeignKey<EventReport>(e => e.EventId)
    .OnDelete(DeleteBehavior.Restrict);
```

### 4. Many-to-Many (qua join table)
```csharp
// Role entity
builder.HasMany(e => e.RolePermissions)
    .WithOne(e => e.Role)
    .HasForeignKey(e => e.RoleId)
    .OnDelete(DeleteBehavior.Cascade);

// Permission entity
builder.HasMany(e => e.RolePermissions)
    .WithOne(e => e.Permission)
    .HasForeignKey(e => e.PermissionId)
    .OnDelete(DeleteBehavior.Cascade);
```

## Special Cases

### 1. RolePermission (Join Table)
- **KHÔNG inherit BaseEntity**
- **Primary Key**: Composite (RoleId + PermissionId)
- **Delete Behavior**: Cascade từ cả hai phía

### 2. EventReport (One-to-One)
- **One-to-One với Event**
- **Primary Key**: EventId (foreign key同时也是primary key)
- **Index**: Unique trên EventId

### 3. Attendee (User hoặc Guest)
- **UserId**: Nullable (guest không có user)
- **GuestName/GuestEmail/GuestPhone**: Nullable (user không cần guest info)
- **Validation logic**: Phải có ít nhất User hoặc Guest info

### 4. TaskStatus Conflict
- **Vấn đề**: Conflict với System.Threading.Tasks.TaskStatus
- **Giải pháp**: Sử dụng alias trong OrgTask entity
```csharp
using DomainTaskStatus = Org.Backend.Domain.Enums.TaskStatus;

public class OrgTask : BaseEntity
{
    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Todo;
}
```

## PostgreSQL-Specific Configurations

### 1. UUID Generation
```csharp
builder.Property(e => e.Id)
    .HasDefaultValueSql("gen_random_uuid()");
```
- **Yêu cầu**: PostgreSQL extension `uuid-ossp` hoặc `pgcrypto`

### 2. UTC Timestamps
```csharp
builder.Property(e => e.CreatedAt)
    .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");
```
- **Rationale**: Lưu trữ UTC trong database, convert sang local timezone ở application layer

## Configuration Organization

### Folder Structure
```
Infrastructure/Persistence/Configurations/
├── BaseEntityConfiguration.cs
├── UserConfiguration.cs
├── OrganizationConfiguration.cs
├── MemberConfiguration.cs
├── RoleConfiguration.cs
├── PermissionConfiguration.cs
├── RolePermissionConfiguration.cs
├── DepartmentConfiguration.cs
├── EventConfiguration.cs
├── EventMemberConfiguration.cs
├── AttendeeConfiguration.cs
├── MilestoneConfiguration.cs
├── EventCategoryConfiguration.cs
├── OrgTaskConfiguration.cs
├── RequestConfiguration.cs
├── NotificationConfiguration.cs
├── FriendRequestConfiguration.cs
├── DigitalAssetConfiguration.cs
├── EventRatingConfiguration.cs
├── EventReportConfiguration.cs
├── ResourceConfiguration.cs
└── ActivityHistoryConfiguration.cs
```

### Auto-Registration
```csharp
// AppDbContext.OnModelCreating
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
```
- **Rationale**: Tự động discover và apply tất cả configurations trong assembly

## Migration Planning Notes

### 1. PostgreSQL Extensions
Cần đảm bảo các extensions được enable:
- `uuid-ossp` hoặc `pgcrypto` cho `gen_random_uuid()`

### 2. Index Creation Order
- Tạo indexes sau khi tables được tạo
- Unique indexes trước khi insert data
- Composite indexes sau khi single column indexes

### 3. Foreign Key Constraints
- Tạo foreign key constraints sau khi referenced tables được tạo
- Xử lý existing data nếu có (để null hoặc set default values)

### 4. Soft-Delete Migration
- Thêm columns `IsDeleted`, `DeletedAt` vào tất cả BaseEntity tables
- Set default value cho `IsDeleted` = false
- Update existing data (nếu có)

## Performance Considerations

### 1. Index Strategy
- Index trên foreign keys cho JOIN performance
- Index trên status fields cho filtering
- Index trên date fields cho range queries
- Tránh over-indexing (chỉ index fields thực tế được query)

### 2. Query Filter Impact
- Soft-delete filter tự động áp dụng cho tất cả queries
- Có thể disable với `.IgnoreQueryFilter()` khi cần
- Cân nhắc index trên `IsDeleted` nếu nhiều queries lọc deleted records

### 3. Navigation Properties
- Tất cả navigation là `virtual` để enable lazy loading
- Cân nhắc sử dụng eager loading (`.Include()`) để avoid N+1 queries
- Collection navigation được init với `new List<T>()` để avoid null reference

## Testing Notes

### 1. Unit Tests
- Test configuration applied correctly
- Test query filters working
- Test timestamps auto-set
- Test delete behaviors

### 2. Integration Tests
- Test foreign key constraints
- Test unique constraints
- Test cascade deletes
- Test soft-delete behavior

## Known Limitations

### 1. No Partial Indexes
- Trong Phase 3B.2, chỉ sử dụng simple indexes
- Partial indexes (filtered indexes) có thể thêm sau nếu cần

### 2. No Case-Insensitive Unique Constraints
- Organization.OrgName không có unique constraint
- Case-insensitive uniqueness sẽ được xử lý ở application layer

### 3. Department.Code Not Unique
- Composite index trên (OrgId, Code) nhưng không unique constraint
- Validation logic sẽ enforce uniqueness trong scope org

## Future Enhancements

1. **Partial Indexes**: Thêm partial indexes cho queries cụ thể
2. **Covering Indexes**: Thêm INCLUDE columns cho index-only scans
3. **Computed Columns**: Thêm computed columns cho derived values
4. **Index Hints**: Add index hints cho complex queries
5. **Partitioning**: Partition large tables (Events, Notifications) nếu cần
