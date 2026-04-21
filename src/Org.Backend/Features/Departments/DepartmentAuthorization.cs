using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using System.Security.Claims;

namespace Org.Backend.Features.Departments;

internal static class DepartmentAuthorization
{
    public static async Task<MemberRole?> ResolveCallerRoleAsync(
        AppDbContext db,
        ClaimsPrincipal user,
        Guid orgId,
        CancellationToken ct)
    {
        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            return null;

        var membership = await db.Members
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.OrgId == orgId, ct);

        if (membership is null)
            return null;

        return ParseRole(membership.Role?.RoleName);
    }

    public static bool CanRead(MemberRole role)   => role >= MemberRole.Member;
    public static bool CanWrite(MemberRole role)  => role >= MemberRole.Manager;
    public static bool CanAssign(MemberRole role) => role >= MemberRole.Manager;
    public static bool CanDelete(MemberRole role) => role >= MemberRole.VicePresident;

    public static int TaskOrder(Org.Shared.TaskStatus status)
        => status switch
        {
            Org.Shared.TaskStatus.InProgress => 0,
            Org.Shared.TaskStatus.Todo       => 1,
            Org.Shared.TaskStatus.Done       => 2,
            _                                => 3
        };

    private static MemberRole ParseRole(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return MemberRole.Member;

        return Enum.TryParse<MemberRole>(roleName, ignoreCase: true, out var parsed)
            ? parsed
            : MemberRole.Member;
    }
}
