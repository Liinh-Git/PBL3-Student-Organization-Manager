# EventRatings Module Contracts

## Module Purpose
Event rating management (DB foundation only).

## Scope Status
**DB_FOUNDATION_ONLY** - No working contract in base prototype

## Related Domain Entity
- `EventRating`

## Why Entity Exists in DB Foundation
EventRating represents user ratings for events. The entity exists in DB v1 to support the `AverageRating` field in Event entity, but no working UI/API is required in the base prototype.

## Why No Working Contract is Required in Base Prototype
The base prototype focuses on the core event management flow (Event → Milestone → EventCategory → Task). Event rating and feedback is a secondary feature that can be added in later phases.

## Possible Future DTOs
- `EventRatingDto`
- `CreateEventRatingRequest`
- `UpdateEventRatingRequest`

## Possible Future Endpoints
- `GET /api/events/{eventId}/ratings` - List event ratings
- `POST /api/events/{eventId}/ratings` - Create rating
- `PUT /api/ratings/{id}` - Update rating

## Explicit Warning
**DO NOT IMPLEMENT NOW**. This is DB foundation only. No EventRating UI/API in base prototype.

---

**End of EventRatings README.md**
