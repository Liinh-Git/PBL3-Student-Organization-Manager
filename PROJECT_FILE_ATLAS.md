# Project File Atlas (Per-File)

Generated on: 2026-04-18

Scope: all files under repository root, excluding build/runtime artifacts (bin, obj, .vs, TestResults, .git).

## Interaction Overview

- Frontend (Blazor): Components/Pages -> Services -> (Mock data store or API clients).
- Shared contracts: src/Org.Shared reused by both frontend and backend.
- Backend: Features -> Domain/Infrastructure -> PostgreSQL via EF Core.
- Tests: unit/integration projects verify backend behavior and schema assumptions.
- Docs/Scripts: support execution, governance, architecture communication, and local ops.

How to read each entry:
- Role: what this file is responsible for.
- Interactions: main upstream/downstream files or runtime touchpoints.

## .env.example

### .env.example

- Role: Template environment/config file for local setup.
- Interactions: Copied/customized to real env file consumed by runtime.


## .gitignore

### .gitignore

- Role: Git ignore ruleset.
- Interactions: Prevents transient/generated files from polluting repository history.


## Class Diagram.drawio

### Class Diagram.drawio

- Role: Diagram source file (architecture/ERD/class visualization).
- Interactions: Communicates system design and data relationships for team understanding.


## docker-compose.yml

### docker-compose.yml

- Role: Container orchestration definition for local environment services.
- Interactions: Starts required infra (database/etc.) for development and tests.


## Docs

### Docs/Auth_Module_Document.md

- Role: Documentation file: Auth_Module_Document.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/00_README_DELIVERY_PACK.md

- Role: Documentation file: 00_README_DELIVERY_PACK.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/01_BRANCH_PLAN_MINIMAL.md

- Role: Documentation file: 01_BRANCH_PLAN_MINIMAL.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/02_PROJECT_FOLDER_STRUCTURE_MAP.md

- Role: Documentation file: 02_PROJECT_FOLDER_STRUCTURE_MAP.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/03_BACKEND_EXECUTION_GUIDE.md

- Role: Documentation file: 03_BACKEND_EXECUTION_GUIDE.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/04_FE1_EXECUTION_GUIDE.md

- Role: Documentation file: 04_FE1_EXECUTION_GUIDE.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/05_FE2_EXECUTION_GUIDE.md

- Role: Documentation file: 05_FE2_EXECUTION_GUIDE.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/06_PM_TECHLEAD_REVIEW_AND_MERGE_GUIDE.md

- Role: Documentation file: 06_PM_TECHLEAD_REVIEW_AND_MERGE_GUIDE.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/07_API_CONTRACT_AND_MOCK_PLAYBOOK.md

- Role: Documentation file: 07_API_CONTRACT_AND_MOCK_PLAYBOOK.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/08_COPY_PASTE_BRIEF_MESSAGES.md

- Role: Documentation file: 08_COPY_PASTE_BRIEF_MESSAGES.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/IMPLEMENTATION_GUIDES/09_FE_MOCK_DATA_GOVERNANCE.md

- Role: Documentation file: 09_FE_MOCK_DATA_GOVERNANCE.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/Seed_Guide.md

- Role: Documentation file: Seed_Guide.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.

### Docs/TECH_STACK.md

- Role: Documentation file: TECH_STACK.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.


## DTO.md

### DTO.md

- Role: Documentation file: DTO.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.


## ERD.drawio

### ERD.drawio

- Role: Diagram source file (architecture/ERD/class visualization).
- Interactions: Communicates system design and data relationships for team understanding.


## PROJECT_FILE_ATLAS.md

### PROJECT_FILE_ATLAS.md

- Role: Documentation file: PROJECT_FILE_ATLAS.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.


## README.md

### README.md

- Role: Documentation file: README.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.


## REQUIREMENT OUTLINE.md

### REQUIREMENT OUTLINE.md

- Role: Documentation file: REQUIREMENT OUTLINE.md.
- Interactions: Guides contributors, execution steps, architecture, or process decisions.


## scripts

### scripts/dev/cleanup-local-artifacts.ps1

- Role: PowerShell automation script for local dev workflow.
- Interactions: Executes repeatable operational tasks (run/sync/cleanup).

### scripts/dev/run-be-fe.ps1

