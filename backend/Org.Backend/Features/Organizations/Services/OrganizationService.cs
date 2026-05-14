using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Organizations.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Backend.Infrastructure.Persistence.Seed;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _context;
    private readonly IHostEnvironment _hostEnvironment;

    public OrganizationService(AppDbContext context, IHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<List<OrganizationSummaryDto>> GetOrganizationsAsync(CancellationToken ct = default)
    {
        var organizations = await _context.Organizations
            .Where(o => o.Status == OrgStatus.Active)
            .OrderBy(o => o.OrgName)
            .ToListAsync(ct);

        return organizations.Select(o => o.ToOrganizationSummaryDto()).ToList();
    }

    public async Task<OrganizationDto> GetDefaultOrganizationAsync(Guid userId, CancellationToken ct = default)
    {
        // Get first organization where user is a member
        var member = await _context.Members
            .Include(m => m.Organization)
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .OrderBy(m => m.JoinDate)
            .FirstOrDefaultAsync(ct);

        if (member == null)
        {
            throw new KeyNotFoundException("No organization found for user");
        }

        return member.Organization.ToOrganizationDto();
    }

    public async Task<OrganizationDto> GetOrganizationByIdAsync(Guid orgId, Guid userId, CancellationToken ct = default)
    {
        // Verify user is a member of this organization
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (organization == null)
        {
            throw new KeyNotFoundException("Organization not found");
        }

        return organization.ToOrganizationDto();
    }

    public async Task<OrganizationPublicOverviewDto> GetPublicOverviewAsync(Guid orgId, CancellationToken ct = default)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (organization == null)
        {
            throw new KeyNotFoundException("Organization not found");
        }

        // Count public events
        var publicEventsCount = await _context.Events
            .CountAsync(e => e.OrgId == orgId && e.Visibility == EventVisibility.Public, ct);

        // Count departments
        var departmentsCount = await _context.Departments
            .CountAsync(d => d.OrgId == orgId, ct);

        return organization.ToOrganizationPublicOverviewDto(publicEventsCount, departmentsCount);
    }

    public async Task<OrganizationDto> CreateOrganizationAsync(Guid userId, CreateOrganizationRequest request, CancellationToken ct = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        
        try
        {
            // Create organization
            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                OrgName = request.OrgName.Trim(),
                Description = request.Description?.Trim(),
                AvatarUrl = request.AvatarUrl?.Trim(),
                CoverUrl = request.CoverUrl?.Trim(),
                FoundingDate = request.FoundingDate?.ToUniversalTime() ?? DateTime.UtcNow,
                Location = request.Location?.Trim(),
                ContactEmail = request.ContactEmail?.Trim(),
                ContactPhone = request.ContactPhone?.Trim(),
                TotalMembers = 1,
                Status = OrgStatus.Active,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync(ct);

            // Create default roles (President, Manager, Member)
            var presidentRole = new Role
            {
                Id = Guid.NewGuid(),
                OrgId = organization.Id,
                RoleName = SeedConstants.PresidentRoleName,
                Description = "Organization president with full permissions",
                IsDefault = true,
                Level = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var managerRole = new Role
            {
                Id = Guid.NewGuid(),
                OrgId = organization.Id,
                RoleName = SeedConstants.ManagerRoleName,
                Description = "Organization manager with management permissions",
                IsDefault = true,
                Level = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var memberRole = new Role
            {
                Id = Guid.NewGuid(),
                OrgId = organization.Id,
                RoleName = SeedConstants.MemberRoleName,
                Description = "Organization member with basic permissions",
                IsDefault = true,
                Level = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Roles.AddRange(presidentRole, managerRole, memberRole);
            await _context.SaveChangesAsync(ct);

            // Get all canonical permissions
            var permissions = await _context.Permissions
                .Where(p => SeedConstants.CanonicalPermissions.Contains(p.PermissionKey))
                .ToListAsync(ct);

            // Assign all permissions to President role
            var presidentPermissions = permissions.Select(p => new RolePermission
            {
                RoleId = presidentRole.Id,
                PermissionId = p.Id
            }).ToList();

            // Assign manager permissions to Manager role
            var managerPermissions = permissions
                .Where(p => SeedConstants.ManagerPermissions.Contains(p.PermissionKey))
                .Select(p => new RolePermission
                {
                    RoleId = managerRole.Id,
                    PermissionId = p.Id
                }).ToList();

            // Assign member permissions to Member role
            var memberPermissions = permissions
                .Where(p => SeedConstants.MemberPermissions.Contains(p.PermissionKey))
                .Select(p => new RolePermission
                {
                    RoleId = memberRole.Id,
                    PermissionId = p.Id
                }).ToList();

            _context.RolePermissions.AddRange(presidentPermissions);
            _context.RolePermissions.AddRange(managerPermissions);
            _context.RolePermissions.AddRange(memberPermissions);
            await _context.SaveChangesAsync(ct);

            // Create current user as President member
            var member = new Member
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrgId = organization.Id,
                RoleId = presidentRole.Id,
                JoinDate = DateTime.UtcNow,
                Status = MemberStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return organization.ToOrganizationDto();
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<OrganizationDto> UpdateOrganizationAsync(Guid orgId, Guid userId, UpdateOrganizationRequest request, CancellationToken ct = default)
    {
        // Verify user is a member with org.overview.write permission
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned in this organization");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.overview.write");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this organization");
        }

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (organization == null)
        {
            throw new KeyNotFoundException("Organization not found");
        }

        // Update organization fields
        organization.OrgName = request.OrgName.Trim();
        organization.Description = request.Description?.Trim();
        organization.AvatarUrl = request.AvatarUrl?.Trim();
        organization.CoverUrl = request.CoverUrl?.Trim();
        organization.FoundingDate = request.FoundingDate?.ToUniversalTime();
        organization.Location = request.Location?.Trim();
        organization.ContactEmail = request.ContactEmail?.Trim();
        organization.ContactPhone = request.ContactPhone?.Trim();
        organization.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return organization.ToOrganizationDto();
    }

    public async Task<OrganizationDto> UploadOrganizationImageAsync(
        Guid orgId,
        Guid userId,
        Stream fileStream,
        string originalFileName,
        string contentType,
        string imageType,
        CancellationToken ct = default)
    {
        var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        if (!allowedTypes.Contains(contentType))
        {
            throw new InvalidOperationException("Only jpeg, png, webp images are allowed");
        }

        // Verify user is a member with org.overview.write permission
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned in this organization");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.overview.write");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this organization");
        }

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (organization == null)
        {
            throw new KeyNotFoundException("Organization not found");
        }

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".bin"
            };
        }

        var fileName = $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var uploadsRoot = Path.Combine(_hostEnvironment.ContentRootPath, "uploads", "organizations");
        Directory.CreateDirectory(uploadsRoot);
        var absolutePath = Path.Combine(uploadsRoot, fileName);

        await using (var output = File.Create(absolutePath))
        {
            await fileStream.CopyToAsync(output, ct);
        }

        var relativeUrl = $"/uploads/organizations/{fileName}";
        if (string.Equals(imageType, "avatar", StringComparison.OrdinalIgnoreCase))
        {
            organization.AvatarUrl = relativeUrl;
        }
        else if (string.Equals(imageType, "cover", StringComparison.OrdinalIgnoreCase))
        {
            organization.CoverUrl = relativeUrl;
        }
        else
        {
            throw new InvalidOperationException("Image type must be avatar or cover");
        }

        organization.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return organization.ToOrganizationDto();
    }

    public async Task DeleteOrganizationAsync(Guid orgId, Guid userId, CancellationToken ct = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        
        try
        {
            // Verify user is a member with org.delete permission
            var member = await _context.Members
                .Include(m => m.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

            if (member == null)
            {
                throw new UnauthorizedAccessException("You are not a member of this organization");
            }

            if (member.Role == null)
            {
                throw new UnauthorizedAccessException("You do not have a role assigned in this organization");
            }

            var hasPermission = member.Role.RolePermissions
                .Any(rp => rp.Permission?.PermissionKey == "org.delete");

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this organization");
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == orgId, ct);

            if (organization == null)
            {
                throw new KeyNotFoundException("Organization not found");
            }

            // Delete in proper order to handle foreign key constraints
            // 1. Delete activity histories
            var activityHistories = await _context.ActivityHistories
                .Where(ah => ah.OrgId == orgId)
                .ToListAsync(ct);
            _context.ActivityHistories.RemoveRange(activityHistories);

            // 2. Delete requests
            var requests = await _context.Requests
                .Where(r => r.OrgId == orgId)
                .ToListAsync(ct);
            _context.Requests.RemoveRange(requests);

            // 3. Delete resources
            var resources = await _context.Resources
                .Where(r => r.OrgId == orgId)
                .ToListAsync(ct);
            _context.Resources.RemoveRange(resources);

            // 4. Delete events
            var events = await _context.Events
                .Where(e => e.OrgId == orgId)
                .ToListAsync(ct);
            _context.Events.RemoveRange(events);

            // 5. Delete departments
            var departments = await _context.Departments
                .Where(d => d.OrgId == orgId)
                .ToListAsync(ct);
            _context.Departments.RemoveRange(departments);

            // 6. Delete role permissions
            var roleIds = await _context.Roles
                .Where(r => r.OrgId == orgId)
                .Select(r => r.Id)
                .ToListAsync(ct);
            
            var rolePermissions = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .ToListAsync(ct);
            _context.RolePermissions.RemoveRange(rolePermissions);

            // 7. Delete roles
            var roles = await _context.Roles
                .Where(r => r.OrgId == orgId)
                .ToListAsync(ct);
            _context.Roles.RemoveRange(roles);

            // 8. Delete members
            var members = await _context.Members
                .Where(m => m.OrgId == orgId)
                .ToListAsync(ct);
            _context.Members.RemoveRange(members);

            // 9. Finally delete the organization
            _context.Organizations.Remove(organization);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
