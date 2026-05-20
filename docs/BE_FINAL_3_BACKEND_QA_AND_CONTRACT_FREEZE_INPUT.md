# BE_FINAL_3_BACKEND_QA_AND_CONTRACT_FREEZE_INPUT

## Purpose

This document provides guidance for comprehensive QA testing and contract freeze of all backend endpoints after BE-FINAL-2 completion.

---

## Prerequisites

Before starting BE-FINAL-3:
1. ✅ BE-FINAL-2 must be complete (all supporting APIs implemented)
2. ✅ Backend must build successfully (0 errors)
3. ✅ Backend must start successfully
4. ✅ All endpoints must be registered

**Current Status**: ⚠️ BE-FINAL-2 at 90% - FastEndpoints pattern correction needed before proceeding.

---

## Available Backend Modules

### Core Modules (12 modules - Phase 4A-5 complete)

1. **Auth** (3 endpoints)
   - POST /api/auth/login
   - POST /api/auth/register
   - GET /api/auth/me

2. **Users** (6 endpoints)
   - GET /api/users/me
   - PUT /api/users/me
   - PUT /api/users/me/change-password
   - GET /api/users/me/organizations
   - GET /api/users/me/events
   - GET /api/users/me/discover/organizations

3. **Organizations** (6 endpoints)
   - GET /api/organizations
   - POST /api/organizations
   - GET /api/organizations/default
   - GET /api/organizations/{id}
   - PUT /api/organizations/{id}
   - GET /api/organizations/{id}/public-overview

4. **Members** (4 endpoints)
   - GET /api/organizations/{orgId}/members
   - POST /api/organizations/{orgId}/members
   - PUT /api/members/{id}/department
   - DELETE /api/members/{id}

5. **RolesPermissions** (7 endpoints)
   - GET /api/organizations/{orgId}/permissions/me
   - GET /api/organizations/{orgId}/permissions
   - GET /api/organizations/{orgId}/roles
   - POST /api/organizations/{orgId}/roles
   - PUT /api/organizations/roles/{roleId}
   - DELETE /api/organizations/roles/{roleId}
   - POST /api/organizations/{orgId}/members/{memberId}/role

6. **Departments** (5 endpoints)
   - GET /api/organizations/{orgId}/departments
   - POST /api/organizations/{orgId}/departments
   - GET /api/departments/{id}
   - PUT /api/departments/{id}
   - DELETE /api/departments/{id}

7. **Events** (7 endpoints)
   - GET /api/organizations/{orgId}/events
   - POST /api/organizations/{orgId}/events
   - GET /api/events/{id}
   - PUT /api/events/{id}
   - DELETE /api/events/{id}
   - GET /api/events/public
   - GET /api/events/{id}/public

8. **Milestones** (5 endpoints)
   - GET /api/events/{eventId}/milestones
   - POST /api/events/{eventId}/milestones
   - GET /api/milestones/{id}
   - PUT /api/milestones/{id}
   - DELETE /api/milestones/{id}

9. **EventCategories** (5 endpoints)
   - GET /api/milestones/{milestoneId}/categories
   - POST /api/milestones/{milestoneId}/categories
   - GET /api/categories/{id}
   - PUT /api/categories/{id}
   - DELETE /api/categories/{id}

10. **Tasks** (6 endpoints)
    - POST /api/categories/{categoryId}/tasks
    - GET /api/tasks/{taskId}
    - PUT /api/tasks/{taskId}
    - DELETE /api/tasks/{taskId}
    - PUT /api/tasks/{taskId}/status
    - PUT /api/tasks/{taskId}/assign

11. **Requests** (4 endpoints - BE-FINAL-2)
    - GET /api/organizations/{orgId}/requests
    - POST /api/organizations/{orgId}/requests
    - GET /api/requests/{requestId}
    - POST /api/organizations/requests/{requestId}/review

12. **Notifications** (4 endpoints - BE-FINAL-2)
    - GET /api/notifications
    - GET /api/notifications/unread-count
    - POST /api/notifications/{id}/read
    - POST /api/notifications/read-all

### Supporting Modules (2 modules - BE-FINAL-2)

