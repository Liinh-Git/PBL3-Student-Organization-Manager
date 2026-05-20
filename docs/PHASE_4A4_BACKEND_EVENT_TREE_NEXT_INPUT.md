# PHASE_4A4_BACKEND_EVENT_TREE_NEXT_INPUT

## Purpose

Input document for Phase 4A-4: Backend EventDetail Tree (Milestones + EventCategories + Tasks) implementation.

---

## Phase 4A-3 Completion Status

✅ **COMPLETE** - 7 read-only endpoints for Members, Departments, and Events implemented and tested.

### What Was Delivered

**Members Module (1 endpoint)**:
- GET /api/organizations/{orgId}/members

**Departments Module (2 endpoints)**:
- GET /api/organizations/{orgId}/departments
- GET /api/departments/{id}

**Events Module (4 endpoints)**:
- GET /api/organizations/{orgId}/events
- GET /api/events/{id}
- GET /api/events/public
- GET /api/events/{id}/public

---

## Endpoint Shapes from Phase 4A-3

### Members Endpoint

#### GET /api/organizations/{orgId}/members
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
      "status": "Active|Invited|Suspended|Left|Removed",
      "joinedAtUtc": "datetime"
    }
  ]
}
```

### Departments Endpoints

#### GET /api/organizations/{orgId}/departments
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
      "status": "Active|Inactive|Archived",
      "createdAtUtc": "datetime",
      "updatedAtUtc": "datetime"
    }
  ]
}
```

#### GET /api/departments/{id}
**Response**: `ApiResponse<DepartmentDto>` (same as above)

### Events Endpoints

#### GET /api/organizations/{orgId}/events
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
      "status": "Draft|Published|InProgress|Completed|Cancelled",
      "visibility": "Public|Private|MembersOnly",
      "location": "string?"
    }
  ]
}
```

#### GET /api/events/{id}
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
    "status": "Draft|Published|InProgress|Completed|Cancelled",
    "visibility": "Public|Private|MembersOnly",
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

#### GET /api/events/{id}/public
**Response**: `ApiResponse<EventPublicDto>` (same as above)

---

## EventDetail Tree Hierarchy

### Entity Relationships

```
Event (1) → Milestone (N)
  └─ Milestone (1) → EventCategory (N)
      └─ EventCategory (1) → OrgTask (N)
          ├─ OrgTask (N) → Member (1) [AssignedToMemberId]
          ├─ OrgTask (N) → Member (1) [CreatedByMemberId]
          └─ OrgTask (N) → Department (1) [AssignedToDepartmentId]
```

### Domain Entities

**Milestone**:
- Id, EventId, MilestoneName, Description, DueDate, Status, DisplayOrder
- Status: NotStarted, InProgress, Completed, Delayed
- Navigation: Event, Categories

**EventCategory**:
- Id, MilestoneId, CategoryName, Description, DepartmentId, DisplayOrder
- Navigation: Milestone, Department, Tasks

**OrgTask**:
- Id, EventCategoryId, TaskName, Description, AssignedToMemberId, AssignedToDepartmentId, CreatedByMemberId, DueDate, Priority, Status, DisplayOrder
- Priority: Low, Medium, High, Critical
- Status: NotStarted, InProgress, Completed, Blocked
- Navigation: EventCategory, AssignedToMember, AssignedToDepartment, CreatedByMember

---

## Phase 4A-4 Target Endpoints

### Milestones Module (5 endpoints)

1. **GET /api/events/{eventId}/milestones** - List event milestones
2. **POST /api/events/{eventId}/milestones** - Create milestone
3. **GET /api/milestones/{id}** - Get milestone detail
4. **PUT /api/milestones/{id}** - Update milestone
5. **DELETE /api/milestones/{id}** - Delete milestone

### EventCategories Module (5 endpoints)

6. **GET /api/milestones/{milestoneId}/categories** - List milestone categories
7. **POST /api/milestones/{milestoneId}/categories** - Create category
8. **GET /api/categories/{id}** - Get category detail
9. **PUT /api/categories/{id}** - Update category
10. **DELETE /api/categories/{id}** - Delete category

### Tasks Module (6 endpoints)

11. **POST /api/categories/{categoryId}/tasks** - Create task
12. **GET /api/tasks/{taskId}** - Get task detail
13. **PUT /api/tasks/{taskId}** - Update task
14. **DELETE /api/tasks/{taskId}** - Delete task
15. **PUT /api/tasks/{taskId}/status** - Update task status
16. **PUT /api/tasks/{taskId}/assign** - Assign task to member/department

**Total**: 16 endpoints

---

## Contract Requirements

### Milestones Contracts

**Request DTOs**:
```csharp
public record CreateMilestoneRequest
{
    public required string MilestoneName { get; init; }
    public string? Description { get; init; }
    public DateTime DueDate { get; init; }
    public int? DisplayOrder { get; init; }
}

