# Next Phase Input - Phase 3B Requirements

## Phase 3B: Domain Model & Database Configuration

This document provides the input requirements for Phase 3B.

## Domain Entities to Define

Based on FINAL CLEAN blueprint, define these entities in `Org.Backend/Domain/Entities/`:

### Core Entities
1. **User** - Id, FullName, Email, Status, AvatarUrl, Bio
2. **Organization** - Id, OrgName, Description, AvatarUrl, CoverUrl, Status, Location
3. **Member** - Id, UserId, OrgId, DepartmentId, RoleId, JoinDate
4. **Role** - Id, RoleName, Description, OrgId, IsDefault
5. **Permission** - Id, PermissionKey, DisplayName, ModuleGroup
6. **RolePermission** - RoleId, PermissionId
7. **Department** - Id, OrgId, DeptName, Code, ManagerId, Function

### Event Chain Entities
8. **Event** - Id, OrgId, EventName, StartDate, EndDate, Budget, Location, Status, Visibility
9. **Milestone** - Id, EventId, Title, OrderIndex, StartDate, EndDate, Status
10. **EventCategory** - Id, MilestoneId, CategoryName, OrderIndex, OwnerDepartmentId
11. **OrgTask** - Id, EventCategoryId, TaskName, AssigneeId, DeptId, Priority, Deadline, Status, Note

### Support Entities
12. **Request** - Id, SenderId, OrgId, RequestType, Content, Status
13. **Notification** - Id, ReceiverId, Title, Message, Type, IsRead, ActionUrl
14. **FriendRequest** - Id, SenderId, ReceiverId, Status, RespondedAt

## Enums to Define

In `Org.Backend/Domain/Enums/` and `Org.Shared/Enums/`:
- UserStatus, OrgStatus, EventStatus, EventVisibility
- TaskStatus, TaskPriority, RequestType, RequestStatus
- NotificationType, FriendRequestStatus

## DbContext to Lock

In `Org.Backend/Infrastructure/Persistence/AppDbContext.cs`:
- Add DbSet for each entity
- Configure soft-delete global query filter
- Implement SaveChangesAsync for CreatedAt/UpdatedAt timestamps

## EF Configurations to Add

In `Org.Backend/Infrastructure/Persistence/Configurations/`:
- Create one configuration class per entity
- Define primary keys, relationships, indexes
- Configure column types and constraints

## Relationship Questions to Resolve

Before creating migrations, resolve:
1. Member-User cascade delete behavior
2. Event-Milestone cascade delete behavior
3. Milestone-EventCategory cascade delete behavior
4. EventCategory-OrgTask cascade delete behavior
5. Role-RolePermission cascade delete behavior
6. Soft-delete implementation details (IsDeleted column on all entities?)

## Migration Safety Requirements

- ⚠️ DO NOT run migrations against production database in Phase 3B
- ⚠️ Only create migration files, do not execute `dotnet ef database update`
- ⚠️ Preserve existing production database structure
- ⚠️ Verify migration SQL before any future execution

## User-Secrets Requirements

Connection string and JWT signing key MUST come from:
- User-secrets: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."`
- Environment variables in production
- NEVER hardcoded in appsettings.json or code

## Existing DB/Admin Preservation

- ⚠️ Existing admin user must be preserved
- ⚠️ Existing roles and permissions must be preserved
- ⚠️ Existing organizations and members must be preserved
- ⚠️ Any seed logic must check for existing data before inserting

## DTO Contracts to Define

In `Org.Shared/Features/*/`:
- Define Request/Response DTOs after domain model is locked
- Match entity structure but with API-specific naming
- Include validation attributes
- Reference FINAL CLEAN blueprint for field lists

## Order of Operations

1. Define domain entities with base properties
2. Define enums
3. Create DbContext with DbSets
4. Create EF configurations
5. Review relationships and cascading
6. Create initial migration (DO NOT UPDATE)
7. Define DTO contracts in Org.Shared
8. Review and verify against FINAL CLEAN blueprint

## Source of Truth

All work must reference `PBL3_SYSTEM_DESIGN_AND_PROTOTYPE_HANDOFF_FINAL_CLEAN.md`.
No assumptions - verify against existing backend audit facts when possible.