13. **Friends** (5 endpoints)
    - GET /api/friends
    - GET /api/friends/requests
    - POST /api/friends/requests
    - POST /api/friends/requests/{id}/accept
    - POST /api/friends/requests/{id}/reject

14. **Discover** (2 endpoints)
    - GET /api/users/me/discover/organizations (Phase 4A-2A)
    - GET /api/users/me/discover/events (BE-FINAL-2)

**Total Expected Endpoints**: 67 (53 from Phase 4A-5 + 14 from BE-FINAL-2)

---

## Explicitly Excluded Modules

These modules are intentionally NOT implemented and should NOT be tested:

### Hard-Excluded
- ❌ Posts
- ❌ Comments
- ❌ Messages/Chat

### DB Foundation Only (No Working API)
- ❌ EventMembers/Attendees
- ❌ EventRatings
- ❌ DigitalAssets upload
- ❌ ActivityHistory feed

### Prototype Only (No Working Module)
- ❌ Finance
- ❌ Reports (working module)
- ❌ Resources (working module)

---

## Safe QA Strategy

### Principle: Preserve Seed Data

**DO NOT**:
- Delete seeded departments (Technology, Events, Marketing)
- Delete seeded demo event (Annual Tech Summit 2026)
- Remove seeded members (admin + 5 demo users)
- Delete default roles (President, Manager, Member)
- Modify seeded organization
- Delete seeded notifications
- Delete seeded friend requests

**DO**:
- Create temporary test data
- Test with temporary organizations/departments/events/roles/members/requests
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

### 1. Core Modules QA (Phase 4A-5)

Refer to `PHASE_4A5D_FINAL_CORE_WRITE_QA_INPUT.md` for detailed test cases for:
- Users
- Organizations
- RolesPermissions
- Members
- Departments
- Events
- Milestones
- EventCategories
- Tasks

### 2. Supporting Modules QA (BE-FINAL-2)

#### 2.1 Requests Module QA

**Test 2.1.1: List Organization Requests**
- Login as admin
- GET /api/organizations/{orgId}/requests
- Verify returns list of requests
- Verify permission check (org.requests.view)

**Test 2.1.2: Create Request**
- Login as member2 (not in org)
- POST /api/organizations/{orgId}/requests
  ```json
  {
    "requestType": "JoinOrganization",
    "content": "I would like to join this organization"
  }
  ```
- Verify request created
- Verify cannot create duplicate pending request
- Verify cannot create if already active member

**Test 2.1.3: Get Request by ID**
- Login as admin
- GET /api/requests/{requestId}
- Verify returns request details
- Verify permission check (org.requests.view)

**Test 2.1.4: Review Request (Approve)**
- Login as admin
- POST /api/organizations/requests/{requestId}/review
  ```json
  {
    "decision": "Approved",
    "reviewNote": "Welcome to the organization"
  }
  ```
- Verify request status updated to Approved
- Verify member created if JoinOrganization request
- Verify permission check (org.requests.review or org.requests.approve)

**Test 2.1.5: Review Request (Reject)**
- Create another request
- POST /api/organizations/requests/{requestId}/review
  ```json
  {
    "decision": "Rejected",
    "reviewNote": "Not eligible at this time"
  }
  ```
- Verify request status updated to Rejected
- Verify no member created

#### 2.2 Notifications Module QA

**Test 2.2.1: List Notifications**
- Login as admin
- GET /api/notifications
- Verify returns list of notifications
- Verify only returns current user's notifications

**Test 2.2.2: Get Unread Count**
- Login as admin
- GET /api/notifications/unread-count
- Verify returns correct unread count

**Test 2.2.3: Mark Notification Read**
- Login as admin
- POST /api/notifications/{id}/read
- Verify notification marked as read
- Verify readAt timestamp set
- Verify unread count decremented

**Test 2.2.4: Mark All Notifications Read**
- Login as admin
- POST /api/notifications/read-all
- Verify all notifications marked as read
- Verify unread count = 0

#### 2.3 Friends Module QA

**Test 2.3.1: List Friends**
- Login as admin
- GET /api/friends
- Verify returns list of accepted friends
- Verify only returns current user's friends

