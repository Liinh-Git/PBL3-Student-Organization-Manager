# Phase 3 Scope Lock

## Phase 3A - Foundation Only

Phase 3A is foundation creation only. The following are explicitly NOT done in Phase 3A:

### Backend - Not Implemented
- ❌ No real endpoint implementations
- ❌ No business services
- ❌ No auth logic implementation
- ❌ No DbSet/entity relationships defined
- ❌ No migrations created
- ❌ No Database.Migrate() calls
- ❌ No admin/user/role/permission seeding
- ❌ No hardcoded connection strings
- ❌ No hardcoded JWT signing keys

### Frontend - Not Implemented
- ❌ No real page implementations
- ❌ No real service implementations
- ❌ No real API calls
- ❌ No fake data
- ❌ No mock usage
- ❌ No post/comment route/service/page
- ❌ No Blazor code copying

### Database - Not Touched
- ❌ No migration execution
- ❌ No database updates
- ❌ No production database modifications
- ❌ No seed data execution

## What Phase 3A Does Create

- ✅ Repository folder structure
- ✅ .NET solution and project files
- ✅ Backend project skeleton with packages
- ✅ Shared contracts project skeleton
- ✅ React + Vite frontend skeleton
- ✅ Placeholder files with TODO comments
- ✅ Documentation files

## Source of Truth

All work follows `PBL3_SYSTEM_DESIGN_AND_PROTOTYPE_HANDOFF_FINAL_CLEAN.md` as the single source of truth.

## Technology Lock

**Backend:**
- C# / .NET 10
- FastEndpoints
- EF Core + PostgreSQL/Npgsql
- JWT Bearer Authentication
- SignalR (optional/future)

**Frontend:**
- React + Vite + JavaScript
- React Router v6+
- Axios
- Context API

## Exclusions

- ❌ Posts/Comments - EXCLUDED from prototype
- ❌ Mock data - Never used as source of truth
- ❌ Blazor implementation - Not copied
- ❌ Messages/Finance/Reports/Resources - PROTOTYPE_ONLY placeholders only
- ❌ `/org/tasks` aggregate board - PROTOTYPE_ONLY placeholder only