- Role: PowerShell automation script for local dev workflow.
- Interactions: Executes repeatable operational tasks (run/sync/cleanup).

### scripts/dev/sync-db.ps1

- Role: PowerShell automation script for local dev workflow.
- Interactions: Executes repeatable operational tasks (run/sync/cleanup).


## src

### src/Org.Backend/appsettings.Development.json

- Role: Application configuration file (environment/runtime settings).
- Interactions: Read by host startup and option binding across services.

### src/Org.Backend/appsettings.json

- Role: Application configuration file (environment/runtime settings).
- Interactions: Read by host startup and option binding across services.

### src/Org.Backend/Domain/Entities/ActivityHistory.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: ActivityHistory.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Attendee.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Attendee.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/BaseEntity.cs

- Role: Backend domain entity mapped by EF Core to database tables.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Department.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Department.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/DigitalAsset.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: DigitalAsset.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Event.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Event.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/EventCategory.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: EventCategory.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/EventMember.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: EventMember.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/EventReport.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: EventReport.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Member.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Member.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Milestone.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Milestone.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Organization.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Organization.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/OrgTask.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: OrgTask.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Permission.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Permission.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Request.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Request.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Resource.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Resource.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/Role.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: Role.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/RolePermission.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: RolePermission.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Entities/User.cs

- Role: Backend domain entity mapped by EF Core to database tables. Main type: User.
- Interactions: Consumed by DbContext, migrations, and feature handlers. Namespace: Org.Backend.Domain.Entities.

### src/Org.Backend/Domain/Enums/Enums.cs

- Role: Backend/shared enum definitions for constrained value sets. Main type: UserStatus.
- Interactions: Used across entities, features, DTO mapping, and validation logic. Namespace: Org.Backend.Domain.Enums.

### src/Org.Backend/Features/Auth/LoginEndpoint.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: LoginEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Auth.

### src/Org.Backend/Features/Auth/MeEndpoint.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: MeEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Auth.

### src/Org.Backend/Features/Auth/RegisterEndpoint.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: RegisterEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Auth.

### src/Org.Backend/Features/Common/ContractMapping.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model).
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Common.

### src/Org.Backend/Features/Departments/DepartmentEndpoints.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: GetDepartmentsEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Departments.

### src/Org.Backend/Features/Departments/DepartmentsFeature.Todos.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model).
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Departments.

### src/Org.Backend/Features/EventCategories/EventCategoriesFeature.Todos.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model).
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.EventCategories.

### src/Org.Backend/Features/EventCategories/EventCategoryEndpoints.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: CreateEventCategoryEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.EventCategories.

### src/Org.Backend/Features/Events/EventEndpoints.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: GetOrganizationEventsEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Events.

### src/Org.Backend/Features/Events/EventsFeature.Todos.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model).
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Events.

### src/Org.Backend/Features/Members/MemberEndpoints.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: GetMembersEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Members.

### src/Org.Backend/Features/Members/MembersFeature.Todos.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model).
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Members.

### src/Org.Backend/Features/Milestones/MilestoneEndpoints.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: CreateMilestoneEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Milestones.

### src/Org.Backend/Features/Milestones/MilestonesFeature.Todos.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model).
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Milestones.

### src/Org.Backend/Features/Organizations/OrganizationEndpoints.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: GetDefaultOrganizationEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Organizations.

### src/Org.Backend/Features/Tasks/TaskEndpoints.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: CreateTaskEndpoint.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Tasks.

### src/Org.Backend/Features/Tasks/TasksFeature.Todos.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model).
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Backend.Features.Tasks.

### src/Org.Backend/Infrastructure/Auth/IJwtTokenService.cs

- Role: Authentication/authorization infrastructure component. Main type: IJwtTokenService.
- Interactions: Works with JWT, claims, and request pipeline security. Namespace: Org.Backend.Infrastructure.Auth.

### src/Org.Backend/Infrastructure/Auth/JwtOptions.cs

- Role: Authentication/authorization infrastructure component. Main type: JwtOptions.
- Interactions: Works with JWT, claims, and request pipeline security. Namespace: Org.Backend.Infrastructure.Auth.

### src/Org.Backend/Infrastructure/Auth/JwtTokenService.cs

