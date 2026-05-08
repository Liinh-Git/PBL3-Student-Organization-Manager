# TODO_IMPLEMENTATION_GUIDE

## Purpose
Detailed implementation guidance for converting Phase 3C skeleton into working implementation. This guide provides step-by-step instructions for implementing each TODO across backend, shared contracts, and frontend.

---

## 1. Implementation Philosophy

### Core Principles
1. **Implement one module at a time** - Complete backend → contract → frontend for each module before moving to next
2. **Verify Swagger before frontend integration** - Test all backend endpoints in Swagger UI before connecting frontend
3. **No mock fallback** - If backend is not ready, frontend shows loading/error states, never fake success
4. **No fake data** - Never hardcode business data; always fetch from API or show empty state
5. **No silent success** - Every operation must have clear success/error feedback
6. **Database/migration still paused** - Unless user explicitly resumes Phase 3B.3

### Implementation Order
Backend → Shared Contracts → Frontend Services → Frontend Adapters → Frontend Pages → Integration Testing

---

## 2. Recommended Implementation Order

### Backend Implementation Order
1. **Auth** - Foundation for all authenticated endpoints
2. **Users** - User profile and settings
3. **Organizations** - Organization CRUD and workspace foundation
4. **RolesPermissions** - Permission system before member/department/event management
5. **Members** - Member management after roles/permissions
6. **Departments** - Department management after members
7. **Events** - Event CRUD before EventDetail tree
8. **Milestones** - Milestone management inside events
9. **EventCategories** - Category management inside milestones
10. **Tasks** - Task management inside categories (CORE inside EventDetail tree)
11. **Requests** - Request join organization workflow
12. **Notifications** - Notification system
13. **Friends/Discover** - Supporting modules last

### Shared Contracts Implementation Order
Convert `.TODO` files to real C# contracts module by module, following backend implementation order.

### Frontend Implementation Order
1. Replace service TODO stubs with httpClient calls after Swagger verified
2. Implement adapters after DTO verified
3. Implement pages with real loading/error/empty/forbidden states
4. Implement EventDetail tree last among event modules

---

## 3. Backend Implementation Guide

### 3.1 Converting Backend Skeleton to Real Implementation

For each module (e.g., `backend/Org.Backend/Features/Auth/`):

#### Step 1: Read Module README
- Read `README.md` for module overview and TODO notes
- Read `Endpoints/README.md` for endpoint plan
- Read `Services/README.md` for service plan
- Read `Validators/README.md` for validation plan
- Read `Mappings/README.md` for mapping plan
- Read `Permissions.TODO.md` for permission requirements

#### Step 2: Create Service Layer
```csharp
// Example: backend/Org.Backend/Features/Auth/Services/AuthService.cs
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        // TODO: Implement login logic
        // 1. Find user by email (case-insensitive)
        // 2. Verify password hash
        // 3. Check user status (Active only)
        // 4. Generate JWT token
        // 5. Update LastLoginAt
        // 6. Return token response
        throw new NotImplementedException();
    }
}
```

#### Step 3: Create Validators
```csharp
// Example: backend/Org.Backend/Features/Auth/Validators/LoginRequestValidator.cs
public class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}
```

#### Step 4: Create Mappings
```csharp
// Example: backend/Org.Backend/Features/Auth/Mappings/AuthMappings.cs
public static class AuthMappings
{
    public static AuthUserDto ToAuthUserDto(this User user)
    {
        return new AuthUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status.ToString()
        };
    }
}
```

#### Step 5: Implement FastEndpoints
```csharp
// Example: backend/Org.Backend/Features/Auth/Endpoints/LoginEndpoint.cs
public class LoginEndpoint : Endpoint<LoginRequest, ApiResponse<AuthTokenResponse>>
{
    private readonly IAuthService _authService;

    public LoginEndpoint(IAuthService authService)
    {
        _authService = authService;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Description(b => b
            .Produces<ApiResponse<AuthTokenResponse>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(401));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _authService.LoginAsync(req, ct);
            await SendAsync(ApiResponse<AuthTokenResponse>.Success(result), cancellation: ct);
        }
        catch (UnauthorizedAccessException ex)
        {
            await SendAsync(ApiResponse<AuthTokenResponse>.Error(ex.Message), 401, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(ApiResponse<AuthTokenResponse>.Error("Login failed"), 400, ct);
        }
    }
}
```

