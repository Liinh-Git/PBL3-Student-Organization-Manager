using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Members.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Members;

namespace Org.Backend.Features.Members.Services;

public class MemberService : IMemberService
{
    private readonly AppDbContext _context;

    public MemberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MemberDto>> GetOrganizationMembersAsync(Guid orgId, Guid userId, CancellationToken ct = default)
    {
        // Verify user is a member of this organization
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }

        // Get all active members with related data
        var members = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Role)
            .Include(m => m.Department)
            .Where(m => m.OrgId == orgId && m.Status == MemberStatus.Active)
            .OrderBy(m => m.JoinDate)
            .ToListAsync(ct);

        return members.Select(m => m.ToMemberDto()).ToList();
    }

    public async Task<MemberDto> AddMemberAsync(Guid orgId, Guid userId, AddMemberRequest request, CancellationToken ct = default)
    {
        // Verify user is active member and has org.members.manage permission
        var currentMember = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (currentMember == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (currentMember.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = currentMember.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.members.manage");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage members");
        }

        // Verify user exists
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if user is already an active member
        var existingMember = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == request.UserId && m.OrgId == orgId && m.Status == MemberStatus.Active, ct);

        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already an active member of this organization");
        }

        // Resolve role
        Guid? roleId = request.RoleId;
        if (roleId.HasValue)
        {
            // Verify role belongs to same organization
            var roleExists = await _context.Roles
                .AnyAsync(r => r.Id == roleId.Value && r.OrgId == orgId, ct);

            if (!roleExists)
            {
                throw new InvalidOperationException("Role not found in this organization");
            }
        }
        else
        {
            // Assign default Member role
            var defaultRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.OrgId == orgId && r.RoleName == "Member" && r.IsDefault, ct);

            roleId = defaultRole?.Id;
        }

        // Verify department if provided
        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value && d.OrgId == orgId, ct);

            if (!departmentExists)
            {
                throw new InvalidOperationException("Department not found in this organization");
            }
        }

        // Create member
        var member = new Member
        {
            UserId = request.UserId,
            OrgId = orgId,
            RoleId = roleId,
            DepartmentId = request.DepartmentId,
            StudentCode = request.StudentCode,
            JoinDate = DateTime.UtcNow,
            Status = MemberStatus.Active
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync(ct);

        // Reload member with related data
        var createdMember = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Role)
            .Include(m => m.Department)
            .FirstAsync(m => m.Id == member.Id, ct);

        return createdMember.ToMemberDto();
    }

    public async Task<MemberDto> UpdateMemberDepartmentAsync(Guid memberId, Guid userId, UpdateMemberDepartmentRequest request, CancellationToken ct = default)
    {
        // Find member and resolve orgId
        var member = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Role)
            .Include(m => m.Department)
            .FirstOrDefaultAsync(m => m.Id == memberId, ct);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found");
        }

        var orgId = member.OrgId;

        // Verify user is active member and has org.members.manage permission
        var currentMember = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (currentMember == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (currentMember.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = currentMember.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.members.manage");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage members");
        }

        // Verify department if provided
        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value && d.OrgId == orgId, ct);

            if (!departmentExists)
            {
                throw new InvalidOperationException("Department not found in this organization");
            }
        }

        // Update department
        member.DepartmentId = request.DepartmentId;
        await _context.SaveChangesAsync(ct);

        // Reload member with updated department
        var updatedMember = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Role)
            .Include(m => m.Department)
            .FirstAsync(m => m.Id == memberId, ct);

        return updatedMember.ToMemberDto();
    }

    public async Task<bool> RemoveMemberAsync(Guid memberId, Guid userId, CancellationToken ct = default)
    {
        // Find member and resolve orgId
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId, ct);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found");
        }

        var orgId = member.OrgId;

        // Verify user is active member and has org.members.manage permission
        var currentMember = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (currentMember == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (currentMember.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = currentMember.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.members.manage");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage members");
        }

        // Soft delete: set status to Removed
        member.Status = MemberStatus.Removed;
        await _context.SaveChangesAsync(ct);

        return true;
    }
}
