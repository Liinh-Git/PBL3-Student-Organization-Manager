# Organizations Services

## IOrganizationService / OrganizationService
**Methods**:
- `Task<List<OrganizationDto>> ListOrganizationsAsync(Guid userId)`
- `Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationRequest request, Guid userId)`
- `Task<OrganizationDto> GetDefaultOrganizationAsync(Guid userId)`
- `Task<OrganizationDto> GetOrganizationAsync(Guid orgId, Guid userId)`
- `Task<OrganizationDto> UpdateOrganizationAsync(Guid orgId, UpdateOrganizationRequest request, Guid userId)`
- `Task<PublicOrganizationDto> GetPublicOverviewAsync(Guid orgId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