#### Step 6: Register Services
```csharp
// backend/Org.Backend/Infrastructure/Startup/ServiceRegistration.cs
public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auth services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        
        // ... other services
        
        return services;
    }
}
```

#### Step 7: Test in Swagger
- Run backend: `dotnet run --project backend/Org.Backend`
- Open Swagger UI: `http://localhost:5000/swagger`
- Test all endpoints with valid/invalid data
- Verify response shapes match contracts
- Verify error handling
- Verify permission checks

---

## 4. Shared Contracts Implementation Guide

### 4.1 Converting Contract Skeleton to Real Implementation

For each module (e.g., `backend/Org.Shared/Features/Auth/`):

#### Step 1: Read Contract README
- Read `README.md` for contract overview and TODO notes
- Read `<Module>Contracts.cs.TODO` for DTO structure notes

#### Step 2: Create Real Contract File
```csharp
// Example: backend/Org.Shared/Features/Auth/AuthContracts.cs
namespace Org.Shared.Features.Auth;

// Request DTOs
public record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record RegisterRequest
{
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? PhoneNumber { get; init; }
}

// Response DTOs
public record AuthUserDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? AvatarUrl { get; init; }
    public required string Status { get; init; }
}

public record AuthTokenResponse
{
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required AuthUserDto User { get; init; }
}

public record CurrentUserResponse
{
    public required AuthUserDto User { get; init; }
}
```

#### Step 3: Delete .TODO File
- Delete `<Module>Contracts.cs.TODO` after creating real contract file
- Update README.md to mark contracts as implemented

#### Step 4: Verify Contract Usage
- Ensure backend endpoints use these contracts
- Ensure frontend services expect these contracts
- Verify field names match across all layers

---

## 5. Frontend Services Implementation Guide

### 5.1 Converting Service Stubs to Real Implementation

For each service (e.g., `frontend/src/services/authService.js`):

#### Step 1: Read Service TODO Comments
- Read all TODO comments in service file
- Understand expected request/response shapes
- Understand error handling requirements

#### Step 2: Import httpClient
```javascript
// frontend/src/services/authService.js
import httpClient from '../api/httpClient';

// Remove TODO comments and implement real API calls
export async function login(credentials) {
  // TODO: Implement login API call
  // POST /auth/login
  // Request: { email, password }
  // Response: ApiResponse<AuthTokenResponse>
  // On success: store token in localStorage, return user
  // On 401: throw error with message
  // On 400: throw validation error
  
  const response = await httpClient.post('/auth/login', credentials);
  
  if (response.success && response.data) {
    // Store token
    localStorage.setItem('org.auth.accessToken', response.data.accessToken);
    localStorage.setItem('org.auth.accessTokenExpiryUtc', response.data.expiresAt);
    return response.data.user;
  }
  
  throw new Error(response.message || 'Login failed');
}
```

#### Step 3: Handle ApiResponse Wrapper
```javascript
// All backend responses use ApiResponse<T> wrapper
// Success response: { success: true, data: T, message: string }
// Error response: { success: false, errors: string[], message: string }

export async function getMe() {
  const response = await httpClient.get('/users/me');
  
  if (response.success && response.data) {
    return response.data; // UserProfileDto
  }
  
  throw new Error(response.message || 'Failed to fetch user profile');
}
```

#### Step 4: Handle List Responses
```javascript
// List responses use ApiResponse<ListResponse<T>>
// ListResponse: { items: T[], totalCount: number, page: number, pageSize: number, totalPages: number }

export async function getMyOrganizations(params = {}) {
  const response = await httpClient.get('/users/me/organizations', { params });
  
  if (response.success && response.data) {
    return response.data.items; // MyOrganizationDto[]
  }
  
  throw new Error(response.message || 'Failed to fetch organizations');
}
```

