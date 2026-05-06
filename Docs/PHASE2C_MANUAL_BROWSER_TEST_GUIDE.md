# Phase 2C Manual Browser Test Guide

**Date:** 2026-05-06
**Purpose:** Manual browser click-through verification for TC-01..TC-15
**Environment:**
- Backend: http://localhost:5058
- Frontend: http://localhost:5236
- Mode: Live (FrontendData:UseMockServices=false)
- Demo Account: example1@gmail.com / example1

---

## Prerequisites

1. **Backend must be running:** http://localhost:5058
2. **Frontend must be running:** http://localhost:5236
3. **Browser:** Any modern browser (Chrome, Firefox, Edge)
4. **DevTools:** Open browser DevTools (F12) to monitor Network tab and Console

---

## Test Data

**Demo Account:**
- Email: example1@gmail.com
- Password: example1

**Default Organization:** Organization 1 (id: d35c9b14-e873-47e7-b4f7-e30c6054925a)

---

## TC-01: Login Success

**Objective:** Verify user can successfully login and access authenticated area

**Steps:**
1. Open browser to http://localhost:5236/login
2. Verify login page renders:
   - Email input field visible
   - Password input field visible
   - "Đăng nhập" button visible
   - "Ghi nhớ đăng nhập" checkbox visible
3. Enter email: example1@gmail.com
4. Enter password: example1
5. Click "Đăng nhập" button
6. **Expected Result:**
   - Redirect occurs to authenticated area (e.g., /home or /org/events)
   - No error messages appear
   - User is logged in (user name/email visible in UI)
7. **Network Tab Verification:**
   - POST /api/auth/login should return 200
   - Response should contain accessToken, userId, fullName, email

**Pass Criteria:** Login succeeds, redirect occurs, no errors

---

## TC-02: Auth Me Bootstrap

**Objective:** Verify authentication persists after page refresh

**Steps:**
1. After successful login from TC-01
2. Press F5 to refresh browser
3. **Expected Result:**
   - User remains logged in
   - No redirect to /login
   - User name/email still visible in UI
4. **Network Tab Verification:**
   - GET /api/auth/me should return 200 with user data
5. **Console Verification:**
   - No auth errors in console

**Pass Criteria:** User remains authenticated after refresh, no auth errors

---

## TC-03: Default Organization Context

**Objective:** Verify organization context resolves correctly for org routes

**Steps:**
1. After successful login
2. Navigate to http://localhost:5236/org/events
3. **Expected Result:**
   - Page loads without error
   - Event list displays (may be empty if no events)
   - No "organization context" error messages
4. Navigate to http://localhost:5236/org/members
5. **Expected Result:**
   - Page loads without error
   - Member list displays
   - No org context errors
6. Navigate to http://localhost:5236/org/departments
7. **Expected Result:**
   - Page loads without error
   - Department list displays
   - No org context errors
8. **Network Tab Verification:**
   - GET /api/organizations/default should return 200 with org data

**Pass Criteria:** All three org routes load without org context errors

---

## TC-04: Organization Overview UI

**Objective:** Verify organization overview page renders correctly

**Steps:**
1. After successful login
2. Navigate to organization overview page (e.g., /org-overview?orgId=d35c9b14-e873-47e7-b4f7-e30c6054925a)
3. **Expected Result:**
   - Org name displays
   - Org description displays
   - No null/empty text in key sections
   - No crash or error page
   - Sections render (basic info, departments if present, leadership/highlights if available)
4. **Console Verification:**
   - No JavaScript errors
   - No null reference errors
5. **Network Tab Verification:**
   - GET /api/organizations/{id}/public-overview returns 200
   - GET /api/organizations/{id}/permissions/me returns 200

**Pass Criteria:** Overview page renders with org data, no null sections, no errors

---

## TC-05: Member List UI

**Objective:** Verify member list page renders correctly

**Steps:**
1. After successful login
2. Navigate to http://localhost:5236/org/members
3. **Expected Result:**
   - Member table renders
   - Member rows display with data
   - Role labels/badges render
   - Department filter options render
   - Permission-based action buttons appear (edit, delete) based on user role
   - No crash or error page
