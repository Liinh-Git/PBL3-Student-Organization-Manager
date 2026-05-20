# EventReports Module Contracts

## Module Purpose
Event report management (DB foundation only).

## Scope Status
**DB_FOUNDATION_ONLY** - No working contract in base prototype

## Related Domain Entity
- `EventReport`

## Why Entity Exists in DB Foundation
EventReport represents event summary reports. The entity exists in DB v1 to preserve the report domain model, but no working UI/API is required in the base prototype.

## Why No Working Contract is Required in Base Prototype
The base prototype focuses on the core event management flow (Event → Milestone → EventCategory → Task). Event reporting is a secondary feature that can be added in later phases. The Reports page remains a PROTOTYPE_ONLY placeholder.

## Possible Future DTOs
- `EventReportDto`
- `CreateEventReportRequest`
- `UpdateEventReportRequest`

## Possible Future Endpoints
- `GET /api/events/{eventId}/report` - Get event report
- `POST /api/events/{eventId}/report` - Create event report
- `PUT /api/reports/{id}` - Update event report

## Explicit Warning
**DO NOT IMPLEMENT NOW**. This is DB foundation only. Reports page remains PROTOTYPE_ONLY placeholder.

---

**End of EventReports README.md**