#### Step 5: Handle Errors
```javascript
// httpClient already handles 401 (clear auth, redirect to login)
// httpClient already handles 403 (do NOT redirect, let page handle)
// Service should throw errors for page to catch

export async function updateMe(payload) {
  try {
    const response = await httpClient.put('/users/me', payload);
    
    if (response.success && response.data) {
      return response.data; // UserProfileDto
    }
    
    throw new Error(response.message || 'Failed to update profile');
  } catch (error) {
    // Re-throw for page to handle
    throw error;
  }
}
```

---

## 6. Frontend Adapters Implementation Guide

### 6.1 Converting Adapter Stubs to Real Implementation

For each adapter (e.g., `frontend/src/adapters/userAdapter.js`):

#### Step 1: Read Adapter TODO Comments
- Read all TODO comments in adapter file
- Understand DTO shape from backend
- Understand ViewModel shape for frontend

#### Step 2: Implement DTO → ViewModel Mapping
```javascript
// frontend/src/adapters/userAdapter.js

export function toUserProfileViewModel(dto) {
  if (!dto) return null;
  
  // TODO: Map UserProfileDto to ViewModel
  // DTO fields: id, fullName, email, phoneNumber, dob, gender, address, avatarUrl, bio, socialLinks, status, profileVisibility
  // ViewModel: flatten, format dates, parse JSON fields
  
  return {
    id: dto.id,
    fullName: dto.fullName,
    email: dto.email,
    phoneNumber: dto.phoneNumber || '',
    dob: dto.dob ? new Date(dto.dob) : null,
    gender: dto.gender || '',
    address: dto.address || '',
    avatarUrl: dto.avatarUrl || '',
    bio: dto.bio || '',
    socialLinks: dto.socialLinks ? JSON.parse(dto.socialLinks) : {},
    status: dto.status,
    profileVisibility: dto.profileVisibility || 'Public',
  };
}
```

#### Step 3: Handle Optional Fields Safely
```javascript
export function toMyOrganizationViewModel(dto) {
  if (!dto) return null;
  
  return {
    id: dto.id,
    orgName: dto.orgName,
    description: dto.description || '',
    avatarUrl: dto.avatarUrl || '',
    coverUrl: dto.coverUrl || '',
    totalMembers: dto.totalMembers || 0,
    status: dto.status,
    myRole: dto.myRole || 'Member', // MemberRole enum
    myPermissions: dto.myPermissions || [], // string[]
  };
}
```

#### Step 4: Handle List Adapters
```javascript
export function toMemberListViewModel(items) {
  if (!Array.isArray(items)) return [];
  
  return items.map(toMemberViewModel).filter(Boolean);
}
```

---

## 7. Frontend Pages Implementation Guide

### 7.1 Converting Page Skeletons to Real Implementation

For each page (e.g., `frontend/src/pages/user/UserProfilePage.jsx`):

#### Step 1: Read Page TODO Comments
- Read all TODO comments in page file
- Understand data loading requirements
- Understand permission requirements
- Understand state management requirements

#### Step 2: Implement Data Loading
```jsx
// frontend/src/pages/user/UserProfilePage.jsx
import { useState, useEffect } from 'react';
import { getMe } from '../../services/userService';
import { toUserProfileViewModel } from '../../adapters/userAdapter';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import ErrorState from '../../components/shared/ErrorState';

export default function UserProfilePage() {
  const [profile, setProfile] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadProfile();
  }, []);

  async function loadProfile() {
    try {
      setIsLoading(true);
      setError(null);
      const dto = await getMe();
      const viewModel = toUserProfileViewModel(dto);
      setProfile(viewModel);
    } catch (err) {
      setError(err.message || 'Failed to load profile');
    } finally {
      setIsLoading(false);
    }
  }

  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorState message={error} onRetry={loadProfile} />;
  if (!profile) return <EmptyState message="Profile not found" />;

  return (
    <div>
      <h1>{profile.fullName}</h1>
      <p>{profile.email}</p>
      {/* ... rest of profile UI */}
    </div>
  );
}
```

