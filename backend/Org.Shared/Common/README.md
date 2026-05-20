# Org.Shared/Common

## Purpose

This folder contains common contract types used across all feature modules in the PBL3 Student Organization Manager system.

## Scope

- **DTOs/Contracts only** - No business logic
- **No entity exposure** - Never expose EF entities directly
- **No EF references** - No DbContext, no navigation properties
- **No backend service references** - Contracts are pure data shapes
- **No frontend-specific code** - Contracts are backend API shapes only

## Usage

These contracts are used to:
1. Define API response wrappers (success, error, list, paged)
2. Align Backend route outputs with Frontend service/adapter expectations
3. Provide consistent error handling shapes
4. Support pagination and filtering patterns

## Files

| File | Purpose | Status |
|---|---|---|
| `ApiResponse.cs.TODO` | Generic API response wrapper | TODO skeleton |
| `ListResponse.cs.TODO` | List response wrapper | TODO skeleton |
| `PagedRequest.cs.TODO` | Paged request parameters | TODO skeleton |
| `ErrorResponse.cs.TODO` | Error response shape | TODO skeleton |
| `ContractConventions.TODO.md` | DTO naming and design conventions | TODO documentation |

## Important Rules

1. **No entity exposure**: Never return `User`, `Organization`, `Event`, etc. entities directly. Always use DTOs.
2. **No EF references**: Contracts must not reference `DbContext`, `DbSet`, or navigation properties.
3. **No fake/default values**: If a field is missing, return `null` or omit it. Do not invent fake data.
4. **UTC datetime rule**: All datetime fields must be UTC and named with `Utc` suffix (e.g., `CreatedAtUtc`).
5. **Enum string rule**: All enums must serialize as strings, not integers.
6. **Optional field rule**: Use nullable types for optional fields. Do not use default values to indicate absence.

## Cross-layer Alignment

These common contracts are referenced by:
- **Backend**: `Org.Backend/Features/*/Endpoints/` - FastEndpoints return these shapes
- **Frontend**: `frontend/org-frontend/src/services/` - Services expect these shapes
- **Frontend**: `frontend/org-frontend/src/adapters/` - Adapters transform these DTOs to ViewModels

## NOT Implemented in Phase 3C

Phase 3C creates **skeleton/TODO files only**. Real C# DTO implementations will be created in later phases.

---

**End of Common README.md**
