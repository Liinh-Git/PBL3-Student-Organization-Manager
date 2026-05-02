// ---- Các endpoint quản lý thành viên trong tổ chức ----
// Thêm thành viên: tự tạo User nếu chưa có, hoặc tái kích hoạt nếu tài khoản đã bị xóa mềm
// Xóa thành viên: cập nhật ManagerId = null cho các phòng ban đang quản lý
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Backend.Services;
using Org.Shared.Features.Members;
using System.Security.Claims;

namespace Org.Backend.Features.Members;

// ---- GET /api/organizations/{orgId}/members — danh sách thành viên kèm user info và role ----
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

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!orgExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanRead(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

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

// ---- POST /api/organizations/{orgId}/members — thêm thành viên mới (tự tạo account nếu chưa có) ----
public sealed class CreateMemberEndpoint(AppDbContext db) : Endpoint<CreateMemberRequest, MemberDto>
{
    public override void Configure()
    {
        Post("/api/organizations/{orgId:guid}/members");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateMemberRequest req, CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");

        if (string.IsNullOrWhiteSpace(req.FullName) || req.FullName.Trim().Length < 2)
            ThrowError("FullName must be at least 2 characters.", StatusCodes.Status400BadRequest);

        var email = req.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            ThrowError("Email is invalid.", StatusCodes.Status400BadRequest);

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!orgExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (req.DepartmentId is not null)
        {
            var departmentExists = await db.Departments
                .AnyAsync(x => x.Id == req.DepartmentId.Value && x.OrgId == orgId, ct);

            if (!departmentExists)
                ThrowError("Department not found in organization.", StatusCodes.Status404NotFound);
        }

        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == email, ct);

        if (user is null)
        {
            user = new Domain.Entities.User
            {
                FullName = req.FullName.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                Status = Domain.Enums.UserStatus.Active
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }
        else if (user.IsDeleted)
        {
            user.IsDeleted = false;
        }

        var existingMember = await db.Members
            .IgnoreQueryFilters()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.OrgId == orgId, ct);

        if (existingMember is not null && !existingMember.IsDeleted)
            ThrowError("User is already a member of this organization.", StatusCodes.Status409Conflict);

        var member = existingMember ?? new Domain.Entities.Member
        {
            UserId = user.Id,
            OrgId = orgId,
            JoinDate = DateTime.UtcNow
        };

        member.IsDeleted = false;
        member.DepartmentId = req.DepartmentId;
        member.User = user;

        if (existingMember is null)
            db.Members.Add(member);

        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendAsync(ContractMapping.ToMemberDto(member), StatusCodes.Status201Created, cancellation: ct);
    }
}

// ---- PUT /api/members/{id}/role — gán/đổi vai trò thành viên (yêu cầu VicePresident+) ----
// Tự tạo Role mới trong tổ chức nếu chưa có
public sealed class UpdateMemberRoleEndpoint(AppDbContext db, INotificationService notificationService) : Endpoint<UpdateMemberRoleRequest, MemberDto>
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

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, member!.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var changerId = Guid.TryParse(userIdText, out var uid) ? uid : (Guid?)null;
        if (changerId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

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

        // Notify member about role change
        try
        {
            await notificationService.NotifyMemberRoleChanged(memberId, changerId.Value, member.OrgId, roleName);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - notification failure should not block business logic
            Console.WriteLine($"Failed to send member role change notification: {ex.Message}");
        }

        member.Role = role;
        await Send.OkAsync(ContractMapping.ToMemberDto(member), ct);
    }
}

// ---- PUT /api/members/{id}/department — phân công thành viên vào phòng ban ----
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

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, member!.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanPlan(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

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

// ---- DELETE /api/members/{id} — xóa mềm thành viên, giải phóng khỏi phòng ban ----
public sealed class DeleteMemberEndpoint(AppDbContext db, INotificationService notificationService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/members/{id:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var memberId = Route<Guid>("id");

        var member = await db.Members.FirstOrDefaultAsync(x => x.Id == memberId, ct);
        if (member is null)
            ThrowError("Member not found.", StatusCodes.Status404NotFound);

        var callerContext = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, member!.OrgId, ct);
        if (callerContext is null || !OrganizationAuthorization.CanDelete(callerContext.Value.Role))
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var removerId = Guid.TryParse(userIdText, out var uid) ? uid : (Guid?)null;
        if (removerId is null)
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var managedDepartments = await db.Departments
            .Where(x => x.ManagerId == memberId)
            .ToListAsync(ct);

        foreach (var department in managedDepartments)
        {
            department.ManagerId = null;
        }

        member!.IsDeleted = true;
        member.DepartmentId = null;

        await db.SaveChangesAsync(ct);

        // Notify member about removal
        try
        {
            await notificationService.NotifyMemberRemoved(member.UserId, removerId.Value, member.OrgId);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - notification failure should not block business logic
            Console.WriteLine($"Failed to send member removal notification: {ex.Message}");
        }

        await Send.NoContentAsync(ct);
    }
}

// ---- POST /api/organizations/{orgId}/leave — current authenticated user leaves organization ----
public sealed class LeaveOrganizationEndpoint(AppDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/organizations/{orgId:guid}/leave");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("orgId");
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            ThrowError("Invalid token subject.", StatusCodes.Status401Unauthorized);

        var membership = await db.Members
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.UserId == userId, ct);

        if (membership is null)
            ThrowError("Membership not found.", StatusCodes.Status404NotFound);

        var roleName = membership.Role?.RoleName?.Trim();
        var isTopLeader = roleName is not null
            && (string.Equals(roleName, "President", StringComparison.OrdinalIgnoreCase)
                || string.Equals(roleName, "Owner", StringComparison.OrdinalIgnoreCase)
                || string.Equals(roleName, "Chairman", StringComparison.OrdinalIgnoreCase)
                || string.Equals(roleName, "Founder", StringComparison.OrdinalIgnoreCase));

        if (isTopLeader)
        {
            var otherTopLeaderExists = await db.Members
                .AsNoTracking()
                .Include(x => x.Role)
                .AnyAsync(x =>
                    x.OrgId == orgId
                    && x.Id != membership.Id
                    && x.Role != null
                    && (
                        x.Role.RoleName.ToLower() == "president"
                        || x.Role.RoleName.ToLower() == "owner"
                        || x.Role.RoleName.ToLower() == "chairman"
                        || x.Role.RoleName.ToLower() == "founder"), ct);

            if (!otherTopLeaderExists)
            {
                ThrowError("Cannot leave organization as the last primary leader. Transfer ownership first.", StatusCodes.Status409Conflict);
            }
        }

        var managedDepartments = await db.Departments
            .Where(x => x.ManagerId == membership.Id)
            .ToListAsync(ct);

        foreach (var department in managedDepartments)
        {
            department.ManagerId = null;
        }

        membership.DepartmentId = null;
        membership.IsDeleted = true;

        var org = await db.Organizations.FirstOrDefaultAsync(x => x.Id == orgId, ct);
        if (org is not null)
        {
            var remainingMembers = await db.Members.CountAsync(x => x.OrgId == orgId && !x.IsDeleted && x.Id != membership.Id, ct);
            org.TotalMembers = Math.Max(remainingMembers, 0);
        }

        await db.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