- Role: Authentication/authorization infrastructure component. Main type: JwtTokenService.
- Interactions: Works with JWT, claims, and request pipeline security. Namespace: Org.Backend.Infrastructure.Auth.

### src/Org.Backend/Infrastructure/Database/AppDbContext.cs

- Role: Database infrastructure: DbContext, persistence config, EF integration. Main type: AppDbContext.
- Interactions: Bridges domain models with PostgreSQL and migrations. Namespace: Org.Backend.Infrastructure.Database.

### src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs

- Role: Database infrastructure: DbContext, persistence config, EF integration. Main type: DatabaseSeeder.
- Interactions: Bridges domain models with PostgreSQL and migrations. Namespace: Org.Backend.Infrastructure.Database.

### src/Org.Backend/Infrastructure/Database/DesignTimeDbContextFactory.cs

- Role: Database infrastructure: DbContext, persistence config, EF integration. Main type: DesignTimeDbContextFactory.
- Interactions: Bridges domain models with PostgreSQL and migrations. Namespace: Org.Backend.Infrastructure.Database.

### src/Org.Backend/Infrastructure/Startup/DotEnvLoader.cs

- Role: Startup/DI configuration for service registration and middleware pipeline.
- Interactions: Binds configuration, registers services, controls app behavior at boot. Namespace: Org.Backend.Infrastructure.Startup.

### src/Org.Backend/Infrastructure/Startup/MiddlewarePipelineExtensions.cs

- Role: Startup/DI configuration for service registration and middleware pipeline. Main type: MiddlewarePipelineExtensions.
- Interactions: Binds configuration, registers services, controls app behavior at boot. Namespace: Org.Backend.Infrastructure.Startup.

### src/Org.Backend/Infrastructure/Startup/SeedModeRunner.cs

- Role: Startup/DI configuration for service registration and middleware pipeline. Main type: SeedModeRunner.
- Interactions: Binds configuration, registers services, controls app behavior at boot. Namespace: Org.Backend.Infrastructure.Startup.

### src/Org.Backend/Infrastructure/Startup/ServiceRegistrationExtensions.cs

- Role: Startup/DI configuration for service registration and middleware pipeline. Main type: ServiceRegistrationExtensions.
- Interactions: Binds configuration, registers services, controls app behavior at boot. Namespace: Org.Backend.Infrastructure.Startup.

### src/Org.Backend/Migrations/20260328045346_InitialCreate.cs

- Role: EF Core migration step capturing schema change history. Main type: InitialCreate.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Migrations/20260328045346_InitialCreate.Designer.cs

- Role: EF Core migration step capturing schema change history.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Migrations/20260328062942_AddConstraintsAndIndexes.cs

- Role: EF Core migration step capturing schema change history. Main type: AddConstraintsAndIndexes.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Migrations/20260328062942_AddConstraintsAndIndexes.Designer.cs

- Role: EF Core migration step capturing schema change history.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Migrations/20260402103306_AddEventCategoryHierarchy.cs

- Role: EF Core migration step capturing schema change history. Main type: AddEventCategoryHierarchy.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Migrations/20260402103306_AddEventCategoryHierarchy.Designer.cs

- Role: EF Core migration step capturing schema change history.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Migrations/20260404060523_AddMilestoneStartEndDateAndDepartmentCode.cs

- Role: EF Core migration step capturing schema change history. Main type: AddMilestoneStartEndDateAndDepartmentCode.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Migrations/20260404060523_AddMilestoneStartEndDateAndDepartmentCode.Designer.cs

- Role: EF Core migration step capturing schema change history.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Migrations/AppDbContextModelSnapshot.cs

- Role: EF Core migration step capturing schema change history.
- Interactions: Applied in sequence to update DB schema and constraints. Namespace: Org.Backend.Migrations.

### src/Org.Backend/Org.Backend.csproj

- Role: Project file defining target framework, dependencies, and build items.
- Interactions: Controls compilation, package restore, and output assembly behavior.

### src/Org.Backend/Org.Backend.csproj.user

- Role: User-local IDE settings file.
- Interactions: Affects local tooling behavior; should not define runtime logic.

### src/Org.Backend/Org.Backend.http

- Role: HTTP request collection for manual API testing.
- Interactions: Used with VS Code/IDE REST tools to call backend endpoints.

### src/Org.Backend/Program.cs

