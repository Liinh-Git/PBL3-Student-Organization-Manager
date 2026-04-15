namespace Org.Frontend.Services.Organizations;

public interface IOrganizationContext
{
    Task<Guid> GetOrganizationIdAsync(CancellationToken ct = default);
}