**Test 2.3.2: List Friend Requests**
- Login as admin
- GET /api/friends/requests
- Verify returns list of pending received requests
- Verify only returns requests where current user is receiver

**Test 2.3.3: Send Friend Request**
- Login as member1
- POST /api/friends/requests
  ```json
  {
    "receiverId": "{member2UserId}"
  }
  ```
- Verify friend request created
- Verify cannot send to self
- Verify cannot send duplicate pending request
- Verify cannot send if already friends

**Test 2.3.4: Accept Friend Request**
- Login as member2
- POST /api/friends/requests/{id}/accept
- Verify friend request status updated to Accepted
- Verify respondedAt timestamp set
- Verify both users now appear in each other's friends list

**Test 2.3.5: Reject Friend Request**
- Create another friend request
- Login as receiver
- POST /api/friends/requests/{id}/reject
- Verify friend request status updated to Rejected
- Verify respondedAt timestamp set
- Verify users do not appear in each other's friends list

#### 2.4 Discover Module QA

**Test 2.4.1: Discover Organizations**
- Login as member1
- GET /api/users/me/discover/organizations
- Verify returns organizations where user is NOT a member
- Verify only returns Active organizations
- Verify ordered by totalMembers descending

**Test 2.4.2: Discover Events**
- Login as member1
- GET /api/users/me/discover/events
- Verify returns public events
- Verify only returns Published or Ongoing events
- Verify ordered by startDate ascending

---

## Permission Testing

### Test Permission Checks

For each endpoint, verify:
1. **401 Unauthorized**: Request without JWT token
2. **403 Forbidden**: Request with JWT but insufficient permissions
3. **200 OK**: Request with JWT and correct permissions

### Permission Matrix

| Module | Endpoint | Required Permission |
|---|---|---|
| Requests | GET /api/organizations/{orgId}/requests | org.requests.view |
| Requests | POST /api/organizations/{orgId}/requests | JWT only |
| Requests | GET /api/requests/{requestId} | org.requests.view |
| Requests | POST /api/organizations/requests/{requestId}/review | org.requests.review or org.requests.approve |
| Notifications | GET /api/notifications | JWT only |
| Notifications | GET /api/notifications/unread-count | JWT only |
| Notifications | POST /api/notifications/{id}/read | JWT only |
| Notifications | POST /api/notifications/read-all | JWT only |
| Friends | GET /api/friends | JWT only |
| Friends | GET /api/friends/requests | JWT only |
| Friends | POST /api/friends/requests | JWT only |
| Friends | POST /api/friends/requests/{id}/accept | JWT only |
| Friends | POST /api/friends/requests/{id}/reject | JWT only |
| Discover | GET /api/users/me/discover/organizations | JWT only |
| Discover | GET /api/users/me/discover/events | JWT only |

---

## Validation Testing

### Test Validation Rules

For each write endpoint, verify:
1. **Required fields**: Request with missing required fields returns 400
2. **Field length**: Request with too long fields returns 400
3. **Field format**: Request with invalid format returns 400
4. **Business rules**: Request violating business rules returns 400

### Validation Test Cases

#### Requests Module
- RequestType: required, must be valid enum value
- Content: required, max 2000 characters
- Title: optional, max 200 characters
- DesiredPosition: optional, max 100 characters
- Decision: must be "Approved" or "Rejected"
- ReviewNote: optional, max 1000 characters

#### Friends Module
- ReceiverId: required, must exist
- Cannot send to self
- Cannot send duplicate pending request
- Cannot send if already friends

---

## Error Handling Testing

### Test Error Responses

For each endpoint, verify:
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

## Contract Freeze Requirements

### What is Contract Freeze?

After BE-FINAL-3 QA is complete, all backend API contracts should be frozen:
- No new endpoints added without explicit approval
- No breaking changes to existing endpoints
- No changes to request/response DTOs
- Frontend can safely integrate against stable contracts

### Contract Documentation

Document for each endpoint:
1. **Route**: Full path with parameters
2. **Method**: GET, POST, PUT, DELETE
3. **Request DTO**: Full schema with field types
4. **Response DTO**: Full schema with field types
5. **Status Codes**: All possible status codes
6. **Permissions**: Required permissions
7. **Validation Rules**: All validation constraints

