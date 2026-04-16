namespace Org.Frontend.Services.Organizations;

public sealed class MockOrganizationContext : IOrganizationContext
{
    private static readonly Guid MockOrganizationId = Guid.Parse("97b12d10-8368-4c55-abdb-69490895f4f3");

    public Task<Guid> GetOrganizationIdAsync(CancellationToken ct = default)
        => Task.FromResult(MockOrganizationId);

    public Task ResetAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
