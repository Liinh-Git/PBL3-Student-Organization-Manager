# EventRatings Module

## Status
**DB_FOUNDATION_ONLY** - No working endpoint in base prototype

## Domain Entity Purpose
`EventRating` represents user ratings and reviews for events. Supports the `AverageRating` cached field in Event entity.

## Why It Exists in Database Foundation
EventRating is included in DB v1 to preserve the domain integrity of event feedback and rating systems. The entity and relationships are established now to support future rating and review features.

## Why No Working Endpoint is Required in Base Prototype
The base prototype focuses on core event planning. Event rating and review features are deferred to future phases. The database foundation is in place to support the AverageRating field, but no working rating UI/API is required for initial release.

## Possible Future Endpoints
- `GET /api/events/{eventId}/ratings` - List event ratings
- `POST /api/events/{eventId}/ratings` - Submit rating
- `GET /api/ratings/{id}` - Get rating details
- `PUT /api/ratings/{id}` - Update rating
- `DELETE /api/ratings/{id}` - Delete rating

## Future Features
- Rating submission form
- Rating display (stars, aspects)
- Rating statistics
- Review moderation
- Aspect-based rating (Overall, Content, Logistics, Staff, Experience)

## EXPLICIT WARNING
**DO NOT IMPLEMENT NOW**

EventRating is DB foundation only. No rating UI, API, or statistics calculation should be created in Phase 3C. Future implementation will be guided by user requirements and UX design.

## Related Domain Entities
- `EventRating` (Domain/Entities/EventRating.cs)
- `Event`, `User`
- Enums: `RatingAspect`

## Cross-layer Notes
- No shared contract in Phase 3C
- No frontend service in Phase 3C
- No frontend adapter in Phase 3C
- No frontend component in Phase 3C
- Status: **DB_FOUNDATION_ONLY**