- Role: Application entry point: builds host and starts app.
- Interactions: Invokes startup extensions and wires full runtime pipeline.

### src/Org.Backend/Properties/launchSettings.json

- Role: JSON configuration/data file.
- Interactions: Read by runtime services, startup configuration, or mock dataset loaders.

### src/Org.Frontend/appsettings.Development.json

- Role: Application configuration file (environment/runtime settings).
- Interactions: Read by host startup and option binding across services.

### src/Org.Frontend/appsettings.json

- Role: Application configuration file (environment/runtime settings).
- Interactions: Read by host startup and option binding across services.

### src/Org.Frontend/Components/_Imports.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/App.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Auth/AuthBootstrapper.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Injected dependencies: FrontendAuthStateProvider AuthStateProvider.

### src/Org.Frontend/Components/Auth/RedirectToLogin.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Injected dependencies: NavigationManager Navigation, FrontendAuthStateProvider AuthStateProvider.

### src/Org.Frontend/Components/Layout/AuthLayout.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Injected dependencies: NavigationManager Navigation.

### src/Org.Frontend/Components/Layout/AuthLayout.razor.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/Components/Layout/EmptyLayout.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Layout/EmptyLayout.razor.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/Components/Layout/LandingLayout.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Layout/LandingLayout.razor.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/Components/Layout/MainLayout.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Injected dependencies: FrontendAuthStateProvider AuthStateProvider, NavigationManager Navigation.

### src/Org.Frontend/Components/Layout/MainLayout.razor.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/Components/Layout/NavMenu.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Layout/NavMenu.razor.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/Components/Layout/ReconnectModal.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Layout/ReconnectModal.razor.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/Components/Layout/ReconnectModal.razor.js

- Role: JavaScript asset supporting browser-side behaviors/integration.
- Interactions: Loaded by frontend host page and can bridge browser/runtime features.

### src/Org.Frontend/Components/Pages/Auth/Login.razor

- Role: Blazor page component for route '/login'.
- Interactions: Injected dependencies: AuthApiClient AuthApiClient, FrontendAuthStateProvider AuthStateProvider, NavigationManager Navigation.

### src/Org.Frontend/Components/Pages/Auth/Register.razor

- Role: Blazor page component for route '/register'.
- Interactions: Injected dependencies: AuthApiClient AuthApiClient, FrontendAuthStateProvider AuthStateProvider, NavigationManager Navigation.

### src/Org.Frontend/Components/Pages/Departments/DepartmentForm.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Pages/Departments/DepartmentList.razor

- Role: Blazor page component for route '/departments'.
- Interactions: Injected dependencies: IDepartmentService DepartmentService, IMemberService MemberService, IOrganizationContext OrganizationContext, FrontendAuthStateProvider FrontendAuthStateProvider, NavigationManager Navigation, AuthenticationStateProvider AuthStateProvider, IJSRuntime JS.

### src/Org.Frontend/Components/Pages/Error.razor

- Role: Blazor page component for route '/Error'.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Pages/Events/CreateEventDialog.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Injected dependencies: ISnackbar Snackbar.

### src/Org.Frontend/Components/Pages/Events/EventDetail.razor

- Role: Blazor page component for route '/events/{EventId:guid}'.
- Interactions: Injected dependencies: NavigationManager NavigationManager, IEventService EventService, IMilestoneService MilestoneService, IEventCategoryService CategoryService.

### src/Org.Frontend/Components/Pages/Events/EventList.razor

- Role: Blazor page component for route '/events'.
- Interactions: Injected dependencies: IEventService EventService, IOrganizationContext OrganizationContext, IDialogService DialogService, ISnackbar Snackbar, NavigationManager NavigationManager.

### src/Org.Frontend/Components/Pages/Home.razor

- Role: Blazor page component for route '/home'.
- Interactions: Injected dependencies: AuthenticationStateProvider AuthStateProvider.

### src/Org.Frontend/Components/Pages/Landing.razor

- Role: Blazor page component for route '/landing'.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Pages/Main.razor

- Role: Blazor page component for route '/'.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Pages/Main.razor.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/Components/Pages/Members/AssignRoleDialog.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Pages/Members/MemberList.razor

