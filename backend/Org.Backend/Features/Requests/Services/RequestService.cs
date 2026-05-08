using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Requests.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Services;

public class RequestService : IRequestService
{
    private readonly AppDbContext _context;

    public RequestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RequestDto>> GetOrganizationRequestsAsync(Guid userId, Guid orgId, CancellationToken ct = default)
    {
        // Verify user has permission to view requests (org.requests.view)
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.requests.view");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to view requests");
        }

        // Get all requests for this organization
        var requests = await _context.Requests
            .Include(r => r.Sender)
            .Include(r => r.Organization)
            .Include(r => r.DesiredDepartment)
            .Include(r => r.ReviewedByMember)
                .ThenInclude(m => m!.User)
            .Where(r => r.OrgId == orgId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return requests.Select(r => r.ToRequestDto()).ToList();
    }

    public async Task<RequestDto> CreateRequestAsync(Guid userId, Guid orgId, CreateRequestRequest request, CancellationToken ct = default)
    {
        // Verify organization exists
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (organization == null)
        {
            throw new KeyNotFoundException("Organization not found");
        }

        // Check if user is already an active member
        var existingMember = await _context.Members
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (existingMember != null)
        {
            throw new InvalidOperationException("You are already a member of this organization");
        }

        // Check for duplicate pending request
        var existingRequest = await _context.Requests
            .FirstOrDefaultAsync(r => r.OrgId == orgId && r.SenderId == userId && r.Status == RequestStatus.Pending, ct);

        if (existingRequest != null)
        {
            throw new InvalidOperationException("You already have a pending request for this organization");
        }

        // Parse request type
        if (!Enum.TryParse<RequestType>(request.RequestType, out var requestType))
        {
            throw new ArgumentException($"Invalid request type: {request.RequestType}");
        }

        // Validate desired department if provided
        if (request.DesiredDepartmentId.HasValue)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == request.DesiredDepartmentId.Value && d.OrgId == orgId, ct);

            if (department == null)
            {
                throw new KeyNotFoundException("Desired department not found");
            }
        }

        // Create request
        var newRequest = new Request
        {
            SenderId = userId,
            OrgId = orgId,
            RequestType = requestType,
            Title = request.Title,
            Content = request.Content.Trim(),
            DesiredDepartmentId = request.DesiredDepartmentId,
            DesiredPosition = request.DesiredPosition?.Trim(),
            Status = RequestStatus.Pending
        };

        _context.Requests.Add(newRequest);
        await _context.SaveChangesAsync(ct);

        // Reload with navigation properties
        var createdRequest = await _context.Requests
            .Include(r => r.Sender)
            .Include(r => r.Organization)
            .Include(r => r.DesiredDepartment)
            .FirstAsync(r => r.Id == newRequest.Id, ct);

        return createdRequest.ToRequestDto();
    }

    public async Task<RequestDto> GetRequestByIdAsync(Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var request = await _context.Requests
            .Include(r => r.Sender)
            .Include(r => r.Organization)
            .Include(r => r.DesiredDepartment)
            .Include(r => r.ReviewedByMember)
                .ThenInclude(m => m!.User)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);

        if (request == null)
        {
            throw new KeyNotFoundException("Request not found");
        }

        // Verify user has permission to view this request
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == request.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this request");
        }

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.requests.view");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to view requests");
        }

        return request.ToRequestDto();
    }

    public async Task<RequestDto> ReviewRequestAsync(Guid userId, Guid requestId, ReviewRequestRequest reviewRequest, CancellationToken ct = default)
    {
        var request = await _context.Requests
            .Include(r => r.Sender)
            .Include(r => r.Organization)
            .Include(r => r.DesiredDepartment)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);

        if (request == null)
        {
            throw new KeyNotFoundException("Request not found");
        }

        // Verify user has permission to review requests
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == request.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasReviewPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.requests.review");
        
        var hasApprovePermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.requests.approve");

        if (!hasReviewPermission && !hasApprovePermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to review requests");
        }

        // Verify request is pending
        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot review request with status: {request.Status}");
        }

        // Parse decision
        if (reviewRequest.Decision != "Approved" && reviewRequest.Decision != "Rejected")
        {
            throw new ArgumentException("Decision must be 'Approved' or 'Rejected'");
        }

        var newStatus = reviewRequest.Decision == "Approved" ? RequestStatus.Approved : RequestStatus.Rejected;

        // Update request
        request.Status = newStatus;
        request.ReviewNote = reviewRequest.ReviewNote?.Trim();
        request.ReviewedByMemberId = member.Id;
        request.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        // If approved and it's a join request, create member (simple implementation)
        // Note: This is a basic implementation. Production code might need more complex logic.
        if (newStatus == RequestStatus.Approved && request.RequestType == RequestType.JoinOrganization)
        {
            // Check if user is not already a member
            var existingMember = await _context.Members
                .FirstOrDefaultAsync(m => m.OrgId == request.OrgId && m.UserId == request.SenderId, ct);

            if (existingMember == null)
            {
                // Get default member role
                var defaultRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.OrgId == request.OrgId && r.IsDefault && r.RoleName == "Member", ct);

                if (defaultRole != null)
                {
                    var newMember = new Member
                    {
                        UserId = request.SenderId,
                        OrgId = request.OrgId,
                        RoleId = defaultRole.Id,
                        DepartmentId = request.DesiredDepartmentId,
                        StudentCode = null,
                        Status = MemberStatus.Active,
                        JoinDate = DateTime.UtcNow
                    };

                    _context.Members.Add(newMember);
                    
                    // Update organization total members
                    var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == request.OrgId, ct);
                    if (org != null)
                    {
                        org.TotalMembers++;
                    }

                    await _context.SaveChangesAsync(ct);
                }
            }
        }

        // Reload with navigation properties
        var updatedRequest = await _context.Requests
            .Include(r => r.Sender)
            .Include(r => r.Organization)
            .Include(r => r.DesiredDepartment)
            .Include(r => r.ReviewedByMember)
                .ThenInclude(m => m!.User)
            .FirstAsync(r => r.Id == requestId, ct);

        return updatedRequest.ToRequestDto();
    }
}
