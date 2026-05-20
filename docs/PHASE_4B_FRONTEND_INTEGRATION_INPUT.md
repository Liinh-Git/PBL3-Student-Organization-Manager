# PHASE_4B_FRONTEND_INTEGRATION_INPUT

## Purpose

Input document for Phase 4B: Frontend Integration for EventDetail Tree (Milestones + EventCategories + Tasks).

---

## Phase 4A-4 Completion Status

✅ **COMPLETE** - 16 endpoints for Milestones, EventCategories, and Tasks implemented and backend started successfully.

### What Was Delivered

**Milestones Module (5 endpoints)**:
- GET /api/events/{eventId}/milestones
- POST /api/events/{eventId}/milestones
- GET /api/milestones/{id}
- PUT /api/milestones/{id}
- DELETE /api/milestones/{id}

**EventCategories Module (5 endpoints)**:
- GET /api/milestones/{milestoneId}/categories
- POST /api/milestones/{milestoneId}/categories
- GET /api/categories/{id}
- PUT /api/categories/{id}
- DELETE /api/categories/{id}

**Tasks Module (6 endpoints)**:
- POST /api/categories/{categoryId}/tasks
- GET /api/tasks/{taskId}
- PUT /api/tasks/{taskId}
- DELETE /api/tasks/{taskId}
- PUT /api/tasks/{taskId}/status
- PUT /api/tasks/{taskId}/assign

---

## Complete Backend API Reference

### Auth Endpoints (Phase 4A-1)

#### POST /api/auth/login
**Request**:
```json
{
  "email": "string",
  "password": "string"
}
```

**Response**: `ApiResponse<AuthTokenResponse>`
```json
{
  "success": true,
  "data": {
    "accessToken": "string",
    "tokenType": "Bearer",
    "expiresAtUtc": "datetime",
    "user": {
      "id": "guid",
      "fullName": "string",
      "email": "string",
      "status": "Active",
      "avatarUrl": "string?",
      "lastLoginAtUtc": "datetime"
    }
  }
}
```

#### POST /api/auth/register
**Request**:
```json
{
  "fullName": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string?"
}
```

**Response**: `ApiResponse<AuthTokenResponse>` (same as login)

#### GET /api/auth/me
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<CurrentUserResponse>`
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "guid",
      "fullName": "string",
      "email": "string",
      "status": "Active",
      "avatarUrl": "string?",
      "lastLoginAtUtc": "datetime"
    }
  }
}
```

---

### Users Endpoints (Phase 4A-2A)

#### GET /api/users/me
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<UserProfileDto>`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "fullName": "string",
    "email": "string",
    "phoneNumber": "string?",
    "avatarUrl": "string?",
    "bio": "string?",
    "status": "Active",
    "profileVisibility": "Public",
    "lastLoginAtUtc": "datetime"
  }
}
```

#### GET /api/users/me/organizations
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<MyOrganizationDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "string",
      "description": "string?",
      "avatarUrl": "string?",
      "coverUrl": "string?",
      "roleId": "guid",
      "roleName": "string",
      "memberId": "guid",
      "joinedAtUtc": "datetime",
      "isDefault": "bool?"
    }
  ]
}
```

#### GET /api/users/me/events
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<MyEventDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "organizationId": "guid",
      "organizationName": "string",
      "name": "string",
      "description": "string?",
      "startDate": "datetime",
      "endDate": "datetime",
      "status": "Published",
      "visibility": "Public",
      "location": "string?"
    }
  ]
}
```

---

### Organizations Endpoints (Phase 4A-2A)

#### GET /api/organizations
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<OrganizationSummaryDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "string",
      "description": "string?",
      "avatarUrl": "string?",
      "totalMembers": "int",
      "status": "Active"
    }
  ]
}
```

#### GET /api/organizations/default
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<OrganizationDto>`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "string",
    "description": "string?",
    "avatarUrl": "string?",
    "coverUrl": "string?",
    "foundingDate": "datetime?",
    "location": "string?",
    "contactEmail": "string?",
    "contactPhone": "string?",
    "totalMembers": "int",
    "status": "Active",
    "createdAtUtc": "datetime",
    "updatedAtUtc": "datetime"
  }
}
```

#### GET /api/organizations/{id}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<OrganizationDto>` (same as above)

