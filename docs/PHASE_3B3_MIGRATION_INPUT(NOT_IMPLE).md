# Migration Input - Phase 3B.3

## Tổng quan
Tài liệu này cung cấp input và hướng dẫn cho việc tạo và chạy EF Core migrations trong Phase 3B.3.

## Trạng thái hiện tại
- ✅ Domain model đã hoàn thành (BaseEntity, 21 Enums, 22 Entities)
- ✅ EF Core configurations đã hoàn thành (22 configuration classes)
- ✅ AppDbContext đã hoàn thành với DbSets và global query filters
- ✅ Build thành công không lỗi
- ✅ EF Core packages đã cài đặt:
  - Microsoft.EntityFrameworkCore v10.0.7
  - Microsoft.EntityFrameworkCore.Design v10.0.7
  - Microsoft.EntityFrameworkCore.Tools v10.0.7
  - Npgsql.EntityFrameworkCore.PostgreSQL v10.0.1
  - Microsoft.EntityFrameworkCore.Relational v10.0.7

## Yêu cầu Connection String

### Connection String Format (PostgreSQL)
```
Host=localhost;Port=5432;Database=pbl3_db;Username=postgres;Password=your_password
```

### Cấu hình Connection String
**KHÔNG hardcode connection string trong code.** Sử dụng:
1. **Development**: `appsettings.Development.json` hoặc user secrets
2. **Production**: Environment variables hoặc configuration provider

