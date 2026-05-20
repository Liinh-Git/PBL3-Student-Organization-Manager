# PHASE_4A3_BACKEND_CORE_NEXT_INPUT

## Purpose

Input document for next backend implementation phase after Phase 4A-2A completion.

---

## Phase 4A-2A Completion Status

✅ **COMPLETE** - 10 read-only endpoints implemented and tested.

### What Was Delivered

**Users Module (4 endpoints)**:
- GET /api/users/me
- GET /api/users/me/organizations
- GET /api/users/me/events
- GET /api/users/me/discover/organizations

**Organizations Module (4 endpoints)**:
- GET /api/organizations
- GET /api/organizations/default
- GET /api/organizations/{id}
- GET /api/organizations/{id}/public-overview

**RolesPermissions Module (2 endpoints)**:
- GET /api/organizations/{orgId}/permissions/me
- GET /api/organizations/{orgId}/roles

---

## Endpoint Shapes from Phase 4A-2A

### Users Endpoints

#### GET /api/users/me
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
    "status": "Active|Inactive|Suspended",
    "profileVisibility": "Public|Private|FriendsOnly?",
    "lastLoginAtUtc": "datetime?"
  }
}
```

#### GET /api/users/me/organizations
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
      "status": "Draft|Published|InProgress|Completed|Cancelled",
      "visibility": "Public|Private|MembersOnly",
      "location": "string?"
    }
  ]
}
```

#### GET /api/users/me/discover/organizations
**Response**: `ApiResponse<List<DiscoverOrganizationDto>>`
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
      "totalMembers": "int",
      "status": "Active|Inactive"
    }
  ]
}
```

### Organizations Endpoints

#### GET /api/organizations
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
      "status": "Active|Inactive"
    }
  ]
}
```

#### GET /api/organizations/default
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
    "status": "Active|Inactive",
    "createdAtUtc": "datetime",
    "updatedAtUtc": "datetime"
  }
}
```

#### GET /api/organizations/{id}
**Response**: `ApiResponse<OrganizationDto>` (same as default)
**Auth**: Requires membership (403 if not member)

#### GET /api/organizations/{id}/public-overview
**Response**: `ApiResponse<OrganizationPublicOverviewDto>`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "string",
    "description": "string?",
    "avatarUrl": "string?",
    "coverUrl": "string?",
    "totalMembers": "int",
    "publicEventsCount": "int?",
    "departmentsCount": "int?"
  }
}
```
**Auth**: Public (no auth required)

### RolesPermissions Endpoints

#### GET /api/organizations/{orgId}/permissions/me
**Response**: `ApiResponse<MyPermissionsResponse>`
```json
{
  "success": true,
  "data": {
    "permissionKeys": ["org.overview.read", "org.overview.write", ...],
    "roleId": "guid",
    "roleName": "string",
    "memberId": "guid",
    "organizationId": "guid"
  }
}
```
**Auth**: Requires membership (403 if not member)

