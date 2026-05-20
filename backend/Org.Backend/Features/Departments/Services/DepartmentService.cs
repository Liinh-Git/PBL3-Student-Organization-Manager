using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Departments.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Backend.Infrastructure.Persistence.Seed;
using Org.Shared.Features.Departments;

namespace Org.Backend.Features.Departments.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> GetOrganizationDepartmentsAsync(Guid orgId, Guid userId, CancellationToken ct = default)
    {
        // Verify user is a member of this organization
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }

        // Get all active departments with manager info
        var departments = await _context.Departments
            .Include(d => d.Manager)
                .ThenInclude(m => m!.User)
            .Where(d => d.OrgId == orgId && d.Status == DepartmentStatus.Active)
            .OrderBy(d => d.DeptName)
            .ToListAsync(ct);

        // Calculate member counts
        var departmentIds = departments.Select(d => d.Id).ToList();
        var memberCounts = await _context.Members
            .Where(m => m.DepartmentId.HasValue && departmentIds.Contains(m.DepartmentId.Value) && m.Status == MemberStatus.Active)
            .GroupBy(m => m.DepartmentId!.Value)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count, ct);

        return departments.Select(d => d.ToDepartmentDto(memberCounts.GetValueOrDefault(d.Id, 0))).ToList();
    }

    public async Task<DepartmentDto> GetDepartmentByIdAsync(Guid departmentId, Guid userId, CancellationToken ct = default)
    {
        var department = await _context.Departments
            .Include(d => d.Manager)
                .ThenInclude(m => m!.User)
            .FirstOrDefaultAsync(d => d.Id == departmentId, ct);

        if (department == null)
        {
            throw new KeyNotFoundException("Department not found");
        }

        // Verify user is a member of the department's organization
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == department.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }

        // Calculate member count
        var memberCount = await _context.Members
            .CountAsync(m => m.DepartmentId == departmentId && m.Status == MemberStatus.Active, ct);

        return department.ToDepartmentDto(memberCount);
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(Guid orgId, Guid userId, CreateDepartmentRequest request, CancellationToken ct = default)
    {
        // Verify user is active member of organization
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        // Verify user has org.departments.manage permission
        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.departments.manage");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage departments");
        }

        var actorRoleName = (member.Role.RoleName ?? string.Empty).Trim().ToLowerInvariant();
        var isLeadership = actorRoleName == "president" || actorRoleName == "vice president" || actorRoleName == "vicepresident";

        // Validate manager if provided
        if (request.ManagerId.HasValue)
        {
            if (!isLeadership)
            {
                throw new UnauthorizedAccessException("Only President/Vice President can assign manager");
            }

            var managerExists = await _context.Members
                .AnyAsync(m => m.Id == request.ManagerId.Value && m.OrgId == orgId && m.Status == MemberStatus.Active, ct);

            if (!managerExists)
            {
                throw new InvalidOperationException("Manager must be an active member of this organization");
            }

            var managerInOtherDepartment = await _context.Members
                .AnyAsync(m => m.Id == request.ManagerId.Value && m.DepartmentId.HasValue, ct);
            if (managerInOtherDepartment)
            {
                throw new InvalidOperationException("Manager already belongs to another department");
            }
        }

        // Create department
        var department = new Department
        {
            Id = Guid.NewGuid(),
            OrgId = orgId,
            DeptName = request.DepartmentName,
            Function = request.Description,
            ManagerId = request.ManagerId,
            Status = DepartmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(ct);

        // Keep manager assignment consistent with member's department assignment
        if (request.ManagerId.HasValue)
        {
            var managerMember = await _context.Members
                .Include(m => m.Role)
                .FirstOrDefaultAsync(m => m.Id == request.ManagerId.Value, ct);
            if (managerMember != null)
            {
                managerMember.DepartmentId = department.Id;
                if (!IsLeadershipRole(managerMember.Role?.RoleName))
                {
                    var managerRoleId = await _context.Roles
                        .Where(r => r.OrgId == orgId && r.RoleName == SeedConstants.ManagerRoleName)
                        .Select(r => (Guid?)r.Id)
                        .FirstOrDefaultAsync(ct);
                    if (managerRoleId.HasValue)
                    {
                        managerMember.RoleId = managerRoleId.Value;
                    }
                }
                managerMember.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        // Load manager info for response
        if (department.ManagerId.HasValue)
        {
            await _context.Entry(department)
                .Reference(d => d.Manager)
                .Query()
                .Include(m => m.User)
                .LoadAsync(ct);
        }

        var created = department.ToDepartmentDto(0);
        await NotifyDepartmentCreatedAsync(department, userId, ct);
        return created;
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(Guid departmentId, Guid userId, UpdateDepartmentRequest request, CancellationToken ct = default)
    {
        // Find department
        var department = await _context.Departments
            .Include(d => d.Manager)
                .ThenInclude(m => m!.User)
            .FirstOrDefaultAsync(d => d.Id == departmentId, ct);

        if (department == null)
        {
            throw new KeyNotFoundException("Department not found");
        }

        // Verify user is active member of organization
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == department.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        // Verify user has org.departments.manage permission or is manager of this department
        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.departments.manage");

        var isDepartmentManager = department.ManagerId.HasValue && department.ManagerId.Value == member.Id;

        if (!hasPermission && !isDepartmentManager)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage departments");
        }

        var actorRoleName = (member.Role.RoleName ?? string.Empty).Trim().ToLowerInvariant();
        var isLeadership = actorRoleName == "president" || actorRoleName == "vice president" || actorRoleName == "vicepresident";

        // Validate manager if provided
        if (request.ManagerId.HasValue)
        {
            if (!isLeadership && request.ManagerId != department.ManagerId)
            {
                throw new UnauthorizedAccessException("Only President/Vice President can change manager");
            }

            var managerExists = await _context.Members
                .AnyAsync(m => m.Id == request.ManagerId.Value && m.OrgId == department.OrgId && m.Status == MemberStatus.Active, ct);

            if (!managerExists)
            {
                throw new InvalidOperationException("Manager must be an active member of this organization");
            }

            var managerInOtherDepartment = await _context.Members
                .AnyAsync(m => m.Id == request.ManagerId.Value && m.DepartmentId.HasValue && m.DepartmentId != department.Id, ct);
            if (managerInOtherDepartment)
            {
                throw new InvalidOperationException("Manager already belongs to another department");
            }
        }

        var previousManagerId = department.ManagerId;

        // Update department
        department.DeptName = request.DepartmentName;
        department.Function = request.Description;
        department.ManagerId = request.ManagerId;
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        // Keep manager assignment consistent with member's department assignment
        if (request.ManagerId.HasValue)
        {
            var managerMember = await _context.Members
                .Include(m => m.Role)
                .FirstOrDefaultAsync(m => m.Id == request.ManagerId.Value, ct);
            if (managerMember != null)
            {
                managerMember.DepartmentId = department.Id;
                if (!IsLeadershipRole(managerMember.Role?.RoleName))
                {
                    var managerRoleId = await _context.Roles
                        .Where(r => r.OrgId == department.OrgId && r.RoleName == SeedConstants.ManagerRoleName)
                        .Select(r => (Guid?)r.Id)
                        .FirstOrDefaultAsync(ct);
                    if (managerRoleId.HasValue)
                    {
                        managerMember.RoleId = managerRoleId.Value;
                    }
                }
                managerMember.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        // If manager changed, clear old manager's department assignment only when it was this department
        if (previousManagerId.HasValue && previousManagerId != request.ManagerId)
        {
            var oldManager = await _context.Members
                .Include(m => m.Role)
                .FirstOrDefaultAsync(m => m.Id == previousManagerId.Value, ct);
            if (oldManager != null && oldManager.DepartmentId == department.Id)
            {
                oldManager.DepartmentId = null;
                var oldRoleName = (oldManager.Role?.RoleName ?? string.Empty).Trim();
                if (string.Equals(oldRoleName, SeedConstants.ManagerRoleName, StringComparison.OrdinalIgnoreCase))
                {
                    var memberRoleId = await _context.Roles
                        .Where(r => r.OrgId == department.OrgId && r.RoleName == SeedConstants.MemberRoleName)
                        .Select(r => (Guid?)r.Id)
                        .FirstOrDefaultAsync(ct);
                    if (memberRoleId.HasValue)
                    {
                        oldManager.RoleId = memberRoleId.Value;
                    }
                }
                oldManager.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        // Reload manager info if changed
        if (department.ManagerId.HasValue)
        {
            await _context.Entry(department)
                .Reference(d => d.Manager)
                .Query()
                .Include(m => m.User)
                .LoadAsync(ct);
        }

        // Calculate member count
        var memberCount = await _context.Members
            .CountAsync(m => m.DepartmentId == departmentId && m.Status == MemberStatus.Active, ct);

        return department.ToDepartmentDto(memberCount);
    }

    public async Task<bool> DeleteDepartmentAsync(Guid departmentId, Guid userId, CancellationToken ct = default)
    {
        // Find department
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId, ct);

        if (department == null)
        {
            throw new KeyNotFoundException("Department not found");
        }

        // Verify user is active member of organization
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == department.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        // Verify user has org.departments.manage permission
        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.departments.manage");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage departments");
        }

        var now = DateTime.UtcNow;
        var linkedMembers = await _context.Members
            .Where(m => m.DepartmentId == departmentId)
            .ToListAsync(ct);

        foreach (var linkedMember in linkedMembers)
        {
            linkedMember.DepartmentId = null;
            linkedMember.UpdatedAt = now;
        }

        var linkedCategories = await _context.EventCategories
            .Where(c => c.OwnerDepartmentId == departmentId)
            .ToListAsync(ct);

        foreach (var category in linkedCategories)
        {
            category.OwnerDepartmentId = null;
            category.UpdatedAt = now;
        }

        var linkedTasks = await _context.OrgTasks
            .Where(t => t.DeptId == departmentId)
            .ToListAsync(ct);

        foreach (var task in linkedTasks)
        {
            task.IsDeleted = true;
            task.DeletedAt = now;
            task.UpdatedAt = now;
        }

        // Soft delete department + archive status
        department.ManagerId = null;
        department.Status = DepartmentStatus.Archived;
        department.IsDeleted = true;
        department.DeletedAt = now;
        department.UpdatedAt = now;

        await _context.SaveChangesAsync(ct);

        return true;
    }

    private async Task NotifyDepartmentCreatedAsync(Department department, Guid actorUserId, CancellationToken ct)
    {
        var orgMembers = await _context.Members
            .Include(m => m.Role)
            .Where(m => m.OrgId == department.OrgId && m.Status == MemberStatus.Active)
            .ToListAsync(ct);

        var leaderReceiverIds = orgMembers
            .Where(m =>
            {
                var roleName = (m.Role?.RoleName ?? string.Empty).Trim().ToLowerInvariant();
                return roleName == "president" || roleName == "vice president" || roleName == "vicepresident";
            })
            .Select(m => m.UserId)
            .Distinct()
            .ToHashSet();

        if (department.ManagerId.HasValue)
        {
            var manager = orgMembers.FirstOrDefault(m => m.Id == department.ManagerId.Value);
            if (manager != null)
            {
                leaderReceiverIds.Add(manager.UserId);
            }
        }

        if (leaderReceiverIds.Count == 0)
        {
            return;
        }

        var notifications = leaderReceiverIds.Select(receiverId => new Notification
        {
            ReceiverId = receiverId,
            ActorId = actorUserId,
            Title = "Department created",
            Message = $"Department '{department.DeptName}' has been created.",
            Type = NotificationType.System,
            RelatedEntityType = nameof(Department),
            RelatedEntityId = department.Id,
            ActionUrl = $"/org/departments?orgId={department.OrgId}",
            IsRead = false
        });

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);
    }

    private static bool IsLeadershipRole(string? roleName)
    {
        var normalized = (roleName ?? string.Empty).Trim().ToLowerInvariant();
        return normalized == "president" || normalized == "vice president" || normalized == "vicepresident";
    }
}