- Role: Blazor page component for route '/members'.
- Interactions: Injected dependencies: IMemberService MemberService, IDepartmentService DepartmentService, IOrganizationContext OrganizationContext, FrontendAuthStateProvider FrontendAuthStateProvider, NavigationManager Navigation, AuthenticationStateProvider AuthStateProvider, IJSRuntime JS.

### src/Org.Frontend/Components/Pages/Members/MemberList.razor.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/Components/Pages/NotFound.razor

- Role: Blazor page component for route '/not-found'.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Pages/Tasks/CreateTaskDialog.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Components/Pages/Tasks/TaskBoard.razor

- Role: Blazor page component for route '/events/{EventId:guid}/tasks/board/{CategoryId:guid}'.
- Interactions: Injected dependencies: ITaskService TaskService, NavigationManager NavigationManager, IDialogService DialogService, ISnackbar Snackbar.

### src/Org.Frontend/Components/Routes.razor

- Role: Blazor Razor component for UI rendering/composition.
- Interactions: Renders UI and interacts with injected services/state.

### src/Org.Frontend/Infrastructure/Auth/BlazorNoOpAuthHandler.cs

- Role: Authentication/authorization infrastructure component. Main type: BlazorNoOpAuthOptions.
- Interactions: Works with JWT, claims, and request pipeline security. Namespace: Org.Frontend.Infrastructure.Auth.

### src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs

- Role: Startup/DI configuration for service registration and middleware pipeline. Main type: FrontendStartupExtensions.
- Interactions: Binds configuration, registers services, controls app behavior at boot. Namespace: Org.Frontend.Infrastructure.Startup.

### src/Org.Frontend/Org.Frontend.csproj

- Role: Project file defining target framework, dependencies, and build items.
- Interactions: Controls compilation, package restore, and output assembly behavior.

### src/Org.Frontend/Org.Frontend.csproj.user

- Role: User-local IDE settings file.
- Interactions: Affects local tooling behavior; should not define runtime logic.

### src/Org.Frontend/Program.cs

- Role: Application entry point: builds host and starts app.
- Interactions: Invokes startup extensions and wires full runtime pipeline.

### src/Org.Frontend/Properties/launchSettings.json

- Role: JSON configuration/data file.
- Interactions: Read by runtime services, startup configuration, or mock dataset loaders.

### src/Org.Frontend/Services/Auth/AccessTokenStore.cs

- Role: C# source file in application code. Main type: IAccessTokenStore.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Auth.

### src/Org.Frontend/Services/Auth/AuthApiClient.cs

- Role: API client for calling backend endpoints. Main type: AuthApiClient.
- Interactions: Uses HttpClient, auth handler, and shared contracts for request/response. Namespace: Org.Frontend.Services.Auth.

### src/Org.Frontend/Services/Auth/AuthApiException.cs

- Role: C# source file in application code. Main type: AuthApiException.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Auth.

### src/Org.Frontend/Services/Auth/AuthHeaderDelegatingHandler.cs

- Role: C# source file in application code. Main type: AuthHeaderDelegatingHandler.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Auth.

### src/Org.Frontend/Services/Auth/FrontendAuthStateProvider.cs

- Role: C# source file in application code. Main type: FrontendAuthStateProvider.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Auth.

### src/Org.Frontend/Services/Auth/ITokenStorage.cs

- Role: C# source file in application code. Main type: ITokenStorage.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Auth.

### src/Org.Frontend/Services/Auth/LocalStorageTokenStorage.cs

- Role: C# source file in application code. Main type: LocalStorageTokenStorage.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Auth.

### src/Org.Frontend/Services/Departments/DepartmentApiClient.cs

- Role: API client for calling backend endpoints. Main type: DepartmentApiClient.
- Interactions: Uses HttpClient, auth handler, and shared contracts for request/response. Namespace: Org.Frontend.Services.Departments.

### src/Org.Frontend/Services/Departments/DepartmentMockService.cs

- Role: Frontend mock service implementing feature behavior from in-memory/mock data. Main type: DepartmentMockService.
- Interactions: Consumes FrontendMockDataStore and returns ViewModel/DTO to UI pages. Namespace: Org.Frontend.Services.Departments.

### src/Org.Frontend/Services/Departments/IDepartmentService.cs

- Role: C# source file in application code. Main type: IDepartmentService.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Departments.

