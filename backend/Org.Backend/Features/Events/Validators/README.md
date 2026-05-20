# Events Validators

## CreateEventRequestValidator
- EventName: required, max 200 chars
- Description: optional, max 5000 chars
- StartDate: required, must be valid date
- EndDate: required, must be after StartDate
- Budget: optional, must be positive
- Location: optional, max 500 chars
- TargetParticipants: optional, must be positive
- Tags: optional, valid JSON array
- Status: required, valid EventStatus enum
- Visibility: required, valid EventVisibility enum

## UpdateEventRequestValidator
- Same rules as Create

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