#### GET /api/organizations/{orgId}/permissions/me
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<MyPermissionsResponse>`
```json
{
  "success": true,
  "data": {
    "permissionKeys": ["org.overview.read", "org.events.manage", ...],
    "roleId": "guid",
    "roleName": "string",
    "memberId": "guid",
    "organizationId": "guid"
  }
}
```

#### GET /api/organizations/{orgId}/roles
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<RoleDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "organizationId": "guid",
      "roleName": "string",
      "description": "string?",
      "level": "int",
      "isDefault": "bool",
      "permissionKeys": ["org.overview.read", "org.events.manage", ...]
    }
  ]
}
```

---

### Members Endpoints (Phase 4A-3)

#### GET /api/organizations/{orgId}/members
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<MemberDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "organizationId": "guid",
      "userId": "guid",
      "departmentId": "guid?",
      "departmentName": "string?",
      "roleId": "guid?",
      "roleName": "string?",
      "studentCode": "string?",
      "fullName": "string",
      "email": "string",
      "avatarUrl": "string?",
      "status": "Active",
      "joinedAtUtc": "datetime"
    }
  ]
}
```

---

### Departments Endpoints (Phase 4A-3)

#### GET /api/organizations/{orgId}/departments
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<DepartmentDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "organizationId": "guid",
      "deptName": "string",
      "code": "string?",
      "function": "string?",
      "managerId": "guid?",
      "managerName": "string?",
      "memberCount": "int",
      "status": "Active",
      "createdAtUtc": "datetime",
      "updatedAtUtc": "datetime"
    }
  ]
}
```

#### GET /api/departments/{id}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<DepartmentDto>` (same as above)

---

### Events Endpoints (Phase 4A-3)

#### GET /api/organizations/{orgId}/events
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<EventSummaryDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "organizationId": "guid",
      "name": "string",
      "description": "string?",
      "startDate": "datetime",
      "endDate": "datetime",
      "status": "Published",
      "visibility": "Public",
      "location": "string?"
    }
  ]
}
```

#### GET /api/events/{id}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<EventDto>`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "organizationId": "guid",
    "name": "string",
    "description": "string?",
    "startDate": "datetime",
    "endDate": "datetime",
    "status": "Published",
    "visibility": "Public",
    "location": "string?",
    "targetParticipants": "int?",
    "budget": "decimal?",
    "averageRating": "double?",
    "tags": "string?",
    "createdAtUtc": "datetime",
    "updatedAtUtc": "datetime"
  }
}
```

#### GET /api/events/public
**No Auth Required**

**Response**: `ApiResponse<List<EventPublicDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "organizationId": "guid",
      "organizationName": "string",
      "name": "string",
      "description": "string?",
      "startDate": "datetime",
      "endDate": "datetime",
      "location": "string?",
      "visibility": "Public",
      "status": "Published"
    }
  ]
}
```

---

### Milestones Endpoints (Phase 4A-4)

#### GET /api/events/{eventId}/milestones
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<MilestoneDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "eventId": "guid",
      "title": "string",
      "description": "string?",
      "startDate": "datetime?",
      "endDate": "datetime?",
      "status": "Planned|InProgress|Completed|Archived",
      "orderIndex": "int",
      "createdAtUtc": "datetime",
      "updatedAtUtc": "datetime?"
    }
  ]
}
```

#### POST /api/events/{eventId}/milestones
**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "title": "string",
  "description": "string?",
  "startDate": "datetime?",
  "endDate": "datetime?",
  "orderIndex": "int?"
}
```

**Response**: `ApiResponse<MilestoneDto>` (same as above)

#### GET /api/milestones/{id}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<MilestoneDto>` (same as above)

#### PUT /api/milestones/{id}
**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "title": "string",
  "description": "string?",
  "startDate": "datetime?",
  "endDate": "datetime?",
  "status": "Planned|InProgress|Completed|Archived",
  "orderIndex": "int?"
}
```

**Response**: `ApiResponse<MilestoneDto>` (same as above)

#### DELETE /api/milestones/{id}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<bool>`
```json
{
  "success": true,
  "data": true,
  "message": "Milestone deleted successfully"
}
```

**Error (if has categories)**:
```json
{
  "success": false,
  "data": null,
  "message": "Cannot delete milestone with existing categories"
}
```

---

### EventCategories Endpoints (Phase 4A-4)

