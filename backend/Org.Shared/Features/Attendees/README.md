# Attendees Module Contracts

## Module Purpose
Event participant/registration/check-in management (DB foundation only).

## Scope Status
**DB_FOUNDATION_ONLY** - No working contract in base prototype

## Related Domain Entity
- `Attendee`

## Why Entity Exists in DB Foundation
Attendee represents event participants, registration, and check-in. The entity exists in DB v1 to preserve the participant/registration/check-in domain model, but no working UI/API is required in the base prototype.

## Why No Working Contract is Required in Base Prototype
The base prototype focuses on the core event management flow (Event → Milestone → EventCategory → Task). Attendee management (registration, check-in, participant tracking) is a secondary feature that can be added in later phases.

## Possible Future DTOs
- `AttendeeDto`
- `RegisterAttendeeRequest`
- `CheckInAttendeeRequest`
- `UpdateAttendeeStatusRequest`

## Possible Future Endpoints
- `GET /api/events/{eventId}/attendees` - List event attendees
- `POST /api/events/{eventId}/attendees` - Register attendee
- `POST /api/attendees/{id}/check-in` - Check-in attendee
- `PUT /api/attendees/{id}/status` - Update attendee status

## Explicit Warning
**DO NOT IMPLEMENT NOW**. This is DB foundation only. No working endpoint/UI in base prototype.

---

**End of Attendees README.md**
