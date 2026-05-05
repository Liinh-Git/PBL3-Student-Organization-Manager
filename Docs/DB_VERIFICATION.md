# Database Verification Report

**Date:** 2026-05-05
**Phase:** Phase 1 Migration + Seed
**Database:** StudentOrgDb (PostgreSQL)
**Migration Applied:** 20260505154524_SyncPendingModelChanges_20260505

---

## Database Connection

**Provider:** PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL)
**Connection String:** Host=localhost;Port=5432;Database=StudentOrgDb;Username=org_admin;Password=***
**Status:** Connected successfully

---

## Migration Status

**Total Migrations:** 7
**Applied Migrations:**
1. 20260328045346_InitialCreate
2. 20260328062942_AddConstraintsAndIndexes
3. 20260402103306_AddEventCategoryHierarchy
4. 20260404060523_AddMilestoneStartEndDateAndDepartmentCode
5. 20260430161649_AddProfileVisibilityAndFriendRequests
6. 20260430170543_AddPostsRatingsAndEventVisibility
7. 20260505154524_SyncPendingModelChanges_20260505 (NEW - adds Notifications table)

---

## Seed Data Verification

**Seed Command:** `dotnet run --project src\Org.Backend\Org.Backend.csproj -- --seed`
**Seed Result:** SUCCESS
**Data Reset:** Yes (seeder uses TRUNCATE TABLE RESTART IDENTITY CASCADE before seeding)

### Table Row Counts

| Table | Row Count | Status |
|-------|-----------|--------|
| Users | 50 | ✓ |
| Organizations | 6 | ✓ |
| Departments | 20 | ✓ |
| Roles | 18 | ✓ |
| Permissions | 20 | ✓ |
| RolePermissions | 30 | ✓ |
| Members | 40 | ✓ |
| Events | 40 | ✓ |
| EventMembers | 45 | ✓ |
| EventReports | 20 | ✓ |
| Milestones | 120 | ✓ |
| EventCategories | 30 | ✓ |
| Tasks | 45 | ✓ |
| Attendees | 60 | ✓ |
| DigitalAssets | 30 | ✓ |
| Requests | 30 | ✓ |
| Resources | 30 | ✓ |
| ActivityHistories | 40 | ✓ |
| Notifications | 0 | ✓ (new table, no seed data yet) |

---

## Demo Account Verification

**Expected Demo Account:**
- Email: example1@gmail.com
- Password: example1

**Source:** DatabaseSeeder.cs (lines 93-120)

**Verification Status:** Pending (will verify via API login test)

---

## Sample Data Preview

### Organizations
- Organization 1 | Campus 1 | Active
- Organization 2 | Campus 2 | Active
- Organization 3 | Campus 3 | Active

### Events
- Event 1 of Org 1 | Planning
- Event 10 of Org 2 | Planning
- Event 11 of Org 2 | Planning

### Event Categories
- Logistics | Order 1
- Logistics | Order 1
- Logistics | Order 1

### Tasks
- Task 1 | Medium | Todo
- Task 10 | Medium | Todo
- Task 11 | Medium | Todo

---

## Mock Data Export

**Export Location:** `src/Org.Frontend/Services/Mocks/Data/`
**Exported Files:**
- users.mock.json
- organizations.mock.json
- departments.mock.json
- members.mock.json
- events.mock.json
- event-members.mock.json
- attendees.mock.json
- milestones.mock.json
- event-categories.mock.json
- tasks.mock.json
- requests.mock.json
- digital-assets.mock.json

**Export Status:** SUCCESS

---

## Database Schema Notes

**New Table Added:** Notifications
- Columns: Id, ReceiverId, Title, Message, Type, IsRead, ActorId, RelatedEntityId, RelatedEntityType, ActionUrl, IconUrl, ReadAt, CreatedAt, UpdatedAt, IsDeleted
- Foreign Keys: ActorId → Users (SetNull), ReceiverId → Users (Cascade)
- Indexes: IX_Notifications_ActorId, IX_Notifications_ReceiverId_CreatedAt, IX_Notifications_ReceiverId_IsRead, IX_Notifications_ReceiverId_Type

**Existing Tables:** All previous tables intact
- No tables dropped
- No columns dropped
- No destructive schema changes

---

## Data Integrity

**Foreign Key Relationships:** All valid
- Members link to Users, Organizations, Departments, Roles
- Events link to Organizations
- Milestones link to Events
- EventCategories link to Milestones
- Tasks link to EventCategories

**Soft Delete:** All tables use IsDeleted filter
- Seed data has IsDeleted = false
- Queries will automatically filter out deleted records

---

## Conclusion

**Database Status:** HEALTHY
**Migration Status:** APPLIED SUCCESSFULLY
**Seed Status:** COMPLETED SUCCESSFULLY
**Data Availability:** Full demo dataset available for testing

**Ready for:** Backend endpoint testing with authenticated requests
