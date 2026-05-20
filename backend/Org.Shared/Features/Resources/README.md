# Resources Module Contracts

## Module Purpose
Organization resource management (DB foundation only).

## Scope Status
**DB_FOUNDATION_ONLY** - No working contract in base prototype

## Related Domain Entity
- `Resource`

## Why Entity Exists in DB Foundation
Resource represents organization resources (equipment, materials, etc.). The entity exists in DB v1 to preserve the resource domain model, but no working UI/API is required in the base prototype.

## Why No Working Contract is Required in Base Prototype
The base prototype focuses on the core event management flow (Event → Milestone → EventCategory → Task). Resource management is a secondary feature that can be added in later phases. The Resources page remains a PROTOTYPE_ONLY placeholder.

## Possible Future DTOs
- `ResourceDto`
- `CreateResourceRequest`
- `UpdateResourceRequest`
- `AllocateResourceRequest`

## Possible Future Endpoints
- `GET /api/organizations/{orgId}/resources` - List organization resources
- `POST /api/organizations/{orgId}/resources` - Create resource
- `PUT /api/resources/{id}` - Update resource
- `POST /api/resources/{id}/allocate` - Allocate resource to event

## Explicit Warning
**DO NOT IMPLEMENT NOW**. This is DB foundation only. Resources page remains PROTOTYPE_ONLY placeholder.

---

**End of Resources README.md**
