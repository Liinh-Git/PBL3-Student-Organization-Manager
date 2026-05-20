# Users Mappings

## Overview
Mapping logic between domain entities and DTOs for user module.

## Planned Mappings

### User Entity → UserProfileDto
- Map all user fields except PasswordHash
- Convert enums to strings
- Parse JSONB fields (SocialLinks)

### Member + Organization → UserOrganizationDto
- Include organization details
- Include user's role and department in org
- Include membership status

### Event + User Participation → UserEventDto
- Include event details
- Include user's role in event (if EventMember)
- Include attendance status (if Attendee)

## NOT Implemented in Phase 3C
- ❌ No real mapping implementations
- ❌ Only README with structure notes
