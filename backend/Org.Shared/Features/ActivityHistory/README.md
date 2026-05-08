# ActivityHistory Module Contracts

## Module Purpose
Organization activity feed/log (DB foundation only).

## Scope Status
**DB_FOUNDATION_ONLY** - No working contract in base prototype

## Related Domain Entity
- `ActivityHistory`

## Why Entity Exists in DB Foundation
ActivityHistory represents organization activity feed/log. The entity exists in DB v1 to preserve the activity tracking domain model, but no working UI/API is required in the base prototype.

## Why No Working Contract is Required in Base Prototype
The base prototype focuses on the core event management flow (Event → Milestone → EventCategory → Task). Activity feed and history tracking is a secondary feature that can be added in later phases.

## Possible Future DTOs
- `ActivityHistoryDto`
- `CreateActivityHistoryRequest`

## Possible Future Endpoints
- `GET /api/organizations/{orgId}/activities` - List organization activities
- `GET /api/organizations/{orgId}/activities/public` - List public activities

## Explicit Warning
**DO NOT IMPLEMENT NOW**. This is DB foundation only. No working feed API in base prototype.

---

**End of ActivityHistory README.md**