#### GET /api/milestones/{milestoneId}/categories
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<List<EventCategoryDto>>`
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "milestoneId": "guid",
      "categoryName": "string",
      "description": "string?",
      "ownerDepartmentId": "guid?",
      "ownerDepartmentName": "string?",
      "orderIndex": "int",
      "createdAtUtc": "datetime",
      "updatedAtUtc": "datetime?",
      "tasks": [
        {
          "id": "guid",
          "eventCategoryId": "guid",
          "taskName": "string",
          "description": "string?",
          "assigneeId": "guid?",
          "assigneeName": "string?",
          "deptId": "guid?",
          "deptName": "string?",
          "createdByMemberId": "guid?",
          "createdByMemberName": "string?",
          "deadline": "datetime?",
          "priority": "Low|Medium|High|Urgent",
          "status": "Todo|InProgress|Blocked|Done|Cancelled",
          "orderIndex": "int?",
          "note": "string?",
          "completedAt": "datetime?",
          "createdAtUtc": "datetime",
          "updatedAtUtc": "datetime?"
        }
      ]
    }
  ]
}
```

#### POST /api/milestones/{milestoneId}/categories
**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "categoryName": "string",
  "description": "string?",
  "ownerDepartmentId": "guid?",
  "orderIndex": "int?"
}
```

**Response**: `ApiResponse<EventCategoryDto>` (same as above)

#### GET /api/categories/{id}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<EventCategoryDto>` (same as above)

#### PUT /api/categories/{id}
**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "categoryName": "string",
  "description": "string?",
  "ownerDepartmentId": "guid?",
  "orderIndex": "int?"
}
```

**Response**: `ApiResponse<EventCategoryDto>` (same as above)

#### DELETE /api/categories/{id}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<bool>`
```json
{
  "success": true,
  "data": true,
  "message": "Category deleted successfully"
}
```

**Error (if has tasks)**:
```json
{
  "success": false,
  "data": null,
  "message": "Cannot delete category with existing tasks"
}
```

---

### Tasks Endpoints (Phase 4A-4)

#### POST /api/categories/{categoryId}/tasks
**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "taskName": "string",
  "description": "string?",
  "assigneeId": "guid?",
  "deptId": "guid?",
  "deadline": "datetime?",
  "priority": "Low|Medium|High|Urgent",
  "orderIndex": "int?",
  "note": "string?"
}
```

**Response**: `ApiResponse<TaskDto>`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "eventCategoryId": "guid",
    "taskName": "string",
    "description": "string?",
    "assigneeId": "guid?",
    "assigneeName": "string?",
    "deptId": "guid?",
    "deptName": "string?",
    "createdByMemberId": "guid?",
    "createdByMemberName": "string?",
    "deadline": "datetime?",
    "priority": "Low|Medium|High|Urgent",
    "status": "Todo|InProgress|Blocked|Done|Cancelled",
    "orderIndex": "int?",
    "note": "string?",
    "completedAt": "datetime?",
    "createdAtUtc": "datetime",
    "updatedAtUtc": "datetime?"
  }
}
```

#### GET /api/tasks/{taskId}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<TaskDto>` (same as above)

#### PUT /api/tasks/{taskId}
**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "taskName": "string",
  "description": "string?",
  "assigneeId": "guid?",
  "deptId": "guid?",
  "deadline": "datetime?",
  "priority": "Low|Medium|High|Urgent",
  "status": "Todo|InProgress|Blocked|Done|Cancelled",
  "orderIndex": "int?",
  "note": "string?"
}
```

**Response**: `ApiResponse<TaskDto>` (same as above)

#### DELETE /api/tasks/{taskId}
**Headers**: `Authorization: Bearer {token}`

**Response**: `ApiResponse<bool>`
```json
{
  "success": true,
  "data": true,
  "message": "Task deleted successfully"
}
```

#### PUT /api/tasks/{taskId}/status
**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "status": "Todo|InProgress|Blocked|Done|Cancelled"
}
```

**Response**: `ApiResponse<TaskDto>` (same as above)

#### PUT /api/tasks/{taskId}/assign
**Headers**: `Authorization: Bearer {token}`

**Request**:
```json
{
  "assigneeId": "guid?",
  "deptId": "guid?"
}
```

**Response**: `ApiResponse<TaskDto>` (same as above)

---

## Frontend Integration Order

### Phase 1: Auth Integration (Already Done?)
1. Implement authService.js with login/register/getCurrentUser
2. Implement userAdapter.js for DTO transformation
3. Implement LoginPage and RegisterPage
4. Add JWT token storage and httpClient interceptor
5. Add authentication context/provider

### Phase 2: Organization Workspace Integration
1. Implement organizationService.js
2. Implement userService.js
3. Implement OrganizationSelector component
4. Implement UserProfile page
5. Test organization selection flow

### Phase 3: Events List Integration
1. Implement eventService.js
2. Implement EventsList page
3. Implement EventCard component
4. Test events list loading

