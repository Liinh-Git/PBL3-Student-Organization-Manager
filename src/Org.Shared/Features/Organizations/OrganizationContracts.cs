namespace Org.Shared.Features.Organizations;

public sealed record OrganizationSummaryDto(
    Guid Id,
    string Name,
    string? Description);

public sealed record GetDefaultOrganizationResponse(OrganizationSummaryDto Data);