public record UpdateMilestoneRequest
{
    public required string MilestoneName { get; init; }
    public string? Description { get; init; }
    public DateTime DueDate { get; init; }
    public required string Status { get; init; } // NotStarted, InProgress, Completed, Delayed
    public int? DisplayOrder { get; init; }
}
```

**Response DTOs**:
```csharp
public record MilestoneDto
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required string MilestoneName { get; init; }
    public string? Description { get; init; }
    public required DateTime DueDate { get; init; }
    public required string Status { get; init; }
    public int? DisplayOrder { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
}
```

### EventCategories Contracts

**Request DTOs**:
```csharp
public record CreateEventCategoryRequest
{
    public required string CategoryName { get; init; }
    public string? Description { get; init; }
    public Guid? DepartmentId { get; init; }
    public int? DisplayOrder { get; init; }
}

public record UpdateEventCategoryRequest
{
    public required string CategoryName { get; init; }
    public string? Description { get; init; }
    public Guid? DepartmentId { get; init; }
    public int? DisplayOrder { get; init; }
}
```

**Response DTOs**:
```csharp
public record EventCategoryDto
{
    public required Guid Id { get; init; }
    public required Guid MilestoneId { get; init; }
    public required string CategoryName { get; init; }
    public string? Description { get; init; }
    public Guid? DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public int? DisplayOrder { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    // Optional: tasks array for frontend tree state management
    // public List<TaskDto>? Tasks { get; init; }
}
```

### Tasks Contracts

**Request DTOs**:
```csharp
public record CreateTaskRequest
{
    public required string TaskName { get; init; }
    public string? Description { get; init; }
    public Guid? AssignedToMemberId { get; init; }
    public Guid? AssignedToDepartmentId { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Priority { get; init; } // Low, Medium, High, Critical
    public int? DisplayOrder { get; init; }
}

public record UpdateTaskRequest
{
    public required string TaskName { get; init; }
    public string? Description { get; init; }
    public Guid? AssignedToMemberId { get; init; }
    public Guid? AssignedToDepartmentId { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Priority { get; init; }
    public string? Status { get; init; } // NotStarted, InProgress, Completed, Blocked
    public int? DisplayOrder { get; init; }
}

public record UpdateTaskStatusRequest
{
    public required string Status { get; init; } // NotStarted, InProgress, Completed, Blocked
}

public record AssignTaskRequest
{
    public Guid? AssignedToMemberId { get; init; }
    public Guid? AssignedToDepartmentId { get; init; }
}
```

**Response DTOs**:
```csharp
public record TaskDto
{
    public required Guid Id { get; init; }
    public required Guid EventCategoryId { get; init; }
    public required string TaskName { get; init; }
    public string? Description { get; init; }
    public Guid? AssignedToMemberId { get; init; }
    public string? AssignedToMemberName { get; init; }
    public Guid? AssignedToDepartmentId { get; init; }
    public string? AssignedToDepartmentName { get; init; }
    public Guid? CreatedByMemberId { get; init; }
    public string? CreatedByMemberName { get; init; }
    public DateTime? DueDate { get; init; }
    public required string Priority { get; init; }
    public required string Status { get; init; }
    public int? DisplayOrder { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
}
```

---

## Permission Requirements

### Read Operations
- **Permission**: `org.workspace.access` (membership check)
- **Endpoints**: All GET endpoints

### Write Operations
- **Permission**: `org.events.manage`
- **Endpoints**: All POST/PUT/DELETE endpoints

### Permission Check Pattern
```csharp
// For read operations
var isMember = await _context.Members
    .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

if (!isMember)
{
    throw new UnauthorizedAccessException("You do not have access to this organization");
}

// For write operations (after membership check)
var hasPermission = await _context.Members
    .Include(m => m.Role)
        .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
    .Where(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active)
    .SelectMany(m => m.Role.RolePermissions.Select(rp => rp.Permission.PermissionKey))
    .AnyAsync(key => key == "org.events.manage", ct);

if (!hasPermission)
{
    throw new UnauthorizedAccessException("You do not have permission to manage events");
}
```

---

## Validation Requirements

### Milestone Validators
- MilestoneName: required, 2-200 chars
- DueDate: required, must be after event start date
- Status: must be valid enum value (NotStarted, InProgress, Completed, Delayed)

### EventCategory Validators
- CategoryName: required, 2-200 chars
- DepartmentId: must exist if provided

### Task Validators
- TaskName: required, 2-200 chars
- AssignedToMemberId: must exist and be active member if provided
- AssignedToDepartmentId: must exist if provided
- Priority: must be valid enum value (Low, Medium, High, Critical)
- Status: must be valid enum value (NotStarted, InProgress, Completed, Blocked)

---

## Business Logic Requirements

### Milestone Business Logic
- DisplayOrder auto-increment if not provided
- Cannot delete milestone if it has categories
- Status auto-calculation based on category completion (optional)

### EventCategory Business Logic
- DisplayOrder auto-increment if not provided
- Cannot delete category if it has tasks
- DepartmentId must belong to same organization

### Task Business Logic
- DisplayOrder auto-increment if not provided
- CreatedByMemberId auto-set from JWT claims
- AssignedToMemberId must be member of same organization
- AssignedToDepartmentId must belong to same organization
- Cannot assign to both member and department (choose one)

---

## Frontend EventDetail Tree State Management

### Critical Frontend Pattern

**Page-level state management**:
```javascript
const [event, setEvent] = useState(null);
const [milestones, setMilestones] = useState([]);

// Load full tree
async function loadEventDetail() {
  const eventDto = await getEventById(eventId);
  setEvent(toEventViewModel(eventDto));

  const milestoneDtos = await getEventMilestones(eventId);
  const milestonesWithCategories = await Promise.all(
    milestoneDtos.map(async (milestone) => {
      const categoryDtos = await getMilestoneCategories(milestone.id);
      const categoriesWithTasks = categoryDtos.map(category => ({
        ...toCategoryViewModel(category),
        tasks: category.tasks || [] // Initialize if absent
      }));
      return {
        ...toMilestoneViewModel(milestone),
        categories: categoriesWithTasks
      };
    })
  );
  setMilestones(milestonesWithCategories);
}

// Create task - append to local state
async function handleCreateTask(categoryId, payload) {
  const taskDto = await createTask(categoryId, payload);
  const taskViewModel = toTaskViewModel(taskDto);
  
  setMilestones(prev => prev.map(milestone => ({
    ...milestone,
    categories: milestone.categories.map(category => 
      category.id === categoryId
        ? { ...category, tasks: [...category.tasks, taskViewModel] }
        : category
    )
  })));
}
```

**CRITICAL**: TaskCard must NOT own source-of-truth state. State lives at page/hook level.

---

## CategoryDto tasks[] Handling

### Option 1: Include tasks[] in CategoryDto (Recommended)
```csharp
public record EventCategoryDto
{
    // ... other fields
    public List<TaskDto>? Tasks { get; init; }
}
```

**Service implementation**:
```csharp
public async Task<List<EventCategoryDto>> GetMilestoneCategoriesAsync(Guid milestoneId, Guid userId, CancellationToken ct)
{
    var categories = await _context.EventCategories
        .Include(c => c.Tasks.Where(t => !t.IsDeleted))
            .ThenInclude(t => t.AssignedToMember)
                .ThenInclude(m => m.User)
        .Include(c => c.Tasks.Where(t => !t.IsDeleted))
            .ThenInclude(t => t.AssignedToDepartment)
        .Where(c => c.MilestoneId == milestoneId && !c.IsDeleted)
        .ToListAsync(ct);

    return categories.Select(c => c.ToEventCategoryDto()).ToList();
}
```

### Option 2: Separate list endpoint (NOT Recommended)
- Do NOT create `GET /api/categories/{categoryId}/tasks` endpoint
- Reason: Adds unnecessary complexity, frontend can initialize `tasks: []` if absent

---

## Implementation Order

### Phase 4A-4A: Milestones Module
1. Create MilestoneContracts.cs
2. Create MilestoneMappings.cs
3. Create MilestoneValidators
4. Create IMilestoneService + MilestoneService
5. Create Milestone endpoints (5 endpoints)
6. Test milestone CRUD

### Phase 4A-4B: EventCategories Module
7. Create CategoryContracts.cs
8. Create CategoryMappings.cs
9. Create CategoryValidators
10. Create ICategoryService + CategoryService
11. Create Category endpoints (5 endpoints)
12. Test category CRUD

### Phase 4A-4C: Tasks Module
13. Create TaskContracts.cs
14. Create TaskMappings.cs
15. Create TaskValidators
16. Create ITaskService + TaskService
17. Create Task endpoints (6 endpoints)
18. Test task CRUD + status/assign

### Phase 4A-4D: Integration Testing
19. Test full EventDetail tree (Event → Milestone → Category → Task)
20. Test permission enforcement
21. Test tree state management patterns

---

## Testing Strategy

### Unit Testing (Optional)
- Service layer unit tests with mocked DbContext
- Validator unit tests

### Integration Testing (Required)
- Test full tree hierarchy
- Test create milestone → create category → create task
- Test update task status
- Test assign task to member/department
- Test delete cascade (cannot delete milestone with categories)
- Test permission enforcement (org.events.manage)

### Test Data
Use existing seed data:
- Admin user: admin@example.com / Admin@123456
- Event: Annual Tech Summit 2026
- Members: 6 members available for task assignment
- Departments: 3 departments available for task assignment

---

## Success Criteria

1. ✅ All 16 endpoints build successfully
2. ✅ All 16 endpoints start successfully
3. ✅ All 16 endpoints pass smoke tests
4. ✅ Validation works correctly
5. ✅ Permission enforcement works correctly (org.events.manage)
6. ✅ Tree hierarchy works correctly (Event → Milestone → Category → Task)
7. ✅ No frontend modified
8. ✅ No migration created (unless entity changes required)
9. ✅ Report created

---

## Known Limitations

### Phase 4A-3 Limitations Carried Forward
1. **No pagination** - List endpoints return all items
2. **No search/filter** - List endpoints return all active items
3. **No soft-delete handling in DTOs** - Soft-deleted items filtered by global query filter

### Phase 4A-4 Specific Limitations
1. **No task dependencies** - Tasks are independent (no predecessor/successor)
2. **No task comments** - Task comments not implemented
3. **No task attachments** - Task attachments not implemented
4. **No task history** - Task change history not tracked
5. **No notifications** - Task assignment/status change notifications not implemented

---

## Next Steps After Phase 4A-4

### Option 1: Members/Departments/Events Write Operations
- POST /api/organizations/{orgId}/members
- PUT /api/members/{id}/department
- DELETE /api/members/{id}
- POST /api/organizations/{orgId}/departments
- PUT /api/departments/{id}
- DELETE /api/departments/{id}
- POST /api/organizations/{orgId}/events
- PUT /api/events/{id}
- DELETE /api/events/{id}

### Option 2: Requests Module
- GET /api/organizations/{orgId}/requests
- POST /api/organizations/{orgId}/requests
- GET /api/requests/{requestId}
- POST /api/organizations/requests/{requestId}/review

### Option 3: Frontend Integration
- Connect frontend EventDetail tree to backend APIs
- Implement tree state management
- Test full user flow

---

**End of PHASE_4A4_BACKEND_EVENT_TREE_NEXT_INPUT.md**