### src/Org.Frontend/Services/EventCategories/EventCategoryApiClient.cs

- Role: API client for calling backend endpoints. Main type: EventCategoryApiClient.
- Interactions: Uses HttpClient, auth handler, and shared contracts for request/response. Namespace: Org.Frontend.Services.EventCategories.

### src/Org.Frontend/Services/EventCategories/EventCategoryMockService.cs

- Role: Frontend mock service implementing feature behavior from in-memory/mock data. Main type: EventCategoryMockService.
- Interactions: Consumes FrontendMockDataStore and returns ViewModel/DTO to UI pages. Namespace: Org.Frontend.Services.EventCategories.

### src/Org.Frontend/Services/EventCategories/IEventCategoryService.cs

- Role: C# source file in application code. Main type: IEventCategoryService.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.EventCategories.

### src/Org.Frontend/Services/Events/EventApiClient.cs

- Role: API client for calling backend endpoints. Main type: EventApiClient.
- Interactions: Uses HttpClient, auth handler, and shared contracts for request/response. Namespace: Org.Frontend.Services.Events.

### src/Org.Frontend/Services/Events/EventMockService.cs

- Role: Frontend mock service implementing feature behavior from in-memory/mock data. Main type: EventMockService.
- Interactions: Consumes FrontendMockDataStore and returns ViewModel/DTO to UI pages. Namespace: Org.Frontend.Services.Events.

### src/Org.Frontend/Services/Events/IEventService.cs

- Role: C# source file in application code. Main type: IEventService.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Events.

### src/Org.Frontend/Services/Members/IMemberService.cs

- Role: C# source file in application code. Main type: IMemberService.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Members.

### src/Org.Frontend/Services/Members/MemberApiClient.cs

- Role: API client for calling backend endpoints. Main type: MemberApiClient.
- Interactions: Uses HttpClient, auth handler, and shared contracts for request/response. Namespace: Org.Frontend.Services.Members.

### src/Org.Frontend/Services/Members/MemberMockService.cs

- Role: Frontend mock service implementing feature behavior from in-memory/mock data. Main type: MemberMockService.
- Interactions: Consumes FrontendMockDataStore and returns ViewModel/DTO to UI pages. Namespace: Org.Frontend.Services.Members.

### src/Org.Frontend/Services/Milestones/IMilestoneService.cs

- Role: C# source file in application code. Main type: IMilestoneService.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Milestones.

### src/Org.Frontend/Services/Milestones/MilestoneApiClient.cs

- Role: API client for calling backend endpoints. Main type: MilestoneApiClient.
- Interactions: Uses HttpClient, auth handler, and shared contracts for request/response. Namespace: Org.Frontend.Services.Milestones.

### src/Org.Frontend/Services/Milestones/MilestoneMockService.cs

- Role: Frontend mock service implementing feature behavior from in-memory/mock data. Main type: MilestoneMockService.
- Interactions: Consumes FrontendMockDataStore and returns ViewModel/DTO to UI pages. Namespace: Org.Frontend.Services.Milestones.

### src/Org.Frontend/Services/Mocks/Data/departments.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/Data/event-categories.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/Data/event-members.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/Data/events.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/Data/members.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/Data/milestones.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/Data/organizations.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/Data/tasks.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/Data/users.mock.json

- Role: Frontend mock domain data file.
- Interactions: Loaded by FrontendMockDataStore and consumed by mock services.

### src/Org.Frontend/Services/Mocks/FrontendMockDataStore.cs

- Role: Centralized loader/cache for all frontend mock data files. Main type: FrontendMockDataStore.
- Interactions: Reads JSON domains, validates topology, serves synchronized access to services. Namespace: Org.Frontend.Services.Mocks.

### src/Org.Frontend/Services/Mocks/MockDataValidator.cs

- Role: Validation rules for mock-data topology and referential consistency. Main type: MockDataValidator.
- Interactions: Executed during mock-store warmup; prevents invalid dataset runtime. Namespace: Org.Frontend.Services.Mocks.

### src/Org.Frontend/Services/Mocks/Models/MockDataModels.cs

- Role: Domain model for frontend mock dataset. Main type: MockDataSet.
- Interactions: Loaded by mock store and consumed by mock services/validator. Namespace: Org.Frontend.Services.Mocks.Models.

