# Milestones Validators

## CreateMilestoneRequestValidator
- Title: required, max 200 chars
- Description: optional, max 2000 chars
- OrderIndex: required, must be non-negative
- StartDate: optional, valid date
- EndDate: optional, must be after StartDate
- Status: required, valid MilestoneStatus enum

## UpdateMilestoneRequestValidator
- Same rules as Create

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