### Phase 4: EventDetail Tree Integration (NEW)
1. Implement milestoneService.js
2. Implement categoryService.js
3. Implement taskService.js
4. Implement EventDetail page with tree state management
5. Implement Milestone CRUD components
6. Implement Category CRUD components
7. Implement Task CRUD components
8. Implement Task status update UI
9. Implement Task assignment UI
10. Test full EventDetail tree flow

---

## Auth Token Shape

### JWT Token Claims
- `sub` (Subject): User ID (Guid)
- `email`: User email address
- `name`: User full name
- `jti` (JWT ID): Unique token identifier
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`: User ID (for compatibility)

### Token Storage
- Store `accessToken` in localStorage or secure storage
- Store `expiresAtUtc` to check expiration
- Clear token on logout or expiration

### Authorization Header
```
Authorization: Bearer {accessToken}
```

---

## Permissions/Me Shape

### MyPermissionsResponse
```json
{
  "permissionKeys": ["org.overview.read", "org.events.manage", ...],
  "roleId": "guid",
  "roleName": "string",
  "memberId": "guid",
  "organizationId": "guid"
}
```

### Canonical Permission Keys
- `org.overview.read` - Read organization overview
- `org.overview.write` - Write organization overview
- `org.workspace.access` - Access organization workspace
- `org.members.manage` - Manage members
- `org.roles.view` - View roles
- `org.roles.create` - Create roles
- `org.roles.update` - Update roles
- `org.roles.delete` - Delete roles
- `org.roles.assign` - Assign roles
- `org.events.create` - Create events
- `org.events.manage` - Manage events (REQUIRED for EventDetail tree write operations)
- `org.departments.manage` - Manage departments
- `org.requests.view` - View requests
- `org.requests.review` - Review requests
- `org.requests.approve` - Approve requests

---

## OrgId Source Rule

### How to Get OrgId

1. **From GET /api/users/me/organizations**:
   - Returns list of organizations user is member of
   - Each organization has `id` field
   - Use first organization or let user select

2. **From Organization Selector**:
   - User selects organization from dropdown
   - Store selected orgId in context/state

3. **From Event**:
   - Event has `organizationId` field
   - Use this for event-specific operations

---

## Event Detail Tree Loading Flow

### Full Tree Loading Strategy

```javascript
async function loadEventDetailTree(eventId) {
  // 1. Load event details
  const event = await getEventById(eventId);
  
  // 2. Load milestones
  const milestones = await getEventMilestones(eventId);
  
  // 3. Load categories with tasks for each milestone
  const milestonesWithCategories = await Promise.all(
    milestones.map(async (milestone) => {
      const categories = await getMilestoneCategories(milestone.id);
      
      // Categories already include tasks[] from backend
      // If tasks is null, initialize to empty array
      const categoriesWithTasks = categories.map(category => ({
        ...category,
        tasks: category.tasks || []
      }));
      
      return {
        ...milestone,
        categories: categoriesWithTasks
      };
    })
  );
  
  return {
    event,
    milestones: milestonesWithCategories
  };
}
```

### State Management

**Page-level state**:
```javascript
const [event, setEvent] = useState(null);
const [milestones, setMilestones] = useState([]);
const [loading, setLoading] = useState(true);
```

**Tree structure**:
```
event: {
  id, name, description, ...
}
milestones: [
  {
    id, title, description, status, ...
    categories: [
      {
        id, categoryName, description, ...
        tasks: [
          {
            id, taskName, description, status, assigneeId, ...
          }
        ]
      }
    ]
  }
]
```

---

## Category tasks[] Handling

### Backend Behavior
- GET /api/milestones/{milestoneId}/categories returns categories with `tasks[]` populated
- GET /api/categories/{id} returns category with `tasks[]` populated
- If no tasks exist, `tasks[]` is empty array
- If tasks exist, `tasks[]` contains full TaskDto objects

### Frontend Handling
```javascript
// When loading categories
const categories = await getMilestoneCategories(milestoneId);

// Initialize tasks if null
const categoriesWithTasks = categories.map(category => ({
  ...category,
  tasks: category.tasks || []
}));

