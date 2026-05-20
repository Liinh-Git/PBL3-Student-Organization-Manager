# EventMembers Module Contracts

## Module Purpose
Event staff/organizer management (DB foundation only).

## Scope Status
**DB_FOUNDATION_ONLY** - No working contract in base prototype

## Related Domain Entity
- `EventMember`

## Why Entity Exists in DB Foundation
EventMember represents event staff/organizer roles within an event. The entity exists in DB v1 to preserve the event staff/organizer domain model, but no working UI/API is required in the base prototype.

## Why No Working Contract is Required in Base Prototype
The base prototype focuses on the core event management flow (Event → Milestone → EventCategory → Task). EventMember management (assigning staff roles, managing event team) is a secondary feature that can be added in later phases.

## Possible Future DTOs
- `EventMemberDto`
- `AddEventMemberRequest`
- `UpdateEventMemberRoleRequest`
- `RemoveEventMemberRequest`

## Possible Future Endpoints
- `GET /api/events/{eventId}/members` - List event staff
- `POST /api/events/{eventId}/members` - Add event staff
- `PUT /api/event-members/{id}/role` - Update event role
- `DELETE /api/event-members/{id}` - Remove event staff

## Explicit Warning
**DO NOT IMPLEMENT NOW**. This is DB foundation only. No working endpoint/UI in base prototype.

---

**End of EventMembers README.md**