### Ví dụ cấu hình trong appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=pbl3_db;Username=postgres;Password=your_password"
  }
}
```

### Ví dụ cấu hình trong Program.cs
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

## Bước tạo Migration

### 1. Chuẩn bị Database
- Đảm bảo PostgreSQL đang chạy
- Tạo database trống (nếu cần):
```sql
CREATE DATABASE pbl3_db;
```
- Đảm bảo PostgreSQL extension `uuid-ossp` hoặc `pgcrypto` được enable:
```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
-- hoặc
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
```

### 2. Cấu hình Connection String
Thêm connection string vào `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=pbl3_db;Username=postgres;Password=your_password"
  }
}
```

### 3. Cấu hình DbContext trong Program.cs
Thêm vào `Program.cs` (trước `builder.Build()`):
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 4. Tạo Migration Chứa Tên
```bash
cd backend/Org.Backend
dotnet ef migrations add InitialCreate --output-dir Infrastructure/Persistence/Migrations
```

**Lưu ý:**
- Migration name nên descriptive: `InitialCreate`, `AddDomainModel`, v.v.
- Output directory: `Infrastructure/Persistence/Migrations`
- Chạy từ thư mục project (`backend/Org.Backend`)

### 5. Review Migration Code
Kiểm tra file migration được tạo trong `Infrastructure/Persistence/Migrations/`:
- Verify tất cả tables được tạo đúng
- Verify columns, types, constraints
- Verify indexes
- Verify foreign key relationships
- Verify unique constraints

### 6. Áp dụng Migration vào Database
```bash
dotnet ef database update
```

**Lưu ý:**
- Chạy từ thư mục project (`backend/Org.Backend`)
- Database phải tồn tại trước khi chạy
- Connection string phải đúng

## Entity Mapping cho Migration

### Tables sẽ được tạo (22 tables)
1. **Users** - Tài khoản người dùng
2. **Organizations** - Tổ chức
3. **Members** - Membership user-org
4. **Roles** - Role tùy chỉnh
5. **Permissions** - Permission keys
6. **RolePermissions** - Mapping role-permission (join table)
7. **Departments** - Phòng ban
8. **Events** - Sự kiện
9. **EventMembers** - Staff nội bộ event
10. **Attendees** - Người tham dự event
11. **Milestones** - Phân chặng event
12. **EventCategories** - Hạng mục task
13. **OrgTasks** - Tasks
14. **Requests** - Requests join org
15. **Notifications** - Notifications
16. **FriendRequests** - Friend requests
17. **DigitalAssets** - File/asset event
18. **EventRatings** - Rating event
19. **EventReports** - Báo cáo event
20. **Resources** - Tài nguyên org
21. **ActivityHistories** - Feed log

### Columns của BaseEntity (áp dụng cho 21 tables trừ RolePermission)
- `Id` (UUID, PK, default gen_random_uuid())
- `CreatedAt` (timestamp, required, default now() AT TIME ZONE 'UTC')
- `UpdatedAt` (timestamp, nullable)
- `IsDeleted` (boolean, default false)
- `DeletedAt` (timestamp, nullable)

### RolePermission Table (không có BaseEntity columns)
- `RoleId` (UUID, PK part 1)
- `PermissionId` (UUID, PK part 2)

## Indexes sẽ được tạo

### Unique Indexes
1. **Users**: `IX_Users_Email` (Email)
2. **Members**: `IX_Members_UserId_OrgId` (UserId + OrgId)
3. **RolePermissions**: PK (RoleId + PermissionId)
4. **FriendRequests**: `IX_FriendRequests_SenderId_ReceiverId` (SenderId + ReceiverId)
5. **EventReports**: `IX_EventReports_EventId` (EventId)

### Simple Indexes (Performance)
- Status fields: User.Status, Organization.Status, Member.Status, Department.Status, Event.Status, Event.Visibility, Milestone.Status, OrgTask.Status, OrgTask.Priority, Request.Status, Request.Type, Notification.Type, FriendRequest.Status, Resource.Status
- Foreign key fields: Tất cả foreign key columns
- Date fields: Event.StartDate, Event.EndDate, OrgTask.Deadline, Notification.CreatedAt, ActivityHistory.CreatedAt
- Searchable fields: Organization.OrgName, Department.Code (composite với OrgId)

## Constraints sẽ được tạo

### Foreign Key Constraints
- Tất cả foreign key relationships theo delete behavior đã cấu hình
- Restrict/NoAction cho các quan hệ quan trọng
- SetNull cho các quan hệ optional
- Cascade cho RolePermission

### Default Values
- `Id`: gen_random_uuid()
- `CreatedAt`: now() AT TIME ZONE 'UTC'
- `IsDeleted`: false
- `TotalMembers`: 0 (Organization)
- `EmailConfirmed`: false (User)
- `IsDefault`: false (Role)
- `JoinDate`: now() AT TIME ZONE 'UTC' (Member)
- `Status`: Default enum values
- `RegisteredAt`: now() AT TIME ZONE 'UTC' (Attendee)
- `UploadedAt`: now() AT TIME ZONE 'UTC' (DigitalAsset)
- `Quantity`: 0 (Resource)
- `IsPublic`: false (ActivityHistory)

## Data Types Mapping

### PostgreSQL Types
| C# Type | PostgreSQL Type | Notes |
|---------|----------------|-------|
| Guid | uuid | với default gen_random_uuid() |
| string (≤ 500) | varchar(n) | với MaxLength |
| string (> 500) | text | unlimited length |
| int | integer | |
| long | bigint | |
| decimal | numeric(18,2) | cho financial |
| DateTime | timestamp with time zone | UTC |
| DateTime? | timestamp with time zone | nullable |
| bool | boolean | |
| enum | varchar | string conversion |
| List/Array | jsonb | cho Tags, SocialLinks |

## Migration Naming Convention

### Initial Migration
```bash
dotnet ef migrations add InitialCreate
```

### Subsequent Migrations
- Descriptive names:
  - `AddEventMembersAndAttendees`
  - `AddRatingAndReportEntities`
  - `AddResourceAndActivityHistory`
  - `UpdateMemberRoleColumn`
  - `AddDepartmentManagerIndex`

### Best Practices
- Migration name nên mô tả thay đổi
- Một migration nên tập trung vào một thay đổi logic
- Tránh quá nhiều thay đổi trong một migration

## Troubleshooting

### Lỗi: "No database provider has been configured"
**Nguyên nhân**: DbContext không được cấu hình với connection string
**Giải pháp**: Thêm `UseNpgsql` trong Program.cs với connection string

### Lỗi: "relation does not exist"
**Nguyên nhân**: Database chưa được tạo
**Giải pháp**: Tạo database trước khi chạy migration

### Lỗi: "extension "uuid-ossp" does not exist"
**Nguyên nhân**: PostgreSQL extension chưa được enable
**Giải pháp**: Enable extension trong database:
```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