4. **Console Verification:**
   - No JavaScript errors
5. **Network Tab Verification:**
   - GET /api/organizations/{orgId}/members returns 200
   - GET /api/organizations/{orgId}/departments returns 200 (for filter)
   - GET /api/organizations/{id}/roles returns 200 (for role management)

**Pass Criteria:** Member list renders with table, roles, departments, no errors

---

## TC-06: Event List Load

**Objective:** Verify event list page renders correctly

**Steps:**
1. After successful login
2. Navigate to http://localhost:5236/org/events
3. **Expected Result:**
   - Event list displays
   - Event cards or table render
   - Event status labels render
   - Filter options (by status) work
   - "Create Event" button visible if user has permission
   - No crash or error page
4. **Console Verification:**
   - No JavaScript errors
5. **Network Tab Verification:**
   - GET /api/organizations/{orgId}/events returns 200

**Pass Criteria:** Event list renders with data, filters work, no errors

---

## TC-07: Event Detail Load

**Objective:** Verify event detail page renders correctly

**Steps:**
1. After successful login
2. Navigate to http://localhost:5236/org/events (event list)
3. Click on an event to view details
4. **Expected Result:**
   - Event detail page loads
   - Event name, description, dates display
   - Milestones tab renders with timeline
   - Categories tab renders
   - Task board tab renders
   - No crash or error page
5. **Console Verification:**
   - No JavaScript errors
6. **Network Tab Verification:**
   - GET /api/events/{id} returns 200
   - GET /api/events/{eventId}/milestones returns 200
   - GET /api/milestones/{milestoneId}/categories returns 200

**Pass Criteria:** Event detail loads with all tabs, no errors

---

## TC-08: Milestone List Load

**Objective:** Verify milestones render in event detail

**Steps:**
1. After successful login
2. Navigate to an event detail page
3. Click on "Timeline" or "Milestones" tab
4. **Expected Result:**
   - Milestone list displays
   - Milestone names, start/end dates render
   - Timeline visualization appears
   - No crash or error page
5. **Network Tab Verification:**
   - GET /api/events/{eventId}/milestones returns 200

**Pass Criteria:** Milestones render with timeline, no errors

---

## TC-09: Category List Load

**Objective:** Verify event categories render in event detail

**Steps:**
1. After successful login
2. Navigate to an event detail page
3. Click on "Categories" tab
4. **Expected Result:**
   - Category list displays
   - Category names render
   - Task counts per category display
   - Lead member info displays if assigned
   - No crash or error page
5. **Network Tab Verification:**
   - GET /api/milestones/{milestoneId}/categories returns 200

**Pass Criteria:** Categories render with task counts, no errors

---

## TC-10: Task Board Load

**Objective:** Verify task board renders correctly

**Steps:**
1. After successful login
2. Navigate to an event detail page
3. Click on a category
4. Click on "Task Board" or navigate to /org/events/{eventId}/tasks/board/{categoryId}
5. **Expected Result:**
   - Task board renders with columns (TODO, IN_PROGRESS, DONE)
   - Task cards display in correct columns
   - Task titles, status, assignees display
   - Drag-and-drop interface appears
   - No crash or error page
6. **Console Verification:**
   - No JavaScript errors
7. **Network Tab Verification:**
   - GET /api/categories/{categoryId}/tasks returns 200

**Pass Criteria:** Task board renders with columns and tasks, no errors

---

## TC-11: Task Status Update

**Objective:** Verify task status can be updated

**Steps:**
1. After successful login
2. Navigate to task board
3. Drag a task from TODO to IN_PROGRESS column
4. **Expected Result:**
   - Task moves to new column
   - Status updates in UI
   - No error message
5. Drag task from IN_PROGRESS to DONE
6. **Expected Result:**
   - Task moves to DONE column
   - Status updates
7. **Network Tab Verification:**
   - PUT /api/tasks/{taskId}/status returns 200

**Pass Criteria:** Task status updates via drag-and-drop, API calls succeed

