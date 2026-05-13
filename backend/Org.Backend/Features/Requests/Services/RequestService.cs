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

        var roleName = (member.Role.RoleName ?? string.Empty).Trim().ToLowerInvariant();
        var isLeadership = roleName == "president" || roleName == "vice president" || roleName == "vicepresident";

        var hasPermission = member.Role.RolePermissions
            .Any(rp =>
                rp.Permission?.PermissionKey == "org.requests.view" ||
                rp.Permission?.PermissionKey == "org.requests.review" ||
                rp.Permission?.PermissionKey == "org.requests.approve");

        if (!hasPermission && !isLeadership)
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
        // Parse request type first - business rules depend on type
        var normalizedRequestType = request.RequestType.Trim();
        if (!Enum.TryParse<RequestType>(normalizedRequestType, true, out var requestType))
        {
            throw new ArgumentException($"Invalid request type: {request.RequestType}");
        }

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

        // JoinOrganization is only for non-members; other request types are for existing members.
        if (requestType == RequestType.JoinOrganization && existingMember != null)
        {
            throw new InvalidOperationException("You are already a member of this organization");
        }
        if (requestType != RequestType.JoinOrganization && existingMember == null)
        {
            throw new InvalidOperationException("You must be an active member to submit this request type");
        }

        // Check for duplicate pending request by type
        var existingRequest = await _context.Requests
            .FirstOrDefaultAsync(r =>
                r.OrgId == orgId &&
                r.SenderId == userId &&
                r.Status == RequestStatus.Pending &&
                r.RequestType == requestType, ct);

        if (existingRequest != null)
        {
            throw new InvalidOperationException($"You already have a pending {requestType} request for this organization");
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

        if (requestType == RequestType.JoinOrganization)
        {
            await NotifyRequestReviewersAsync(newRequest, organization.OrgName, ct);
        }

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

        // Sender can always view their own request (including non-member join requests)
        if (request.SenderId == userId)
        {
            return request.ToRequestDto();
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
            .Any(rp =>
                rp.Permission?.PermissionKey == "org.requests.view" ||
                rp.Permission?.PermissionKey == "org.requests.review" ||
                rp.Permission?.PermissionKey == "org.requests.approve");

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
        await NotifyRequestSenderReviewedAsync(request, member.UserId, ct);

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

    public async Task<List<MyPendingJoinRequestDto>> GetMyPendingJoinRequestsAsync(Guid userId, CancellationToken ct = default)
    {
        var pending = await _context.Requests
            .Where(r => r.SenderId == userId && r.RequestType == RequestType.JoinOrganization && r.Status == RequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MyPendingJoinRequestDto
            {
                RequestId = r.Id,
                OrganizationId = r.OrgId,
                CreatedAtUtc = r.CreatedAt
            })
            .ToListAsync(ct);

        return pending;
    }

    public async Task<bool> WithdrawMyPendingJoinRequestAsync(Guid userId, Guid orgId, CancellationToken ct = default)
    {
        var request = await _context.Requests
            .FirstOrDefaultAsync(r =>
                r.SenderId == userId &&
                r.OrgId == orgId &&
                r.RequestType == RequestType.JoinOrganization &&
                r.Status == RequestStatus.Pending, ct);

        if (request == null)
        {
            return false;
        }

        request.Status = RequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task NotifyRequestReviewersAsync(Request request, string organizationName, CancellationToken ct)
    {
        var reviewerUserIds = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(m => m.OrgId == request.OrgId && m.Status == MemberStatus.Active)
            .Where(m => m.Role != null && m.Role.RolePermissions.Any(rp =>
                rp.Permission != null &&
                (rp.Permission.PermissionKey == "org.requests.review" || rp.Permission.PermissionKey == "org.requests.approve")))
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync(ct);

        if (reviewerUserIds.Count == 0)
        {
            return;
        }

        var senderName = await _context.Users
            .Where(u => u.Id == request.SenderId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct) ?? "A member";

        var notifications = reviewerUserIds.Select(receiverId => new Notification
        {
            ReceiverId = receiverId,
            ActorId = request.SenderId,
            Title = "New join request",
            Message = $"{senderName} submitted a join request to {organizationName}.",
            Type = NotificationType.RequestSubmitted,
            RelatedEntityType = nameof(Request),
            RelatedEntityId = request.Id,
            ActionUrl = $"/org/requests?orgId={request.OrgId}",
            IsRead = false
        });

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);
    }

    private async Task NotifyRequestSenderReviewedAsync(Request request, Guid reviewerUserId, CancellationToken ct)
    {
        var organizationName = await _context.Organizations
            .Where(o => o.Id == request.OrgId)
            .Select(o => o.OrgName)
            .FirstOrDefaultAsync(ct) ?? "organization";

        var reviewerName = await _context.Users
            .Where(u => u.Id == reviewerUserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct) ?? "Reviewer";

        var decisionText = request.Status == RequestStatus.Approved ? "approved" : "rejected";
        var detail = string.IsNullOrWhiteSpace(request.ReviewNote) ? string.Empty : $" Note: {request.ReviewNote}";

        var notification = new Notification
        {
            ReceiverId = request.SenderId,
            ActorId = reviewerUserId,
            Title = $"Request {decisionText}",
            Message = $"Your request to {organizationName} was {decisionText} by {reviewerName}.{detail}",
            Type = NotificationType.RequestReviewed,
            RelatedEntityType = nameof(Request),
            RelatedEntityId = request.Id,
            ActionUrl = $"/org/requests?orgId={request.OrgId}",
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);
    }
}
