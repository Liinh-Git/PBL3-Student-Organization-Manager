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
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }

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

        var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var existingMember = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == request.UserId && m.OrgId == orgId && m.Status == MemberStatus.Active, ct);

        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already an active member of this organization");
        }

        Guid? roleId = request.RoleId;
        if (roleId.HasValue)
        {
            var roleExists = await _context.Roles
                .AnyAsync(r => r.Id == roleId.Value && r.OrgId == orgId, ct);

            if (!roleExists)
            {
                throw new InvalidOperationException("Role not found in this organization");
            }
        }
        else
        {
            var defaultRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.OrgId == orgId && r.RoleName == "Member" && r.IsDefault, ct);

            roleId = defaultRole?.Id;
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value && d.OrgId == orgId, ct);

            if (!departmentExists)
            {
                throw new InvalidOperationException("Department not found in this organization");
            }
        }

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
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == orgId, ct);
        if (org != null)
        {
            org.TotalMembers++;
            org.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(ct);

        var createdMember = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Role)
            .Include(m => m.Department)
            .FirstAsync(m => m.Id == member.Id, ct);

        return createdMember.ToMemberDto();
    }

    public async Task<MemberDto> UpdateMemberDepartmentAsync(Guid memberId, Guid userId, UpdateMemberDepartmentRequest request, CancellationToken ct = default)
    {
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

        var hasOrgPermission = currentMember.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.members.manage");

        var managerDepartmentId = await _context.Departments
            .Where(d => d.ManagerId == currentMember.Id && d.Status == DepartmentStatus.Active)
            .Select(d => (Guid?)d.Id)
            .FirstOrDefaultAsync(ct);

        var targetDepartmentId = request.DepartmentId;

        var managerCanManage = false;
        if (managerDepartmentId.HasValue)
        {
            if (targetDepartmentId.HasValue)
            {
                managerCanManage = targetDepartmentId.Value == managerDepartmentId.Value;
            }
            else if (member.DepartmentId.HasValue)
            {
                managerCanManage = member.DepartmentId.Value == managerDepartmentId.Value;
            }
        }

        if (!hasOrgPermission && !managerCanManage)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this member for the selected department");
        }

        if (targetDepartmentId.HasValue)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == targetDepartmentId.Value && d.OrgId == orgId, ct);

            if (!departmentExists)
            {
                throw new InvalidOperationException("Department not found in this organization");
            }

            if (member.DepartmentId.HasValue && member.DepartmentId.Value != targetDepartmentId.Value)
            {
                throw new InvalidOperationException("Member already belongs to another department");
            }
        }

        var previousDepartmentId = member.DepartmentId;
        member.DepartmentId = targetDepartmentId;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        if (previousDepartmentId != targetDepartmentId)
        {
            var departmentName = targetDepartmentId.HasValue
                ? await _context.Departments.Where(d => d.Id == targetDepartmentId.Value).Select(d => d.DeptName).FirstOrDefaultAsync(ct)
                : null;

            _context.Notifications.Add(new Notification
            {
                ReceiverId = member.UserId,
                ActorId = userId,
                Title = targetDepartmentId.HasValue ? "Department assignment updated" : "Department assignment removed",
                Message = targetDepartmentId.HasValue
                    ? $"You have been assigned to department '{departmentName ?? "Unknown"}'."
                    : "You have been removed from your department.",
                Type = NotificationType.System,
                RelatedEntityType = nameof(Member),
                RelatedEntityId = member.Id,
                ActionUrl = $"/org/members?orgId={orgId}",
                IsRead = false
            });
            await _context.SaveChangesAsync(ct);

            if (targetDepartmentId.HasValue)
            {
                var managerUserId = await _context.Departments
                    .Where(d => d.Id == targetDepartmentId.Value)
                    .Select(d => d.Manager != null ? d.Manager.UserId : (Guid?)null)
                    .FirstOrDefaultAsync(ct);

                if (managerUserId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        ReceiverId = managerUserId.Value,
                        ActorId = userId,
                        Title = "Member added to your department",
                        Message = $"{member.User.FullName} was assigned to your department '{departmentName ?? "Unknown"}'.",
                        Type = NotificationType.System,
                        RelatedEntityType = nameof(Member),
                        RelatedEntityId = member.Id,
                        ActionUrl = $"/org/members?orgId={orgId}",
                        IsRead = false
                    });
                }
            }

            if (previousDepartmentId.HasValue && (!targetDepartmentId.HasValue || previousDepartmentId.Value != targetDepartmentId.Value))
            {
                var oldDepartmentName = await _context.Departments
                    .Where(d => d.Id == previousDepartmentId.Value)
                    .Select(d => d.DeptName)
                    .FirstOrDefaultAsync(ct);

                var oldManagerUserId = await _context.Departments
                    .Where(d => d.Id == previousDepartmentId.Value)
                    .Select(d => d.Manager != null ? d.Manager.UserId : (Guid?)null)
                    .FirstOrDefaultAsync(ct);

                if (oldManagerUserId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        ReceiverId = oldManagerUserId.Value,
                        ActorId = userId,
                        Title = "Member removed from your department",
                        Message = $"{member.User.FullName} was removed from your department '{oldDepartmentName ?? "Unknown"}'.",
                        Type = NotificationType.System,
                        RelatedEntityType = nameof(Member),
                        RelatedEntityId = member.Id,
                        ActionUrl = $"/org/members?orgId={orgId}",
                        IsRead = false
                    });
                }
            }

            await _context.SaveChangesAsync(ct);
        }

        var updatedMember = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Role)
            .Include(m => m.Department)
            .FirstAsync(m => m.Id == memberId, ct);

        return updatedMember.ToMemberDto();
    }

    public async Task<bool> RemoveMemberAsync(Guid memberId, Guid userId, RemoveMemberRequest? request, CancellationToken ct = default)
    {
        var member = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.Id == memberId, ct);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found");
        }

        var orgId = member.OrgId;

        var currentMember = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (currentMember == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        var isSelfLeave = currentMember.Id == member.Id;

        if (isSelfLeave)
        {
            var roleName = (member.Role?.RoleName ?? string.Empty).Trim().ToLowerInvariant();
            if (roleName == "president" || roleName == "vice president" || roleName == "vicepresident")
            {
                throw new InvalidOperationException("Leadership roles cannot leave organization directly");
            }
        }
        else
        {
            if (currentMember.Role == null)
            {
                throw new UnauthorizedAccessException("You do not have a role assigned");
            }

            var hasPermission = currentMember.Role.RolePermissions
                .Any(rp => rp.Permission?.PermissionKey == "org.members.manage");

            var managerDepartmentId = await _context.Departments
                .Where(d => d.ManagerId == currentMember.Id && d.Status == DepartmentStatus.Active)
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync(ct);

            var managerCanManage = managerDepartmentId.HasValue && member.DepartmentId.HasValue && managerDepartmentId.Value == member.DepartmentId.Value;

            if (!hasPermission && !managerCanManage)
            {
                throw new UnauthorizedAccessException("You do not have permission to manage members");
            }
        }

        member.Status = MemberStatus.Removed;
        member.UpdatedAt = DateTime.UtcNow;
        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == orgId, ct);
        if (organization != null)
        {
            organization.TotalMembers = Math.Max(0, organization.TotalMembers - 1);
            organization.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(ct);

        var actionByName = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct) ?? "A member";

        var orgName = await _context.Organizations
            .Where(o => o.Id == orgId)
            .Select(o => o.OrgName)
            .FirstOrDefaultAsync(ct) ?? "organization";

        var reason = string.IsNullOrWhiteSpace(request?.Reason)
            ? "No reason provided."
            : request!.Reason!.Trim();

        var message = isSelfLeave
            ? $"You have left {orgName}. Reason: {reason}"
            : $"You were removed from {orgName} by {actionByName}. Reason: {reason}";

        _context.Notifications.Add(new Notification
        {
            ReceiverId = member.UserId,
            ActorId = userId,
            Title = isSelfLeave ? "Left organization" : "Removed from organization",
            Message = message,
            Type = NotificationType.System,
            RelatedEntityType = nameof(Member),
            RelatedEntityId = member.Id,
            ActionUrl = "/user/organizations",
            IsRead = false
        });

        if (isSelfLeave)
        {
            var leaders = await _context.Members
                .Include(m => m.Role)
                .Include(m => m.User)
                .Where(m => m.OrgId == orgId && m.Status == MemberStatus.Active)
                .ToListAsync(ct);

            var leaderNotifications = leaders
                .Where(m =>
                {
                    var roleName = (m.Role?.RoleName ?? string.Empty).Trim().ToLowerInvariant();
                    return roleName == "president" || roleName == "vice president" || roleName == "vicepresident";
                })
                .Select(m => new Notification
                {
                    ReceiverId = m.UserId,
                    ActorId = userId,
                    Title = "Member left organization",
                    Message = $"{member.User?.FullName ?? "A member"} has left {orgName}. Reason: {reason}",
                    Type = NotificationType.System,
                    RelatedEntityType = nameof(Member),
                    RelatedEntityId = member.Id,
                    ActionUrl = $"/org/members?orgId={orgId}",
                    IsRead = false
                })
                .ToList();

            if (leaderNotifications.Count > 0)
            {
                _context.Notifications.AddRange(leaderNotifications);
            }
        }

        await _context.SaveChangesAsync(ct);

        return true;
    }
}
