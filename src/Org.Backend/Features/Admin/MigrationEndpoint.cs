// ---- Temporary endpoint to apply migrations ----
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;

namespace Org.Backend.Features.Admin;

public sealed class ApplyMigrationEndpoint : EndpointWithoutRequest
{
    public required AppDbContext Db { get; init; }

    public override void Configure()
    {
        Post("/api/admin/apply-migration");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            await Db.Database.MigrateAsync(ct);
            await Send.OkAsync("Migration applied successfully", ct);
        }
        catch (Exception ex)
        {
            ThrowError($"Error: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