#### GET /api/organizations/{orgId}/roles
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
      "isDefault": "bool",
      "permissionKeys": ["org.overview.read", ...]
    }
  ]
}
```
**Auth**: Requires membership (403 if not member)

---

## Field Mappings for Next Modules

### orgId Field
- **Source**: `Organization.Id` (Guid)
- **Used in**: Members, Departments, Events, Roles, Requests
- **Query string**: `/org/*` routes use `?orgId=` query parameter

### memberId Field
- **Source**: `Member.Id` (Guid)
- **Used in**: Task assignments, role assignments, event memberships
- **Relationship**: Links User to Organization with Role

### roleId Field
- **Source**: `Role.Id` (Guid)
- **Used in**: Member role assignment, permission resolution
- **Relationship**: Role → RolePermissions → Permissions

### userId Field
- **Source**: `User.Id` (Guid)
- **JWT Claim**: `ClaimTypes.NameIdentifier` or `JwtRegisteredClaimNames.Sub`
- **Used in**: All authenticated endpoints

---

## Known Limitations

### Phase 4A-2A Limitations

1. **No write operations** - All endpoints are read-only
2. **No pagination** - List endpoints return all items (no page/pageSize)
3. **No search/filter** - List endpoints return all active items
4. **No permission enforcement on read** - Only membership checks implemented
5. **No soft-delete handling in DTOs** - Soft-deleted items filtered by global query filter

### Recommended Enhancements for Phase 4A-2B

1. **Add pagination** - Use `ListResponse<T>` for list endpoints
2. **Add search/filter** - Add query parameters for filtering
3. **Add permission checks** - Enforce `org.roles.view` for roles endpoint
4. **Add sorting** - Add orderBy query parameter

---

## Next Suggested Backend Batch: Phase 4A-2B

### Priority 1: User Profile Write Operations

**Endpoints**:
1. PUT /api/users/me - Update user profile
2. PUT /api/users/me/change-password - Change password

**Contracts Needed**:
- `UpdateUserProfileRequest` (FullName, PhoneNumber?, AvatarUrl?, Bio?)
- `ChangePasswordRequest` (CurrentPassword, NewPassword, ConfirmPassword?)

**Validators Needed**:
- `UpdateUserProfileRequestValidator`
- `ChangePasswordRequestValidator`

**Service Methods**:
- `IUserService.UpdateMeAsync()`
- `IUserService.ChangePasswordAsync()`

---

### Priority 2: Organization Write Operations

**Endpoints**:
3. POST /api/organizations - Create organization
4. PUT /api/organizations/{id} - Update organization

**Contracts Needed**:
- `CreateOrganizationRequest` (Name, Description?, AvatarUrl?, CoverUrl?, Location?)
- `UpdateOrganizationRequest` (Name?, Description?, AvatarUrl?, CoverUrl?, Location?, Status?)

**Validators Needed**:
- `CreateOrganizationRequestValidator`
- `UpdateOrganizationRequestValidator`

**Service Methods**:
- `IOrganizationService.CreateOrganizationAsync()`
- `IOrganizationService.UpdateOrganizationAsync()`

**Business Logic**:
- Create default roles when creating organization
- Create creator as President/Admin member
- Update TotalMembers count

---

### Priority 3: Role Management Write Operations

**Endpoints**:
5. POST /api/organizations/{orgId}/roles - Create role
6. PUT /api/organizations/roles/{roleId} - Update role
7. DELETE /api/organizations/roles/{roleId} - Delete role
8. POST /api/organizations/{orgId}/members/{memberId}/role - Assign role

**Contracts Needed**:
- `CreateRoleRequest` (RoleName, Description?, PermissionKeys)
- `UpdateRoleRequest` (RoleName?, Description?, PermissionKeys?)
- `AssignRoleToMemberRequest` (RoleId)

**Validators Needed**:
- `CreateRoleRequestValidator`
- `UpdateRoleRequestValidator`
- `AssignRoleToMemberRequestValidator`

**Service Methods**:
- `IRoleService.CreateRoleAsync()`
- `IRoleService.UpdateRoleAsync()`
- `IRoleService.DeleteRoleAsync()`
- `IRoleService.AssignRoleToMemberAsync()`

**Business Logic**:
- Validate permission keys against canonical list
- Prevent duplicate role names in same org
- Prevent deleting role if members assigned
- Prevent deleting default roles

**Permission Requirements**:
- Create role: `org.roles.create`
- Update role: `org.roles.update`
- Delete role: `org.roles.delete`
- Assign role: `org.roles.assign`

---

### Priority 4: Members Module (Read + Write)

**Endpoints**:
9. GET /api/organizations/{orgId}/members - List members
10. POST /api/organizations/{orgId}/members - Add member
11. PUT /api/members/{id}/department - Update member department
12. DELETE /api/members/{id} - Remove member

**Contracts Needed**:
- `MemberDto` (Id, UserId, UserFullName, UserEmail, UserAvatarUrl, DepartmentId?, DepartmentName?, RoleId, RoleName, JoinDate, Status)
- `AddMemberRequest` (UserId or Email, RoleId?, DepartmentId?)
- `UpdateMemberDepartmentRequest` (DepartmentId?)

**Permission Requirements**:
- List members: `org.workspace.access`
- Add member: `org.members.manage`
- Update department: `org.members.manage`
- Remove member: `org.members.manage`

---

### Priority 5: Departments Module (Read + Write)

**Endpoints**:
13. GET /api/organizations/{orgId}/departments - List departments
14. POST /api/organizations/{orgId}/departments - Create department
15. GET /api/departments/{id} - Get department detail
16. PUT /api/departments/{id} - Update department
17. DELETE /api/departments/{id} - Delete department

**Contracts Needed**:
- `DepartmentDto` (Id, OrgId, DeptName, Description?, ManagerId?, ManagerName?, MemberCount, CreatedAtUtc)
- `CreateDepartmentRequest` (DeptName, Description?, ManagerId?)
- `UpdateDepartmentRequest` (DeptName?, Description?, ManagerId?)

**Permission Requirements**:
- List departments: `org.workspace.access`
- Create department: `org.departments.manage`
- Update department: `org.departments.manage`
- Delete department: `org.departments.manage`

---

### Priority 6: Events Module (List + Create)

**Endpoints**:
18. GET /api/organizations/{orgId}/events - List organization events
19. POST /api/organizations/{orgId}/events - Create event
20. GET /api/events/{id} - Get event detail
21. GET /api/events/public - List public events
22. GET /api/events/{id}/public - Get public event detail

**Contracts Needed**:
- `EventDto` (Id, OrgId, EventName, Description?, StartDate, EndDate, Budget?, Location?, TargetParticipants?, Tags?, Status, Visibility, AverageRating?, CreatedByMemberId?, CreatedAtUtc)
- `EventSummaryDto` (Id, OrgId, EventName, Description?, StartDate, EndDate, Status, Visibility, Location?)
- `EventPublicDto` (Id, OrgName, EventName, Description?, StartDate, EndDate, Location?, Visibility)
- `CreateEventRequest` (EventName, Description?, StartDate, EndDate, Budget?, Location?, TargetParticipants?, Tags?, Visibility)

**Permission Requirements**:
- List events: `org.workspace.access`
- Create event: `org.events.create`
- Get event detail: `org.workspace.access`
- List public events: Public (no auth)
- Get public event detail: Public (no auth)

---

## Implementation Pattern Summary

### Service Layer Pattern

```csharp
public interface IModuleService
{
    Task<Dto> GetAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<List<Dto>> ListAsync(Guid orgId, Guid userId, CancellationToken ct = default);
    Task<Dto> CreateAsync(Guid orgId, Guid userId, CreateRequest request, CancellationToken ct = default);
    Task<Dto> UpdateAsync(Guid id, Guid userId, UpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}
```

### Endpoint Pattern

```csharp
public class CreateEndpoint : Endpoint<CreateRequest, ApiResponse<Dto>>
{
    private readonly IModuleService _service;

    public override void Configure()
    {
        Post("/path");
        Description(b => b
            .Produces<ApiResponse<Dto>>(200)
            .Produces<ApiResponse<Dto>>(400)
            .Produces<ApiResponse<Dto>>(401)
            .Produces<ApiResponse<Dto>>(403)
            .WithTags("Module"));
    }

    public override async Task HandleAsync(CreateRequest req, CancellationToken ct)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var result = await _service.CreateAsync(orgId, userId, req, ct);
            Response = ApiResponse<Dto>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<Dto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<Dto>.ErrorResponse("Operation failed", new List<string> { ex.Message });
        }
    }
}
```

### Validator Pattern

```csharp
public class CreateRequestValidator : Validator<CreateRequest>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
```

---

## Testing Strategy for Phase 4A-2B

### Unit Testing (Optional)

- Service layer unit tests with mocked DbContext
- Validator unit tests

### Integration Testing (Required)

- Endpoint smoke tests with real database
- Test valid requests
- Test invalid requests (validation errors)
- Test unauthorized requests (401)
- Test forbidden requests (403)
- Test permission enforcement

### Test Data

Use existing seed data:
- Admin user: admin@example.com / Admin@123456
- Demo users: member1@example.com / User@123456 (etc.)
- Default organization: Student Organization
- Default roles: President, Manager, Member

---

## Recommended Implementation Order for Phase 4A-2B

1. **User profile updates** (simplest, no permission checks needed)
2. **Organization create/update** (moderate complexity, membership checks)
3. **Role management** (complex, permission checks required)
4. **Members CRUD** (depends on roles)
5. **Departments CRUD** (depends on members)
6. **Events list/create** (depends on members)

---

## Success Criteria for Phase 4A-2B

1. ✅ All write endpoints build successfully
2. ✅ All write endpoints start successfully
3. ✅ All write endpoints pass smoke tests
4. ✅ Validation works correctly
5. ✅ Permission enforcement works correctly
6. ✅ No frontend modified
7. ✅ No migration created (unless entity changes required)
8. ✅ Report created

---

**End of PHASE_4A3_BACKEND_CORE_NEXT_INPUT.md**
