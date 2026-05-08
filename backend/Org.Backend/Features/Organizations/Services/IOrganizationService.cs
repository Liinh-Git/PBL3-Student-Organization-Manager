using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Services;

public interface IOrganizationService
{
    Task<List<OrganizationSummaryDto>> GetOrganizationsAsync(CancellationToken ct = default);
    Task<OrganizationDto> GetDefaultOrganizationAsync(Guid userId, CancellationToken ct = default);
    Task<OrganizationDto> GetOrganizationByIdAsync(Guid orgId, Guid userId, CancellationToken ct = default);
    Task<OrganizationPublicOverviewDto> GetPublicOverviewAsync(Guid orgId, CancellationToken ct = default);
    
    // Write operations
    Task<OrganizationDto> CreateOrganizationAsync(Guid userId, CreateOrganizationRequest request, CancellationToken ct = default);
    Task<OrganizationDto> UpdateOrganizationAsync(Guid orgId, Guid userId, UpdateOrganizationRequest request, CancellationToken ct = default);
}
