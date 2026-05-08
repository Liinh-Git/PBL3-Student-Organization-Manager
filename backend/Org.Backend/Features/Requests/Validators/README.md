# Requests Validators

## SubmitRequestRequestValidator
- RequestType: required, valid RequestType enum
- Title: optional, max 200 chars
- Content: required, max 2000 chars
- DesiredDepartmentId: optional, must be valid department
- DesiredPosition: optional, max 100 chars

## ReviewRequestRequestValidator
- Status: required, must be Approved or Rejected
- ReviewNote: optional, max 1000 chars

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