### src/Org.Frontend/Services/Organizations/IOrganizationContext.cs

- Role: C# source file in application code. Main type: IOrganizationContext.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Organizations.

### src/Org.Frontend/Services/Organizations/MockOrganizationContext.cs

- Role: Mock organization context resolving active org ID for FE workflows. Main type: MockOrganizationContext.
- Interactions: Reads organization list from mock store and is injected into services/pages. Namespace: Org.Frontend.Services.Organizations.

### src/Org.Frontend/Services/Organizations/OrganizationApiClient.cs

- Role: API client for calling backend endpoints. Main type: OrganizationApiClient.
- Interactions: Uses HttpClient, auth handler, and shared contracts for request/response. Namespace: Org.Frontend.Services.Organizations.

### src/Org.Frontend/Services/Tasks/ITaskService.cs

- Role: C# source file in application code. Main type: ITaskService.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Frontend.Services.Tasks.

### src/Org.Frontend/Services/Tasks/TaskApiClient.cs

- Role: API client for calling backend endpoints. Main type: TaskApiClient.
- Interactions: Uses HttpClient, auth handler, and shared contracts for request/response. Namespace: Org.Frontend.Services.Tasks.

### src/Org.Frontend/Services/Tasks/TaskMockService.cs

- Role: Frontend mock service implementing feature behavior from in-memory/mock data. Main type: TaskMockService.
- Interactions: Consumes FrontendMockDataStore and returns ViewModel/DTO to UI pages. Namespace: Org.Frontend.Services.Tasks.

### src/Org.Frontend/ViewModels/EventCategoryViewModels.cs

- Role: Frontend ViewModel definition for UI state, form input, and projection. Main type: EventCategoryViewModel.
- Interactions: Used by Razor pages/components and feature services. Namespace: Org.Frontend.ViewModels.

### src/Org.Frontend/ViewModels/EventViewModels.cs

- Role: Frontend ViewModel definition for UI state, form input, and projection. Main type: EventViewModel.
- Interactions: Used by Razor pages/components and feature services. Namespace: Org.Frontend.ViewModels.

### src/Org.Frontend/ViewModels/MilestoneViewModels.cs

- Role: Frontend ViewModel definition for UI state, form input, and projection. Main type: MilestoneViewModel.
- Interactions: Used by Razor pages/components and feature services. Namespace: Org.Frontend.ViewModels.

### src/Org.Frontend/ViewModels/TaskViewModels.cs

- Role: Frontend ViewModel definition for UI state, form input, and projection. Main type: TaskViewModel.
- Interactions: Used by Razor pages/components and feature services. Namespace: Org.Frontend.ViewModels.

### src/Org.Frontend/wwwroot/app.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/favicon.png

- Role: Image/static asset used by UI or docs.
- Interactions: Referenced by frontend pages/layout or markdown docs.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap.min.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap.min.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.min.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.min.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-grid.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-grid.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-grid.min.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-grid.min.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-grid.rtl.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-grid.rtl.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-grid.rtl.min.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-grid.rtl.min.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.min.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.min.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.rtl.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.rtl.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.rtl.min.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.rtl.min.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.min.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.min.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.min.css

- Role: Stylesheet controlling visual presentation and layout.
- Interactions: Consumed by Razor components/pages to render consistent UI.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.min.css.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.js

- Role: JavaScript asset supporting browser-side behaviors/integration.
- Interactions: Loaded by frontend host page and can bridge browser/runtime features.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.js.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js

- Role: JavaScript asset supporting browser-side behaviors/integration.
- Interactions: Loaded by frontend host page and can bridge browser/runtime features.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.esm.js

- Role: JavaScript asset supporting browser-side behaviors/integration.
- Interactions: Loaded by frontend host page and can bridge browser/runtime features.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.esm.js.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.esm.min.js

- Role: JavaScript asset supporting browser-side behaviors/integration.
- Interactions: Loaded by frontend host page and can bridge browser/runtime features.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.esm.min.js.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.js

- Role: JavaScript asset supporting browser-side behaviors/integration.
- Interactions: Loaded by frontend host page and can bridge browser/runtime features.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.js.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.min.js

- Role: JavaScript asset supporting browser-side behaviors/integration.
- Interactions: Loaded by frontend host page and can bridge browser/runtime features.