#### Step 3: Implement Permission Checks
```jsx
// frontend/src/pages/org/OrgMembersPage.jsx
import { useOrg } from '../../hooks/useOrg';
import { usePermission } from '../../hooks/usePermission';
import ForbiddenState from '../../components/shared/ForbiddenState';

export default function OrgMembersPage() {
  const { orgId, isMember } = useOrg();
  const { hasPermission } = usePermission();
  
  const canView = hasPermission('org.workspace.access');
  const canManage = hasPermission('org.members.manage');

  if (!isMember || !canView) {
    return <ForbiddenState message="You do not have permission to view members" />;
  }

  // ... rest of page implementation
}
```

#### Step 4: Implement CRUD Operations
```jsx
async function handleAddMember(payload) {
  try {
    setIsSubmitting(true);
    setError(null);
    const dto = await addMember(orgId, payload);
    const viewModel = toMemberViewModel(dto);
    setMembers(prev => [...prev, viewModel]);
    setShowAddModal(false);
    // Show success toast
  } catch (err) {
    setError(err.message || 'Failed to add member');
  } finally {
    setIsSubmitting(false);
  }
}
```

---

## 8. EventDetail Tree Implementation Guide

### 8.1 EventDetail State Management

The EventDetail tree is **CRITICAL** and requires careful state management.

#### Step 1: Load Event and Tree Data
```jsx
// frontend/src/pages/org/OrgEventDetailPage.jsx
import { useState, useEffect } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { getEventById } from '../../services/eventService';
import { getEventMilestones } from '../../services/milestoneService';
import { getMilestoneCategories } from '../../services/categoryService';

export default function OrgEventDetailPage() {
  const { eventId } = useParams(); // eventId from path
  const [searchParams] = useSearchParams();
  const orgId = searchParams.get('orgId'); // orgId from query string

  const [event, setEvent] = useState(null);
  const [milestones, setMilestones] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadEventDetail();
  }, [eventId, orgId]);

  async function loadEventDetail() {
    try {
      setIsLoading(true);
      setError(null);

      // Load event
      const eventDto = await getEventById(eventId);
      const eventViewModel = toEventViewModel(eventDto);
      setEvent(eventViewModel);

      // Load milestones
      const milestoneDtos = await getEventMilestones(eventId);
      const milestoneViewModels = milestoneDtos.map(toMilestoneViewModel);

      // Load categories for each milestone
      const milestonesWithCategories = await Promise.all(
        milestoneViewModels.map(async (milestone) => {
          const categoryDtos = await getMilestoneCategories(milestone.id);
          const categoryViewModels = categoryDtos.map(toCategoryViewModel);
          
          // Initialize tasks array if absent
          const categoriesWithTasks = categoryViewModels.map(category => ({
            ...category,
            tasks: category.tasks || [] // If CategoryDto lacks tasks[], initialize []
          }));
          
          return {
            ...milestone,
            categories: categoriesWithTasks
          };
        })
      );

      setMilestones(milestonesWithCategories);
    } catch (err) {
      setError(err.message || 'Failed to load event detail');
    } finally {
      setIsLoading(false);
    }
  }

  // ... rest of implementation
}
```

#### Step 2: Handle Category DTO tasks[] Handling
```javascript
// CategoryDto may include optional tasks[] array
// If tasks[] exists → use it
// If tasks[] absent → initialize tasks: []
// Do NOT invent a separate list-by-category task endpoint

const categoryViewModels = categoryDtos.map(dto => {
  const viewModel = toCategoryViewModel(dto);
  return {
    ...viewModel,
    tasks: viewModel.tasks || [] // Safe initialization
  };
});
```

#### Step 3: Handle Create Task Success
```jsx
async function handleCreateTask(categoryId, payload) {
  try {
    setIsSubmitting(true);
    setError(null);
    
    // Create task
    const taskDto = await createTask(categoryId, payload);
    const taskViewModel = toTaskViewModel(taskDto);
    
    // Append to local category.tasks[]
    setMilestones(prev => prev.map(milestone => ({
      ...milestone,
      categories: milestone.categories.map(category => 
        category.id === categoryId
          ? { ...category, tasks: [...category.tasks, taskViewModel] }
          : category
      )
    })));
    
    setShowTaskModal(false);
    // Show success toast
  } catch (err) {
    setError(err.message || 'Failed to create task');
  } finally {
    setIsSubmitting(false);
  }
}
```

