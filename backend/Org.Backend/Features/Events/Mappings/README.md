# Events Mappings

## Event Entity → EventDto
- Map all event fields including Location, TargetParticipants, Budget, AverageRating, Tags
- Convert EventStatus and EventVisibility enums to strings
- Parse Tags JSONB field
- Include milestone count, category count, task count
- Include creator member details

## Event Entity → PublicEventDto
- Map public fields only
- Exclude internal planning details
- Include public statistics

## NOT Implemented in Phase 3C
- ❌ No real mapping implementations
