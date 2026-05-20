# EventMembers Module

## Status
**DB_FOUNDATION_ONLY** - No working endpoint in base prototype

## Domain Entity Purpose
`EventMember` represents internal staff/organizer roles within an event. This is distinct from:
- `Member` = user membership in organization
- `Attendee` = event participant/registration/check-in

## Why It Exists in Database Foundation
EventMember is included in DB v1 to preserve the domain integrity of event staff/organizer management. The entity and relationships are established now to support future event permission and role features.

## Why No Working Endpoint is Required in Base Prototype
The base prototype focuses on core event planning (Event → Milestone → Category → Task). Event staff management and internal event roles are deferred to future phases. The database foundation is in place, but no working UI/API is required for initial release.

## Possible Future Endpoints
- `GET /api/events/{eventId}/members` - List event staff
- `POST /api/events/{eventId}/members` - Add staff to event
- `PUT /api/event-members/{id}/role` - Update staff role
- `DELETE /api/event-members/{id}` - Remove staff from event

## Future Features
- Event-level permission system (currentUserEventRole, canManage)
- Event staff dashboard
- Staff role assignment (Manager, CoManager, Staff, Volunteer, Support)
- Staff-specific notifications

## EXPLICIT WARNING
**DO NOT IMPLEMENT NOW**

EventMember is DB foundation only. No endpoint, service, validator, or mapping should be created in Phase 3C. Future implementation will be guided by user requirements and API design.

## Related Domain Entities
- `EventMember` (Domain/Entities/EventMember.cs)
- `Event`, `Member`
- Enums: `EventRole`

## Cross-layer Notes
- No shared contract in Phase 3C
- No frontend service in Phase 3C
- No frontend adapter in Phase 3C
- No frontend component in Phase 3C
- Status: **DB_FOUNDATION_ONLY**
