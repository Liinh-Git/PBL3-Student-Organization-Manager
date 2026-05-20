# Repository Structure Lock

## Final Folder Tree

```
PBL3-rescue/
├── PBL3-rescue.sln              # .NET solution file
├── README.md                    # Project overview
│
├── backend/
│   ├── Org.Backend/             # Main backend API project
│   │   ├── Org.Backend.csproj
│   │   ├── Program.cs           # Application entry point
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   │
│   │   ├── Domain/
│   │   │   ├── Entities/        # Domain entity classes
│   │   │   └── Enums/           # Domain enums
│   │   │
│   │   ├── Features/            # Vertical slice feature folders
│   │   │   ├── Auth/
│   │   │   ├── Users/
│   │   │   ├── Organizations/
│   │   │   ├── Members/
│   │   │   ├── Departments/
│   │   │   ├── Events/
│   │   │   ├── Milestones/
│   │   │   ├── EventCategories/
│   │   │   ├── Tasks/
│   │   │   ├── Requests/
│   │   │   ├── Notifications/
│   │   │   ├── RolesPermissions/
│   │   │   └── Common/
│   │   │
│   │   └── Infrastructure/
│   │       ├── Auth/            # JWT configuration
│   │       ├── Persistence/
│   │       │   ├── Configurations/  # EF entity configurations
│   │       │   ├── Seed/            # Seed data (not executed in Phase 3A)
│   │       │   └── AppDbContext.cs  # EF DbContext
│   │       ├── Realtime/        # SignalR hubs (optional/future)
│   │       └── Startup/         # Service registration
│   │
│   └── Org.Shared/              # Shared contracts project
│       ├── Org.Shared.csproj
│       ├── Common/              # ApiResponse, ListResponse, etc.
│       ├── Enums/               # Shared enums
│       └── Features/            # DTO contracts per module
│           ├── Auth/
│           ├── Users/
│           ├── Organizations/
│           ├── Members/
│           ├── Departments/
│           ├── Events/
│           ├── Milestones/
│           ├── EventCategories/
│           ├── Tasks/
│           ├── Requests/
│           ├── Notifications/
│           └── RolesPermissions/
│
├── frontend/            # React + Vite frontend
│       ├── package.json
│       ├── vite.config.js
│       ├── .env.example
│       ├── index.html
│       └── src/
│           ├── main.jsx
│           ├── App.jsx
│           ├── index.css
│           │
│           ├── api/
│           │   └── httpClient.js     # Centralized HTTP client
│           │
│           ├── contexts/
│           │   ├── AuthContext.jsx   # Auth state management
│           │   └── OrgContext.jsx    # Org workspace state
│           │
│           ├── hooks/
│           │   ├── useAuth.js
│           │   ├── useOrg.js
│           │   ├── usePermission.js
│           │   └── useNotifications.js
│           │
│           ├── services/           # API service layer (1 per module)
│           │   ├── authService.js
│           │   ├── userService.js
│           │   ├── organizationService.js
│           │   ├── roleService.js
│           │   ├── memberService.js
│           │   ├── eventService.js
│           │   ├── milestoneService.js
│           │   ├── categoryService.js
│           │   ├── taskService.js
│           │   ├── departmentService.js
│           │   ├── notificationService.js
│           │   ├── requestService.js
│           │   ├── friendService.js
│           │   └── discoverService.js
│           │
│           ├── adapters/           # DTO → ViewModel adapters
│           │   ├── userAdapter.js
│           │   ├── organizationAdapter.js
│           │   ├── eventAdapter.js
│           │   ├── milestoneAdapter.js
│           │   ├── categoryAdapter.js
│           │   ├── taskAdapter.js
│           │   ├── memberAdapter.js
│           │   ├── departmentAdapter.js
│           │   ├── notificationAdapter.js
│           │   └── requestAdapter.js
│           │
│           ├── router/
│           │   ├── AppRouter.jsx
│           │   ├── ProtectedRoute.jsx
│           │   └── OrgMemberRoute.jsx
│           │
│           ├── layouts/
│           │   ├── AppLayout.jsx
│           │   ├── PublicLayout.jsx
│           │   ├── Sidebar.jsx
│           │   └── TopBar.jsx
│           │
│           ├── components/
│           │   ├── shared/           # Shared UI components
│           │   │   ├── LoadingSpinner.jsx
│           │   │   ├── EmptyState.jsx
│           │   │   ├── ErrorState.jsx
│           │   │   ├── ForbiddenState.jsx
│           │   │   ├── PrototypePlaceholder.jsx
│           │   │   ├── ConfirmDialog.jsx
│           │   │   └── Pagination.jsx
│           │   ├── notifications/
│           │   │   └── NotificationBadge.jsx
│           │   ├── org/
│           │   │   ├── OrgCard.jsx
│           │   │   └── OrgSwitcher.jsx
│           │   ├── event/
│           │   │   ├── EventCard.jsx
│           │   │   └── EventStatusBadge.jsx
│           │   └── event-detail/      # EventDetail-specific components
│           │       ├── MilestonePanel.jsx
│           │       ├── CategoryPanel.jsx
│           │       ├── TaskCard.jsx
│           │       ├── TaskStatusControl.jsx
│           │       ├── TaskAssignControl.jsx
│           │       ├── MilestoneFormModal.jsx
│           │       ├── CategoryFormModal.jsx
│           │       └── TaskFormModal.jsx
│           │
│           └── pages/
│               ├── public/           # Public pages
│               ├── auth/             # Login/Register
│               ├── user/             # User workspace
│               └── org/              # Org workspace
│
└── docs/                         # Phase 3 documentation
    ├── PHASE_3A_REPO_FOUNDATION_REPORT.md
    ├── PHASE_3_SCOPE_LOCK.md
    ├── REPO_STRUCTURE_LOCK.md
    ├── DO_NOT_IMPLEMENT_YET.md
    └── NEXT_PHASE_INPUT.md
```

## Folder Purpose

### Backend (Org.Backend)
- **Purpose**: Main API backend with FastEndpoints
- **Architecture**: Vertical Slice / Feature-based
- **Tech**: C#/.NET 10, EF Core, PostgreSQL, JWT

### Shared Contracts (Org.Shared)
- **Purpose**: Shared DTOs, enums, and API contracts
- **Usage**: Referenced by both backend and frontend
- **Tech**: C# class library

### Frontend (org-frontend)
- **Purpose**: React SPA for user interface
- **Architecture**: Component-based with Context API
- **Tech**: React + Vite + JavaScript, React Router v6+

### Docs
- **Purpose**: Phase 3 documentation and scope tracking
- **Language**: Vietnamese for reports/docs
