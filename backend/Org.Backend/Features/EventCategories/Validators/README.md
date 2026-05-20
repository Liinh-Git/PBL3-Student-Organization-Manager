# EventCategories Validators

## CreateCategoryRequestValidator
- CategoryName: required, max 200 chars
- Description: optional, max 2000 chars
- OrderIndex: required, must be non-negative
- OwnerDepartmentId: optional, must be valid department

## UpdateCategoryRequestValidator
- Same rules as Create

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
