# PHASE_4A5D_FINAL_CORE_WRITE_QA_INPUT

## Purpose

This document provides guidance for comprehensive QA testing of all Phase 4A-5 core write endpoints.

---

## Available Endpoints

### Phase 4A-5 Core Write APIs (17 endpoints)

#### Users Module (2 endpoints)
1. PUT /api/users/me - Update user profile
2. PUT /api/users/me/change-password - Change password

#### Organizations Module (2 endpoints)
3. POST /api/organizations - Create organization
4. PUT /api/organizations/{id} - Update organization

#### RolesPermissions Module (4 endpoints)
5. POST /api/organizations/{orgId}/roles - Create role
6. PUT /api/organizations/roles/{roleId} - Update role
7. DELETE /api/organizations/roles/{roleId} - Delete role
8. POST /api/organizations/{orgId}/members/{memberId}/role - Assign role to member

#### Members Module (3 endpoints)
9. POST /api/organizations/{orgId}/members - Add member
10. PUT /api/members/{id}/department - Update member department
11. DELETE /api/members/{id} - Remove member

#### Departments Module (3 endpoints)
12. POST /api/organizations/{orgId}/departments - Create department
13. PUT /api/departments/{id} - Update department
14. DELETE /api/departments/{id} - Delete department

#### Events Module (3 endpoints)
15. POST /api/organizations/{orgId}/events - Create event
16. PUT /api/events/{id} - Update event
17. DELETE /api/events/{id} - Delete event

---

## Safe QA Strategy

### Principle: Preserve Seed Data

**DO NOT**:
- Delete seeded departments (Technology, Events, Marketing)
- Delete seeded demo event (Annual Tech Summit 2026)
- Remove seeded members (admin + 5 demo users)
- Delete default roles (President, Manager, Member)
- Modify seeded organization

**DO**:
- Create temporary test data
- Test with temporary organizations/departments/events/roles/members
- Delete only temporary test data
- Use isolated test environment if possible

---

## Test Credentials

### Admin Account
- **Email**: admin@example.com
- **Password**: Admin@123456
- **Role**: President (all permissions)
- **OrgId**: 7e919159-bc23-4cc9-9e49-2b82715ff4b8

### Demo Member Accounts
All use password `User@123456`:
- member1@example.com (John Doe)
- member2@example.com (Jane Smith)
- member3@example.com (Bob Johnson)
- member4@example.com (Alice Williams)
- member5@example.com (Charlie Brown)

---

## QA Test Plan

### 1. Users Module QA

#### Test 1.1: Update Profile
- Login as admin
- Update profile (fullName, phoneNumber, bio)
- Verify response contains updated fields
- Verify profile is updated in database

#### Test 1.2: Change Password
- Login as admin
- Change password with valid current password
- Verify success response
- Login with new password
- Change password back to original

### 2. Organizations Module QA

#### Test 2.1: Create Organization
- Login as admin
- Create temporary organization "QA Test Org"
- Verify organization created with default roles
- Verify current user is President member
- Save orgId for subsequent tests

#### Test 2.2: Update Organization
- Update temporary organization name/description
- Verify response contains updated fields
- Verify organization is updated in database

#### Test 2.3: Cleanup
- Note: Organization deletion not implemented
- Document temporary organization for manual cleanup

### 3. RolesPermissions Module QA

#### Test 3.1: Create Role
- Login as admin
- Create temporary role "QA Test Role" in temporary org
- Assign permissions: org.overview.read, org.workspace.access
- Verify role created with correct permissions
- Save roleId for subsequent tests

#### Test 3.2: Update Role
- Update temporary role name/description/permissions
- Verify response contains updated fields
- Verify role is updated in database

#### Test 3.3: Assign Role to Member
- Create temporary member (or use existing member)
- Assign temporary role to member
- Verify member role updated

#### Test 3.4: Delete Role
- Verify cannot delete default roles (President, Manager, Member)
- Verify cannot delete role with active members
- Remove member from temporary role
- Delete temporary role
- Verify role deleted (soft delete)

### 4. Members Module QA

#### Test 4.1: Add Member
- Login as admin
- Add temporary member to temporary org
- Assign role and department
- Verify member created with correct fields
- Save memberId for subsequent tests

#### Test 4.2: Update Member Department
- Update temporary member department
- Verify response contains updated department
- Verify member department updated in database

#### Test 4.3: Remove Member
- Remove temporary member
- Verify member removed (soft delete, status = Removed)

### 5. Departments Module QA

#### Test 5.1: Create Department
- Login as admin
- Create temporary department "QA Test Department" in temporary org
- Assign manager (optional)
- Verify department created with correct fields
- Save departmentId for subsequent tests

#### Test 5.2: Update Department
- Update temporary department name/description/manager
- Verify response contains updated fields
- Verify department is updated in database

#### Test 5.3: Delete Department
- Verify cannot delete department with active members
- Delete temporary department (no active members)
- Verify department deleted (soft delete, status = Archived)

### 6. Events Module QA

#### Test 6.1: Create Event
- Login as admin
- Create temporary event "QA Test Event" in temporary org
- Set startDate, endDate, location, visibility
- Verify event created with correct fields
- Verify status = Draft
- Save eventId for subsequent tests

#### Test 6.2: Update Event
- Update temporary event name/description/dates/location/visibility
- Verify response contains updated fields
- Verify event is updated in database

#### Test 6.3: Delete Event
- Delete temporary event
- Verify event deleted (soft delete, status = Cancelled)

---

## Permission Testing

### Test Permission Checks

