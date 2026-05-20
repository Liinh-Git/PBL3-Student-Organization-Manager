# Attendees Module

## Status
**DB_FOUNDATION_ONLY** - No working endpoint in base prototype

## Domain Entity Purpose
`Attendee` represents event participants, registration, and check-in management. This is distinct from:
- `Member` = user membership in organization
- `EventMember` = internal event staff/organizer

## Why It Exists in Database Foundation
Attendee is included in DB v1 to preserve the domain integrity of event participation and registration. The entity and relationships are established now to support future attendee management features.

## Why No Working Endpoint is Required in Base Prototype
The base prototype focuses on core event planning (Event → Milestone → Category → Task). Attendee registration, check-in, and participant management are deferred to future phases. The database foundation is in place, but no working UI/API is required for initial release.

## Possible Future Endpoints
- `GET /api/events/{eventId}/attendees` - List event attendees
- `POST /api/events/{eventId}/attendees` - Register attendee
- `PUT /api/attendees/{id}/check-in` - Check-in attendee
- `DELETE /api/attendees/{id}` - Cancel registration

## Future Features
- Event registration form
- Attendee check-in system
- Guest attendee support (UserId nullable)
- Attendance statistics
- Waitlist management

## EXPLICIT WARNING
**DO NOT IMPLEMENT NOW**

Attendee is DB foundation only. No endpoint, service, validator, or mapping should be created in Phase 3C. Future implementation will be guided by user requirements and API design.

## Related Domain Entities
- `Attendee` (Domain/Entities/Attendee.cs)
- `Event`, `User`
- Enums: `AttendeeStatus`

## Cross-layer Notes
- No shared contract in Phase 3C
- No frontend service in Phase 3C
- No frontend adapter in Phase 3C
- No frontend component in Phase 3C
- Status: **DB_FOUNDATION_ONLY**