#### Step 4: Handle Update/Status/Assign Task
```jsx
async function handleUpdateTaskStatus(taskId, newStatus) {
  try {
    const taskDto = await updateTaskStatus(taskId, { status: newStatus });
    const taskViewModel = toTaskViewModel(taskDto);
    
    // Mutate tree state at page level
    setMilestones(prev => prev.map(milestone => ({
      ...milestone,
      categories: milestone.categories.map(category => ({
        ...category,
        tasks: category.tasks.map(task => 
          task.id === taskId ? taskViewModel : task
        )
      }))
    })));
  } catch (err) {
    setError(err.message || 'Failed to update task status');
  }
}
```

#### Step 5: Handle Delete Task
```jsx
async function handleDeleteTask(taskId) {
  try {
    await deleteTask(taskId);
    
    // Remove from local category.tasks[]
    setMilestones(prev => prev.map(milestone => ({
      ...milestone,
      categories: milestone.categories.map(category => ({
        ...category,
        tasks: category.tasks.filter(task => task.id !== taskId)
      }))
    })));
    
    // Show success toast
  } catch (err) {
    setError(err.message || 'Failed to delete task');
  }
}
```

#### Step 6: Pass State and Callbacks to Components
```jsx
return (
  <div>
    <h1>{event.eventName}</h1>
    
    {milestones.map(milestone => (
      <MilestonePanel
        key={milestone.id}
        milestone={milestone}
        categories={milestone.categories}
        onCreateCategory={handleCreateCategory}
        onUpdateCategory={handleUpdateCategory}
        onDeleteCategory={handleDeleteCategory}
        onCreateTask={handleCreateTask}
        onUpdateTask={handleUpdateTask}
        onUpdateTaskStatus={handleUpdateTaskStatus}
        onAssignTask={handleAssignTask}
        onDeleteTask={handleDeleteTask}
        canManage={canManage}
      />
    ))}
  </div>
);
```

#### Step 7: TaskCard Must NOT Own Source-of-Truth State
```jsx
// TaskCard.jsx - receives task and callbacks only
export default function TaskCard({ task, onUpdateStatus, onAssign, onDelete, canManage }) {
  // TaskCard does NOT own source-of-truth state
  // State lives at page/hook level
  // TaskCard only renders task and calls callbacks
  
  return (
    <div className="task-card">
      <h4>{task.taskName}</h4>
      <TaskStatusControl 
        task={task} 
        onUpdateStatus={onUpdateStatus} 
        canManage={canManage} 
      />
      <TaskAssignControl 
        task={task} 
        onAssign={onAssign} 
        canManage={canManage} 
      />
      {canManage && (
        <button onClick={() => onDelete(task.id)}>Delete</button>
      )}
    </div>
  );
}
```

---

## 9. Permission Implementation Guide

### 9.1 Canonical Permission Keys

Use these canonical permission keys only:
- `org.overview.read`
- `org.overview.write`
- `org.workspace.access`
- `org.members.manage`
- `org.roles.view`
- `org.roles.create`
- `org.roles.update`
- `org.roles.delete`
- `org.roles.assign`
- `org.events.create`
- `org.events.manage`
- `org.departments.manage`
- `org.requests.view`
- `org.requests.review`
- `org.requests.approve`

### 9.2 normalizePermissionKeys Implementation

```javascript
// frontend/src/services/roleService.js
export function normalizePermissionKeys(response) {
  // Handle all documented response shapes
  if (Array.isArray(response)) return response;
  if (Array.isArray(response?.permissionKeys)) return response.permissionKeys;
  if (Array.isArray(response?.permissions)) return response.permissions;
  if (Array.isArray(response?.data)) return response.data;
  if (Array.isArray(response?.data?.permissionKeys)) return response.data.permissionKeys;
  if (Array.isArray(response?.data?.permissions)) return response.data.permissions;

  console.warn('[roleService] Cannot parse permissions, using safe fallback');
  return []; // Safe fallback: deny by default
}
```

### 9.3 Permission Fallback Safety

**CRITICAL**: Fallback must NEVER grant workspace access:
- Fallback returns `[]` (no permissions)
- Fallback never includes `org.workspace.access`
- Fallback never includes write/manage permissions
- If permission parse fails, user sees public/readonly UI only
- `isMember` must NOT be inferred from fallback permissions