// No need for separate GET /api/categories/{categoryId}/tasks endpoint
```

---

## Task Mutation Strategy for Frontend

### Create Task
```javascript
async function handleCreateTask(categoryId, taskData) {
  // 1. Call API
  const newTask = await createTask(categoryId, taskData);
  
  // 2. Append to local state
  setMilestones(prev => prev.map(milestone => ({
    ...milestone,
    categories: milestone.categories.map(category =>
      category.id === categoryId
        ? { ...category, tasks: [...category.tasks, newTask] }
        : category
    )
  })));
}
```

### Update Task Status
```javascript
async function handleUpdateTaskStatus(taskId, status) {
  // 1. Call API
  const updatedTask = await updateTaskStatus(taskId, status);
  
  // 2. Update local state
  setMilestones(prev => prev.map(milestone => ({
    ...milestone,
    categories: milestone.categories.map(category => ({
      ...category,
      tasks: category.tasks.map(task =>
        task.id === taskId ? updatedTask : task
      )
    }))
  })));
}
```

### Assign Task
```javascript
async function handleAssignTask(taskId, assigneeId, deptId) {
  // 1. Call API
  const updatedTask = await assignTask(taskId, assigneeId, deptId);
  
  // 2. Update local state
  setMilestones(prev => prev.map(milestone => ({
    ...milestone,
    categories: milestone.categories.map(category => ({
      ...category,
      tasks: category.tasks.map(task =>
        task.id === taskId ? updatedTask : task
      )
    }))
  })));
}
```

### Delete Task
```javascript
async function handleDeleteTask(taskId) {
  // 1. Call API
  await deleteTask(taskId);
  
  // 2. Remove from local state
  setMilestones(prev => prev.map(milestone => ({
    ...milestone,
    categories: milestone.categories.map(category => ({
      ...category,
      tasks: category.tasks.filter(task => task.id !== taskId)
    }))
  })));
}
```

---

## Known Backend Limitations

### Phase 4A-4 Limitations
1. **No pagination** - List endpoints return all items
2. **No search/filter** - List endpoints return all active items
3. **No task dependencies** - Tasks are independent (no predecessor/successor)
4. **No task comments** - Task comments not implemented
5. **No task attachments** - Task attachments not implemented
6. **No task history** - Task change history not tracked
7. **No notifications** - Task assignment/status change notifications not implemented
8. **No OrderIndex in OrgTask domain** - Using default value 0 in TaskDto

### Phase 4A-3 Limitations
1. **No pagination** - List endpoints return all items
2. **No search/filter** - List endpoints return all active items
3. **No soft-delete handling in DTOs** - Soft-deleted items filtered by global query filter

---

## Frontend Implementation Recommendations

### 1. Service Layer
Create service files for each module:
- `milestoneService.js` - Milestone CRUD operations
- `categoryService.js` - Category CRUD operations
- `taskService.js` - Task CRUD operations

### 2. Adapter Layer
Create adapter files for DTO transformation:
- `milestoneAdapter.js` - Transform MilestoneDto to ViewModel
- `categoryAdapter.js` - Transform EventCategoryDto to ViewModel
- `taskAdapter.js` - Transform TaskDto to ViewModel

### 3. Component Structure
```
EventDetailPage/
├── EventDetailHeader.jsx
├── MilestoneList.jsx
│   ├── MilestoneCard.jsx
│   ├── MilestoneForm.jsx
│   └── CategoryList.jsx
│       ├── CategoryCard.jsx
│       ├── CategoryForm.jsx
│       └── TaskList.jsx
│           ├── TaskCard.jsx
│           ├── TaskForm.jsx
│           ├── TaskStatusDropdown.jsx
│           └── TaskAssignModal.jsx
```

### 4. State Management
- Use page-level state for tree structure
- Use context for shared data (orgId, permissions)
- Use local state for UI state (modals, forms)

### 5. Permission Checks
```javascript
// Check if user has org.events.manage permission
const canManageEvents = permissions.permissionKeys.includes('org.events.manage');

// Show/hide create/edit/delete buttons based on permission
{canManageEvents && <CreateMilestoneButton />}
```

---

## Testing Strategy

### Unit Testing
- Test service layer functions
- Test adapter transformations
- Test component rendering

### Integration Testing
- Test full tree loading flow
- Test create/update/delete operations
- Test permission enforcement
- Test error handling

### E2E Testing
- Test full user flow from login to task management
- Test tree state management
- Test optimistic updates

---

## Success Criteria

1. ✅ User can view EventDetail tree (Event → Milestones → Categories → Tasks)
2. ✅ User can create/update/delete milestones (if has permission)
3. ✅ User can create/update/delete categories (if has permission)
4. ✅ User can create/update/delete tasks (if has permission)
5. ✅ User can update task status
6. ✅ User can assign tasks to members/departments
7. ✅ Tree state updates correctly after mutations
8. ✅ Permission checks work correctly
9. ✅ Error handling works correctly
10. ✅ Loading states work correctly

---

**End of PHASE_4B_FRONTEND_INTEGRATION_INPUT.md**