### Lỗi: "could not drop constraint"
**Nguyên nhân**: Có data conflict khi drop constraint
**Giải pháp**: Xóa hoặc update data conflict trước khi drop constraint

### Lỗi: "duplicate key value violates unique constraint"
**Nguyên nhân**: Data vi phạm unique constraint
**Giải pháp**: Xóa hoặc update data conflict

## Rollback Migration

### Rollback một migration
```bash
dotnet ef database update <previous-migration-name>
```

### Rollback về migration cụ thể
```bash
dotnet ef database update 0
```

### Xóa migration gần nhất (chưa apply)
```bash
dotnet ef migrations remove
```

## Seeding Data (Phase sau)

### Seed Data không được tạo trong Phase 3B.3
- Phase 3B.3 chỉ tạo migration, không seed data
- Seed data sẽ được implement trong Phase sau (3C hoặc 3D)

### Seed Data Plan (tương lai)
1. **Default Permissions**: Seed permissions mặc định cho các module
2. **Default Roles**: Seed default roles (Member, Manager, VP, President)
3. **Admin User**: Seed admin user (development only)
4. **Sample Organization**: Seed sample org cho development/testing

## Verification Steps sau Migration

### 1. Verify Tables Created
```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public'
ORDER BY table_name;
```

### 2. Verify Columns
```sql
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'Users'
ORDER BY ordinal_position;
```

### 3. Verify Indexes
```sql
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'Users';
```

### 4. Verify Foreign Keys
```sql
SELECT
    tc.table_name,
    tc.constraint_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY';
```

### 5. Test Basic CRUD
```csharp
// Test insert
var org = new Organization { OrgName = "Test Org" };
context.Organizations.Add(org);
await context.SaveChangesAsync();

// Test query
var orgs = await context.Organizations.ToListAsync();

// Test update
org.Description = "Updated";
await context.SaveChangesAsync();

// Test soft-delete
org.IsDeleted = true;
await context.SaveChangesAsync();
```

## Environment-Specific Notes

### Development Environment
- Sử dụng `appsettings.Development.json`
- Có thể log detailed SQL queries
- Enable detailed error messages

### Production Environment
- Sử dụng environment variables
- Disable detailed error messages
- Sử dụng connection string secure
- Sử dụng managed PostgreSQL service (AWS RDS, Azure Database for PostgreSQL, v.v.)

## Migration Best Practices

### 1. Version Control
- Luôn commit migration files vào git
- Migration files là source of truth cho database schema
- Không modify migration files đã áp dụng (create new migration thay thế)

### 2. Team Collaboration
- Coordinate với team khi merge migrations
- Resolve migration conflicts sớm
- Test migrations trên development database trước khi merge

### 3. Production Deployment
- Test migrations trên staging environment trước
- Backup production database trước khi apply migration
- Có rollback plan
- Monitor database performance sau migration

### 4. Breaking Changes
- Tránh breaking changes trong migrations
- Nếu cần breaking change, tạo data migration script riêng
- Document breaking changes rõ ràng

## Kết quả Mong Đợi sau Phase 3B.3

1. ✅ Migration file được tạo trong `Infrastructure/Persistence/Migrations/`
2. ✅ Database schema được tạo đúng theo DOMAIN_ENTITY_LOCK_V1
3. ✅ Tất cả tables, columns, indexes, constraints được tạo đúng
4. ✅ EF Core có thể kết nối và query database thành công
5. ✅ Sẵn sàng cho Phase 3C (implement business logic và endpoints)

## Lưu ý quan trọng

1. **KHÔNG tạo migration trong Phase 3B.2**: Phase 3B.2 chỉ tạo domain model và EF configurations
2. **KHÔNG run migration trong Phase 3B.2**: Migration sẽ được tạo và run trong Phase 3B.3
3. **Connection string phải được cấu hình đúng**: Sử dụng configuration provider, không hardcode
4. **PostgreSQL extension phải được enable**: `uuid-ossp` hoặc `pgcrypto` cho UUID generation
5. **Database phải tồn tại trước khi apply migration**: Tạo database trước khi chạy `dotnet ef database update`