---

## 10. Route Implementation Guide

### 10.1 VITE_API_BASE_URL Convention

```env
# frontend/.env
VITE_API_BASE_URL=http://localhost:5000/api
```

`VITE_API_BASE_URL` **already includes `/api`**.

Service paths **must NOT** include `/api`:

```javascript
// ✅ Correct
httpClient.get('/organizations');
httpClient.get(`/organizations/${orgId}/events`);

// ❌ Wrong
httpClient.get('/api/organizations');
```

### 10.2 orgId Query String Rule

All `/org/*` routes use query string `?orgId=`:

```javascript
// ✅ Correct
const [searchParams] = useSearchParams();
const orgId = searchParams.get('orgId');

// ❌ Wrong
const { orgId } = useParams();
```

`useParams()` is **only** for resource IDs in path:

```
/org/events/:eventId?orgId=
```

- `eventId` → `useParams()`
- `orgId` → `useSearchParams()`

---

## 11. Migration/DB Reminder

### 11.1 Migration Paused

Migration creation and database update are paused in Phase 3C. Do not proceed with migration unless user explicitly confirms.

### 11.2 When to Resume Migration

Only resume migration when:
1. User explicitly requests Phase 3B.3 (migration creation)
2. All domain entities are verified and locked
3. Backend implementation is ready to test with real database

### 11.3 Migration Safety

When resuming migration:
- **Do NOT drop existing database** without explicit user decision
- **Do NOT update production database** without explicit user confirmation
- **Create migration first**, review SQL, then apply
- **Backup database** before applying migration

---

## 12. Testing Strategy

### 12.1 Backend Testing
1. Test each endpoint in Swagger UI
2. Test with valid data
3. Test with invalid data (validation errors)
4. Test with missing auth (401)
5. Test with insufficient permissions (403)
6. Test edge cases (empty lists, null values, etc.)

### 12.2 Frontend Testing
1. Test loading states
2. Test error states
3. Test empty states
4. Test forbidden states (403)
5. Test CRUD operations
6. Test permission gating
7. Test EventDetail tree state management

### 12.3 Integration Testing
1. Test full user flow (register → login → create org → add members → create event → create milestone → create category → create task)
2. Test permission flow (assign role → verify permissions → test access)
3. Test error recovery (network error → retry → success)

---

## 13. Common Pitfalls to Avoid

### 13.1 Backend Pitfalls
- ❌ Exposing domain entities directly in API responses
- ❌ Not using ApiResponse wrapper
- ❌ Not handling soft-delete in queries
- ❌ Not checking permissions before operations
- ❌ Not validating input
- ❌ Not handling concurrent updates

### 13.2 Frontend Pitfalls
- ❌ Using mock data instead of real API calls
- ❌ Not handling loading/error/empty states
- ❌ Not handling 403 errors at page level
- ❌ Using `useParams()` for orgId (should use `useSearchParams()`)
- ❌ Including `/api` in service paths (VITE_API_BASE_URL already includes it)
- ❌ TaskCard owning source-of-truth state (state lives at page/hook level)
- ❌ Inventing list-by-category task endpoint (CategoryDto may include tasks[])
- ❌ Granting permissions in fallback (fallback returns [])

### 13.3 EventDetail Tree Pitfalls
- ❌ Not initializing tasks: [] when CategoryDto lacks tasks[]
- ❌ Not appending TaskDto locally after create task success
- ❌ Not mutating tree state at page/hook level for update/status/assign
- ❌ TaskCard owning source-of-truth state
- ❌ Inventing list-by-category task endpoint

---

## 14. Next Steps After Phase 3C

### Option 1: Backend Implementation (Recommended)
Start implementing backend endpoints module by module, following the recommended implementation order.

### Option 2: Frontend Visual Refinement
Refine frontend UI/UX without connecting to backend (still using TODO stubs).

### Option 3: Resume DB/Migration
Resume Phase 3B.3 to create and apply migrations, then proceed with backend implementation.

---

**End of TODO_IMPLEMENTATION_GUIDE.md**