### Contract Delta Document

Update `PHASE_4_BACKEND_CONTRACT_DELTA.md` with:
- All BE-FINAL-2 endpoint shapes
- Any changes to existing endpoint shapes
- Endpoints FE may safely call
- Endpoints FE must not call (excluded modules)

---

## Known Limitations

### Requests Module
- Auto-member creation on approval is basic implementation
- Only creates member if default "Member" role exists
- Does not handle complex approval workflows
- Does not send notifications (can be added later)

### Notifications Module
- No SignalR real-time updates (REST only)
- No notification creation API (created by system events)
- No notification deletion (can be added later)

### Friends Module
- No friend deletion/unfriend (can be added later)
- No friend blocking (can be added later)
- No friend search (can be added later)

### Discover Module
- No search/filter parameters (can be added later)
- No pagination (can be added later)
- Simple discovery logic (can be enhanced later)

---

## Cleanup Strategy

### After QA Testing

1. **Delete temporary test data**:
   - Delete temporary requests
   - Delete temporary friend requests
   - Delete temporary notifications (if any created)
   - Delete temporary organizations (if deletion implemented)

2. **Verify seed data intact**:
   - 3 default roles (President, Manager, Member)
   - 6 seed users (admin + 5 members)
   - 3 departments (Technology, Events, Marketing)
   - 1 demo event (Annual Tech Summit 2026)
   - 3 seed notifications for admin
   - 2 seed friend requests

3. **Document any issues**:
   - Failed tests
   - Unexpected behavior
   - Performance issues
   - Security concerns

---

## QA Checklist

### Pre-QA
- [ ] BE-FINAL-2 complete (all supporting APIs implemented)
- [ ] Backend builds successfully (0 errors)
- [ ] Backend starts successfully
- [ ] 67 endpoints registered
- [ ] Database seeded with demo data
- [ ] Admin credentials working

### QA Execution - Core Modules
- [ ] Auth module tests passed
- [ ] Users module tests passed
- [ ] Organizations module tests passed
- [ ] RolesPermissions module tests passed
- [ ] Members module tests passed
- [ ] Departments module tests passed
- [ ] Events module tests passed
- [ ] Milestones module tests passed
- [ ] EventCategories module tests passed
- [ ] Tasks module tests passed

### QA Execution - Supporting Modules
- [ ] Requests module tests passed
- [ ] Notifications module tests passed
- [ ] Friends module tests passed
- [ ] Discover module tests passed

### QA Execution - Cross-Cutting
- [ ] Permission tests passed
- [ ] Validation tests passed
- [ ] Error handling tests passed

### Post-QA
- [ ] Temporary test data cleaned up
- [ ] Seed data verified intact
- [ ] Issues documented
- [ ] QA report created
- [ ] Contract delta document updated
- [ ] Contract freeze confirmed

---

## Recommendations

### Immediate Next Steps

1. **Complete BE-FINAL-2**
   - Fix FastEndpoints pattern in all 18 endpoints
   - Build and verify 0 errors
   - Start backend and verify 67 endpoints

2. **Run Comprehensive QA**
   - Test all 67 endpoints
   - Verify all permissions
   - Verify all validations
   - Verify all error handling

3. **Freeze Contracts**
   - Document all endpoint shapes
   - Update contract delta document
   - Communicate freeze to frontend team

4. **Prepare for Frontend Integration**
   - Provide stable API documentation
   - Provide test credentials
   - Provide safe QA strategy
   - Provide known limitations

---

## Success Criteria

BE-FINAL-3 is complete when:
1. ✅ All 67 endpoints tested
2. ✅ All permission checks verified
3. ✅ All validation rules verified
4. ✅ All error handling verified
5. ✅ Temporary test data cleaned up
6. ✅ Seed data verified intact
7. ✅ QA report created
8. ✅ Contract delta document updated
9. ✅ Contract freeze confirmed
10. ✅ Frontend integration can begin

---

**End of BE_FINAL_3_BACKEND_QA_AND_CONTRACT_FREEZE_INPUT.md**
