# Do Not Implement Yet - Phase 3A Restrictions

This document lists all items that MUST NOT be implemented in Phase 3A.

## Backend - Forbidden

### API & Business Logic
- ❌ No FastEndpoints endpoint implementations
- ❌ No business service implementations
- ❌ No real auth logic (JWT generation/validation)
- ❌ No authorization policies
- ❌ No permission checking logic

### Database Operations
- ❌ No EF migrations created (`dotnet ef migrations add`)
- ❌ No migration execution (`dotnet ef database update`)
- ❌ No `Database.Migrate()` calls in code
- ❌ No database seeding
- ❌ No admin user creation
- ❌ No role/permission seeding
- ❌ No production database modifications

### Configuration
- ❌ No hardcoded connection strings
- ❌ No hardcoded JWT signing keys
- ❌ No secrets in appsettings.json
- ❌ No secrets in code

### Features
- ❌ No Posts/Comments endpoints or logic
- ❌ No Messages/Chat endpoints
- ❌ No Finance endpoints
- ❌ No Reports endpoints
- ❌ No Resources endpoints

## Frontend - Forbidden

### Implementation
- ❌ No real page implementations
- ❌ No real service implementations
- ❌ No real API calls to backend
- ❌ No fake/mock data
- ❌ No mock file imports
- ❌ No Blazor code copying

### Features
- ❌ No Posts/Comments routes/pages/services
- ❌ No working Messages/Chat page
- ❌ No working Finance page
- ❌ No working Reports page
- ❌ No working Resources page
- ❌ No working `/org/tasks` aggregate board (only placeholder allowed)

### EventDetail
- ⚠️ EventDetail UI skeleton can be created
- ⚠️ But NO real task CRUD implementation in Phase 3A
- ⚠️ Task module is CORE but implementation is Phase 3B/3C

## Database - Forbidden

- ❌ No migration creation
- ❌ No migration execution
- ❌ No database updates
- ❌ No seed data execution
- ❌ No production database modifications
- ❌ No admin/role/permission seeding

## What IS Allowed in Phase 3A

✅ Folder structure creation
✅ Project file creation (.csproj, package.json)
✅ Package installation (dotnet add package, npm install)
✅ Placeholder files with TODO comments
✅ README files explaining future implementation
✅ Minimal compile-safe skeleton code
✅ Documentation files

## When to Implement

- **Phase 3B**: Domain entities, DbContext, EF configurations
- **Phase 3C**: Backend endpoints and business logic
- **Phase 3D**: Frontend pages, services, routing
- **Phase 3E**: Integration testing and refinement

## Source of Truth

All implementation must follow `PBL3_SYSTEM_DESIGN_AND_PROTOTYPE_HANDOFF_FINAL_CLEAN.md`.
