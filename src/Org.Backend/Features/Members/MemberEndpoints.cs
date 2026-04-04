using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members;

public sealed class GetMembersEndpoint(AppDbContext db) : EndpointWithoutRequest<GetMembersResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{orgId:guid}/members");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");

        var members = await db.Members
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.OrgId == orgId)
            .OrderBy(x => x.User.FullName)
            .ToListAsync(ct);

        var items = members.Select(ContractMapping.ToMemberDto).ToList();
        await Send.OkAsync(new GetMembersResponse(items), ct);
    }
}

public sealed class UpdateMemberRoleEndpoint(AppDbContext db) : Endpoint<UpdateMemberRoleRequest, MemberDto>
{
    public override void Configure()
    {
        Put("/api/members/{id:guid}/role");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateMemberRoleRequest req, CancellationToken ct)
    {
        var memberId = Route<Guid>("id");

        var member = await db.Members
            .Include(x => x.User)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == memberId, ct);

        if (member is null)
            ThrowError("Member not found.", StatusCodes.Status404NotFound);

        var roleName = req.Role.ToString();

        var role = await db.Roles
            .FirstOrDefaultAsync(x => x.OrgId == member!.OrgId && x.RoleName == roleName, ct);

        if (role is null)
        {
            var newRole = new Domain.Entities.Role
            {
                OrgId = member!.OrgId,
                RoleName = roleName,
                Description = $"Auto-generated role for {roleName}"
            };

            db.Roles.Add(newRole);

            try
            {
                await db.SaveChangesAsync(ct);
                role = newRole;
            }
            catch (DbUpdateException)
            {
                db.Entry(newRole).State = EntityState.Detached;

                role = await db.Roles
                    .FirstOrDefaultAsync(x => x.OrgId == member!.OrgId && x.RoleName.ToLower() == roleName.ToLower(), ct);

                if (role is null)
                    throw;
            }
        }

        member!.RoleId = role.Id;
        await db.SaveChangesAsync(ct);

        member.Role = role;
        await Send.OkAsync(ContractMapping.ToMemberDto(member), ct);
    }
}

public sealed class UpdateMemberDepartmentEndpoint(AppDbContext db) : Endpoint<UpdateMemberDepartmentRequest, MemberDto>
{
    public override void Configure()
    {
        Put("/api/members/{id:guid}/department");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateMemberDepartmentRequest req, CancellationToken ct)
    {
        var memberId = Route<Guid>("id");

        var member = await db.Members
            .Include(x => x.User)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == memberId, ct);

        if (member is null)
            ThrowError("Member not found.", StatusCodes.Status404NotFound);

        if (req.DepartmentId is not null)
        {
            var departmentExists = await db.Departments
                .AnyAsync(x => x.Id == req.DepartmentId.Value && x.OrgId == member!.OrgId, ct);

            if (!departmentExists)
                ThrowError("Department not found in member organization.", StatusCodes.Status404NotFound);
        }

        member!.DepartmentId = req.DepartmentId;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToMemberDto(member), ct);
    }
}