### src/Org.Frontend/wwwroot/lib/bootstrap/dist/js/bootstrap.min.js.map

- Role: Source map file for debugging minified JS/CSS assets.
- Interactions: Used by browser devtools to map bundled code back to source.

### src/Org.Shared/Class1.cs

- Role: Shared contract/feature code reused by backend and frontend. Main type: Class1.
- Interactions: Prevents duplication by centralizing common DTOs/enums/messages. Namespace: Org.Shared.

### src/Org.Shared/Common/ApiContracts.cs

- Role: Shared contract/feature code reused by backend and frontend. Main type: ListResponse<T>.
- Interactions: Prevents duplication by centralizing common DTOs/enums/messages. Namespace: Org.Shared.Common.

### src/Org.Shared/Contracts/DepartmentContracts.cs

- Role: Shared contract/feature code reused by backend and frontend. Main type: DepartmentDto.
- Interactions: Prevents duplication by centralizing common DTOs/enums/messages. Namespace: Org.Shared.Contracts.

### src/Org.Shared/Contracts/MemberContracts.cs

- Role: Shared contract/feature code reused by backend and frontend. Main type: MemberDto.
- Interactions: Prevents duplication by centralizing common DTOs/enums/messages. Namespace: Org.Shared.Contracts.

### src/Org.Shared/Enums.cs

- Role: Shared contract/feature code reused by backend and frontend. Main type: MemberRole.
- Interactions: Prevents duplication by centralizing common DTOs/enums/messages. Namespace: Org.Shared.

### src/Org.Shared/Features/Auth/AuthContracts.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: RegisterRequest.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Shared.Features.Auth.

### src/Org.Shared/Features/Departments/DepartmentContracts.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: DepartmentDto.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Shared.Features.Departments.

### src/Org.Shared/Features/EventCategories/EventCategoryContracts.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: EventCategoryDto.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Shared.Features.EventCategories.

### src/Org.Shared/Features/Events/EventContracts.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: EventDto.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Shared.Features.Events.

### src/Org.Shared/Features/Members/MemberContracts.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: MemberDto.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Shared.Features.Members.

### src/Org.Shared/Features/Milestones/MilestoneContracts.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: MilestoneDto.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Shared.Features.Milestones.

### src/Org.Shared/Features/Organizations/OrganizationContracts.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: OrganizationSummaryDto.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Shared.Features.Organizations.

### src/Org.Shared/Features/Tasks/TaskContracts.cs

- Role: Vertical-slice feature implementation (endpoint/handler/validation/model). Main type: TaskDto.
- Interactions: Coordinates DTOs, domain services, persistence, and response contracts. Namespace: Org.Shared.Features.Tasks.

### src/Org.Shared/Org.Shared.csproj

- Role: Project file defining target framework, dependencies, and build items.
- Interactions: Controls compilation, package restore, and output assembly behavior.


## StudentOrgManager.slnx

### StudentOrgManager.slnx

- Role: Solution manifest for multi-project orchestration.
- Interactions: Used by dotnet CLI/IDE to build and run all projects together.


## tests

### tests/Org.Backend.IntegrationTests/DbConnectionTests.cs

- Role: C# source file in application code. Main type: DbConnectionTests.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Backend.IntegrationTests.

### tests/Org.Backend.IntegrationTests/DbSchemaTests.cs

- Role: C# source file in application code. Main type: DbSchemaTests.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Backend.IntegrationTests.

### tests/Org.Backend.IntegrationTests/Org.Backend.IntegrationTests.csproj

- Role: Project file defining target framework, dependencies, and build items.
- Interactions: Controls compilation, package restore, and output assembly behavior.

### tests/Org.Backend.IntegrationTests/UnitTest1.cs

- Role: C# source file in application code. Main type: UnitTest1.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Backend.IntegrationTests.

### tests/Org.Backend.UnitTests/Org.Backend.UnitTests.csproj

- Role: Project file defining target framework, dependencies, and build items.
- Interactions: Controls compilation, package restore, and output assembly behavior.

### tests/Org.Backend.UnitTests/UnitTest1.cs

- Role: C# source file in application code. Main type: UnitTest1.
- Interactions: Interacts via .NET DI, namespaces, and referenced contracts. Namespace: Org.Backend.UnitTests.

