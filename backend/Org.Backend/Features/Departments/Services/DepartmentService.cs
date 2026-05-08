using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Departments.Mappings;
using Org.Backend.Infrastructure.Persistence;
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

        // Validate manager if provided
        if (request.ManagerId.HasValue)
        {
            var managerExists = await _context.Members
                .AnyAsync(m => m.Id == request.ManagerId.Value && m.OrgId == orgId && m.Status == MemberStatus.Active, ct);

            if (!managerExists)
            {
                throw new InvalidOperationException("Manager must be an active member of this organization");
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

        // Load manager info for response
        if (department.ManagerId.HasValue)
        {
            await _context.Entry(department)
                .Reference(d => d.Manager)
                .Query()
                .Include(m => m.User)
                .LoadAsync(ct);
        }

        return department.ToDepartmentDto(0);
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

        // Validate manager if provided
        if (request.ManagerId.HasValue)
        {
            var managerExists = await _context.Members
                .AnyAsync(m => m.Id == request.ManagerId.Value && m.OrgId == department.OrgId && m.Status == MemberStatus.Active, ct);

            if (!managerExists)
            {
                throw new InvalidOperationException("Manager must be an active member of this organization");
            }
        }

        // Update department
        department.DeptName = request.DepartmentName;
        department.Function = request.Description;
        department.ManagerId = request.ManagerId;
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

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

        // Check if department has active members
        var activeMemberCount = await _context.Members
            .CountAsync(m => m.DepartmentId == departmentId && m.Status == MemberStatus.Active, ct);

        if (activeMemberCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete department with {activeMemberCount} active member(s) assigned");
        }

        // Soft delete: set status to Archived
        department.Status = DepartmentStatus.Archived;
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return true;
    }
}