---

## TC-12: Task Creation

**Objective:** Verify new task can be created

**Steps:**
1. After successful login
2. Navigate to task board
3. Click "Create Task" button
4. Fill in task details:
   - Title: Test Task
   - Description: Test description
   - Assignee (optional)
   - Due date (optional)
5. Click "Save"
6. **Expected Result:**
   - Task appears in task board
   - Task details display correctly
   - No error message
7. **Network Tab Verification:**
   - POST /api/categories/{categoryId}/tasks returns 200 or 201

**Pass Criteria:** Task created successfully, appears in board, no errors

---

## TC-13: Permission Gates

**Objective:** Verify permission-based UI elements work correctly

**Steps:**
1. After successful login as President/Manager role
2. Navigate to /org/members
3. **Expected Result:**
   - "Add Member" button visible
   - "Edit" buttons on member rows visible
   - "Delete" buttons on member rows visible
   - Role assignment dialog accessible
4. Navigate to /org/events
5. **Expected Result:**
   - "Create Event" button visible
6. Navigate to task board
7. **Expected Result:**
   - "Create Task" button visible
8. **Network Tab Verification:**
   - GET /api/organizations/{id}/permissions/me returns correct permission flags

**Pass Criteria:** Permission-based buttons/actions appear based on user role

---

## TC-14: Notification Badge

**Objective:** Verify notification badge works

**Steps:**
1. After successful login
2. Look at notification bell icon in header
3. **Expected Result:**
   - Notification badge displays unread count
   - Clicking bell opens notification popover
   - Notification list displays
   - "Mark as read" buttons work
4. **Network Tab Verification:**
   - GET /api/notifications returns 200
   - GET /api/notifications/unread-count returns 200

**Pass Criteria:** Notification badge displays count, popover opens, notifications render

---

## TC-15: Request List

**Objective:** Verify organization requests page works

**Steps:**
1. After successful login as President/Manager
2. Navigate to /org/requests
3. **Expected Result:**
   - Request list displays
   - Request details (requester, title, message) render
   - Approve/Reject buttons visible
   - Status filters work
   - No crash or error page
4. **Network Tab Verification:**
   - GET /api/organizations/{id}/requests returns 200
   - POST /api/organizations/requests/{requestId}/review returns 200 on approve/reject

**Pass Criteria:** Request list renders, approve/reject actions work, no errors

---

## Troubleshooting

**Issue: 500 error on org routes**
- Check Backend is running on port 5058
- Check Frontend is running on port 5236
- Verify user is logged in
- Check Network tab for API call failures

**Issue: Redirect to login loop**
- Clear browser cookies
- Check if auth token is valid
- Verify /api/auth/me returns 200

**Issue: Null/empty data in UI**
- Check if database has seeded data
- Verify API responses contain data
- Check Network tab for API call failures

---

## Test Summary Checklist

After completing manual testing, record results:

- TC-01 Login Success: [ ] PASS [ ] FAIL
- TC-02 Auth Me Bootstrap: [ ] PASS [ ] FAIL
- TC-03 Default Organization Context: [ ] PASS [ ] FAIL
- TC-04 Organization Overview UI: [ ] PASS [ ] FAIL
- TC-05 Member List UI: [ ] PASS [ ] FAIL
- TC-06 Event List Load: [ ] PASS [ ] FAIL
- TC-07 Event Detail Load: [ ] PASS [ ] FAIL
- TC-08 Milestone List Load: [ ] PASS [ ] FAIL
- TC-09 Category List Load: [ ] PASS [ ] FAIL
- TC-10 Task Board Load: [ ] PASS [ ] FAIL
- TC-11 Task Status Update: [ ] PASS [ ] FAIL
- TC-12 Task Creation: [ ] PASS [ ] FAIL
- TC-13 Permission Gates: [ ] PASS [ ] FAIL
- TC-14 Notification Badge: [ ] PASS [ ] FAIL
- TC-15 Request List: [ ] PASS [ ] FAIL

**Total Passed:** ___/15
**Total Failed:** ___/15
