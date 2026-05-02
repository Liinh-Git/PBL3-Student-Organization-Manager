using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;
using Org.Shared;
using System.Security.Claims;

namespace Org.Backend.Features.Common;

internal readonly record struct OrganizationMemberContext(Guid MemberId, Guid? RoleId, MemberRole Role);

internal static class OrganizationAuthorization
{
    public static async Task<OrganizationMemberContext?> ResolveCallerContextAsync(
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

        return new OrganizationMemberContext(membership.Id, membership.RoleId, ParseRoleName(membership.Role?.RoleName));
    }

    public static bool CanRead(MemberRole role) => role >= MemberRole.Member;

    public static bool CanPlan(MemberRole role) => role >= MemberRole.Manager;

    public static bool CanDelete(MemberRole role) => role >= MemberRole.VicePresident;

    public static MemberRole ParseRoleName(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return MemberRole.Member;

        return Enum.TryParse<MemberRole>(roleName, ignoreCase: true, out var parsed)
            ? parsed
            : MemberRole.Member;
    }
}