For each write endpoint, verify:
1. **401 Unauthorized**: Request without JWT token
2. **403 Forbidden**: Request with JWT but insufficient permissions
3. **200 OK**: Request with JWT and correct permissions

### Permission Matrix

| Endpoint | Required Permission |
|---|---|
| PUT /api/users/me | JWT only |
| PUT /api/users/me/change-password | JWT only |
| POST /api/organizations | JWT only |
| PUT /api/organizations/{id} | org.overview.write |
| POST /api/organizations/{orgId}/roles | org.roles.create |
| PUT /api/organizations/roles/{roleId} | org.roles.update |
| DELETE /api/organizations/roles/{roleId} | org.roles.delete |
| POST /api/organizations/{orgId}/members/{memberId}/role | org.roles.assign |
| POST /api/organizations/{orgId}/members | org.members.manage |
| PUT /api/members/{id}/department | org.members.manage |
| DELETE /api/members/{id} | org.members.manage |
| POST /api/organizations/{orgId}/departments | org.departments.manage |
| PUT /api/departments/{id} | org.departments.manage |
| DELETE /api/departments/{id} | org.departments.manage |
| POST /api/organizations/{orgId}/events | org.events.create |
| PUT /api/events/{id} | org.events.manage |
| DELETE /api/events/{id} | org.events.manage |

---

## Validation Testing

### Test Validation Rules

For each write endpoint, verify:
1. **Required fields**: Request with missing required fields returns 400
2. **Field length**: Request with too long fields returns 400
3. **Field format**: Request with invalid format returns 400
4. **Business rules**: Request violating business rules returns 400

### Validation Test Cases

#### Users Module
- FullName: required, 2-100 characters
- Password: min 8 characters, must contain uppercase, lowercase, digit

#### Organizations Module
- OrgName: required, 2-100 characters

#### RolesPermissions Module
- RoleName: required, 2-100 characters
- PermissionKeys: must be valid canonical permissions

#### Members Module
- UserId: required, must exist
- RoleId: must belong to same org
- DepartmentId: must belong to same org

#### Departments Module
- DepartmentName: required, 2-100 characters
- ManagerId: must be active member of same org

#### Events Module
- EventName: required, 2-200 characters
- StartDate: required
- EndDate: must be >= StartDate
- Visibility: must be "Public", "OrganizationOnly", or "Private"

---

## Safety Testing

### Test Safety Checks

For each delete endpoint, verify:
1. **Soft delete**: Entity not physically deleted
2. **Cascade protection**: Cannot delete if dependencies exist
3. **Status update**: Status updated correctly (Archived, Cancelled, Removed)

### Safety Test Cases

#### Delete Role
- Cannot delete default roles (IsDefault = true)
- Cannot delete role with active members
- Soft delete: RolePermissions cascade deleted

#### Remove Member
- Soft delete: status = Removed
- Member not physically deleted

#### Delete Department
- Cannot delete department with active members
- Soft delete: status = Archived

#### Delete Event
- Soft delete: status = Cancelled
- Event not physically deleted

---

## Error Handling Testing

### Test Error Responses

For each write endpoint, verify:
1. **404 Not Found**: Request for non-existent resource
2. **400 Bad Request**: Invalid request data
3. **403 Forbidden**: Insufficient permissions
4. **401 Unauthorized**: Missing or invalid JWT
5. **500 Internal Server Error**: Unexpected errors

### Error Response Format

All errors should return ApiResponse<T> with:
```json
{
  "success": false,
  "data": null,
  "message": "Error message",
  "errors": null
}
```

---

## Cleanup Strategy

### After QA Testing

1. **Delete temporary test data**:
   - Delete temporary events
   - Delete temporary departments
   - Delete temporary members
   - Delete temporary roles
   - Delete temporary organizations (if deletion implemented)

2. **Verify seed data intact**:
   - 3 default roles (President, Manager, Member)
   - 6 seed users (admin + 5 members)
   - 3 departments (Technology, Events, Marketing)
   - 1 demo event (Annual Tech Summit 2026)

3. **Document any issues**:
   - Failed tests
   - Unexpected behavior
   - Performance issues
   - Security concerns

---

## QA Checklist

### Pre-QA
- [ ] Backend builds successfully
- [ ] Backend starts successfully
- [ ] 53 endpoints registered
- [ ] Database seeded with demo data
- [ ] Admin credentials working

### QA Execution
- [ ] Users module tests passed
- [ ] Organizations module tests passed
- [ ] RolesPermissions module tests passed
- [ ] Members module tests passed
- [ ] Departments module tests passed
- [ ] Events module tests passed
- [ ] Permission tests passed
- [ ] Validation tests passed
- [ ] Safety tests passed
- [ ] Error handling tests passed

### Post-QA
- [ ] Temporary test data cleaned up
- [ ] Seed data verified intact
- [ ] Issues documented
- [ ] QA report created

---

## Known Limitations

1. **Organization deletion not implemented**: Temporary organizations cannot be deleted
2. **Smoke tests only**: Full integration tests not implemented
3. **Manual testing required**: Automated test suite not available
4. **Seed data preservation**: Some tests cannot be run without risking seed data

---

## Recommendations

1. **Create isolated test environment**: Separate database for QA testing
2. **Implement automated tests**: Integration test suite for all write endpoints
3. **Add organization deletion**: Allow cleanup of temporary organizations
4. **Add test data seeder**: Separate seeder for test data only
5. **Add test user accounts**: Dedicated test users with various permission levels

---

**End of PHASE_4A5D_FINAL_CORE_WRITE_QA_INPUT.md**
