using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Infrastructure.Persistence;
using Org.Backend.Infrastructure.Persistence.Seed;
using Org.Shared.Features.Invitations;

namespace Org.Backend.Features.Invitations.Services;

public class InvitationService : IInvitationService
{
    private readonly AppDbContext _context;

    public InvitationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InvitationDto> CreateInvitationAsync(Guid orgId, Guid inviterUserId, CreateInvitationRequest request, CancellationToken ct = default)
    {
        var inviterMember = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == inviterUserId && m.Status == MemberStatus.Active, ct);

        if (inviterMember == null)
        {
            throw new UnauthorizedAccessException("You are not an active member of this organization");
        }

        if (inviterMember.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var canManageMembers = inviterMember.Role.RolePermissions.Any(rp => rp.Permission?.PermissionKey == "org.members.manage");
        if (!canManageMembers)
        {
            throw new UnauthorizedAccessException("You do not have permission to invite members");
        }

        return await CreateInvitationCoreAsync(orgId, inviterMember.Id, inviterUserId, request.ReceiverUserId, request.Message, ct);
    }

    public async Task<Request> CreateRecommendationAsync(Guid orgId, Guid recommenderUserId, CreateInvitationRecommendationRequest request, CancellationToken ct = default)
    {
        var recommenderMember = await _context.Members
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == recommenderUserId && m.Status == MemberStatus.Active, ct);
        if (recommenderMember == null)
        {
            throw new UnauthorizedAccessException("You are not an active member of this organization");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ReceiverUserId, ct);
        if (targetUser == null)
        {
            throw new KeyNotFoundException("Receiver user not found");
        }

        var orgExists = await _context.Organizations.AnyAsync(o => o.Id == orgId, ct);
        if (!orgExists)
        {
            throw new KeyNotFoundException("Organization not found");
        }

        var isAlreadyMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == request.ReceiverUserId && m.Status == MemberStatus.Active, ct);
        if (isAlreadyMember)
        {
            throw new InvalidOperationException("Target user is already an active member of this organization");
        }

        var hasPendingRecommendation = await _context.Requests.AnyAsync(r =>
            r.OrgId == orgId &&
            r.SenderId == recommenderUserId &&
            r.RequestType == RequestType.Other &&
            r.Title == InvitationMarkers.RecommendationTitle &&
            r.DesiredPosition == request.ReceiverUserId.ToString() &&
            r.Status == RequestStatus.Pending, ct);
        if (hasPendingRecommendation)
        {
            throw new InvalidOperationException("You already have a pending recommendation for this user");
        }

        var recommendation = new Request
        {
            SenderId = recommenderUserId,
            OrgId = orgId,
            RequestType = RequestType.Other,
            Title = InvitationMarkers.RecommendationTitle,
            Content = string.IsNullOrWhiteSpace(request.Message)
                ? $"Recommend inviting user {request.ReceiverUserId} to this organization."
                : request.Message.Trim(),
            DesiredPosition = request.ReceiverUserId.ToString(),
            Status = RequestStatus.Pending
        };

        _context.Requests.Add(recommendation);
        var organizationName = await _context.Organizations
            .Where(o => o.Id == orgId)
            .Select(o => o.OrgName)
            .FirstOrDefaultAsync(ct) ?? "organization";

        var recommenderName = await _context.Users
            .Where(u => u.Id == recommenderUserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct) ?? "A member";

        var reviewerUserIds = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(m => m.OrgId == orgId && m.Status == MemberStatus.Active)
            .Where(m => m.Role != null && m.Role.RolePermissions.Any(rp =>
                rp.Permission != null &&
                (rp.Permission.PermissionKey == "org.requests.review" || rp.Permission.PermissionKey == "org.requests.approve")))
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync(ct);

        if (reviewerUserIds.Count > 0)
        {
            var notify = reviewerUserIds.Select(receiverId => new Notification
            {
                ReceiverId = receiverId,
                ActorId = recommenderUserId,
                Title = "New member recommendation",
                Message = $"{recommenderName} recommended a friend to join {organizationName}.",
                Type = NotificationType.RequestSubmitted,
                RelatedEntityType = nameof(Request),
                RelatedEntityId = recommendation.Id,
                ActionUrl = $"/org/requests?orgId={orgId}",
                IsRead = false
            });
            _context.Notifications.AddRange(notify);
        }
        await _context.SaveChangesAsync(ct);

        return recommendation;
    }

    public async Task<InvitationDto> CreateInvitationByMemberIdAsync(Guid orgId, Guid inviterMemberId, Guid receiverUserId, string? message, CancellationToken ct = default)
    {
        var inviterMember = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == inviterMemberId && m.OrgId == orgId && m.Status == MemberStatus.Active, ct);
        if (inviterMember == null) throw new KeyNotFoundException("Inviter member not found");
        return await CreateInvitationCoreAsync(orgId, inviterMember.Id, inviterMember.UserId, receiverUserId, message, ct);
    }

    private async Task<InvitationDto> CreateInvitationCoreAsync(Guid orgId, Guid inviterMemberId, Guid inviterUserId, Guid receiverUserId, string? message, CancellationToken ct)
    {
        var inviterMember = await _context.Members
            .Include(m => m.User)
            .FirstAsync(m => m.Id == inviterMemberId, ct);

        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == receiverUserId, ct);
        if (receiver == null)
        {
            throw new KeyNotFoundException("Receiver user not found");
        }

        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == orgId, ct);
        if (organization == null)
        {
            throw new KeyNotFoundException("Organization not found");
        }

        var isAlreadyMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == receiverUserId && m.Status == MemberStatus.Active, ct);
        if (isAlreadyMember)
        {
            throw new InvalidOperationException("User is already an active member of this organization");
        }

        var hasPendingInvite = await _context.Requests.AnyAsync(r =>
            r.OrgId == orgId &&
            r.SenderId == receiverUserId &&
            r.RequestType == RequestType.Other &&
            r.Title == InvitationMarkers.InvitationTitle &&
            r.Status == RequestStatus.Pending, ct);
        if (hasPendingInvite)
        {
            throw new InvalidOperationException("This user already has a pending invitation for this organization");
        }

        var inviteRequest = new Request
        {
            SenderId = receiverUserId,
            OrgId = orgId,
            RequestType = RequestType.Other,
            Title = InvitationMarkers.InvitationTitle,
            Content = string.IsNullOrWhiteSpace(message)
                ? $"{inviterMember.User.FullName} invited you to join {organization.OrgName}."
                : message.Trim(),
            Status = RequestStatus.Pending,
            ReviewedByMemberId = inviterMemberId,
            ReviewNote = message?.Trim()
        };

        _context.Requests.Add(inviteRequest);
        _context.Notifications.Add(new Notification
        {
            ReceiverId = receiverUserId,
            ActorId = inviterUserId,
            Title = "Organization invitation",
            Message = $"{inviterMember.User.FullName} invited you to join {organization.OrgName}.",
            Type = NotificationType.System,
            RelatedEntityType = nameof(Request),
            RelatedEntityId = inviteRequest.Id,
            ActionUrl = "/user/discover",
            IsRead = false
        });

        await _context.SaveChangesAsync(ct);
        return await ToInvitationDtoAsync(inviteRequest.Id, ct);
    }

    public async Task<List<InvitationDto>> GetMyInvitationsAsync(Guid userId, CancellationToken ct = default)
    {
        var invites = await _context.Requests
            .Where(r => r.SenderId == userId && r.RequestType == RequestType.Other && r.Title == InvitationMarkers.InvitationTitle)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => r.Id)
            .ToListAsync(ct);

        var result = new List<InvitationDto>(invites.Count);
        foreach (var id in invites)
        {
            result.Add(await ToInvitationDtoAsync(id, ct));
        }
        return result;
    }

    public async Task<InvitationDto> AcceptInvitationAsync(Guid userId, Guid invitationId, CancellationToken ct = default)
    {
        var invite = await LoadInvitationAsync(userId, invitationId, ct);
        if (invite.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot accept invitation with status: {invite.Status}");
        }

        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var existingMember = await _context.Members
                .FirstOrDefaultAsync(m => m.OrgId == invite.OrgId && m.UserId == userId, ct);

            var memberRole = await ResolveMemberRoleAsync(invite.OrgId, ct);

            if (existingMember == null)
            {
                _context.Members.Add(new Member
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrgId = invite.OrgId,
                    RoleId = memberRole.Id,
                    Status = MemberStatus.Active,
                    JoinDate = DateTime.UtcNow
                });

                var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == invite.OrgId, ct);
                if (org != null) org.TotalMembers++;
            }
            else if (existingMember.Status != MemberStatus.Active)
            {
                existingMember.Status = MemberStatus.Active;
                existingMember.RoleId = memberRole.Id;
                existingMember.JoinDate = DateTime.UtcNow;
                existingMember.UpdatedAt = DateTime.UtcNow;

                var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == invite.OrgId, ct);
                if (org != null) org.TotalMembers++;
            }

            invite.Status = RequestStatus.Approved;
            invite.ReviewedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            var hasActiveMembership = await _context.Members
                .AnyAsync(m => m.OrgId == invite.OrgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);
            if (!hasActiveMembership)
            {
                throw new InvalidOperationException("Invitation accepted but active membership was not created");
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        await NotifyInviterResultAsync(invite, true, ct);
        return await ToInvitationDtoAsync(invitationId, ct);
    }

    public async Task<InvitationDto> RejectInvitationAsync(Guid userId, Guid invitationId, CancellationToken ct = default)
    {
        var invite = await LoadInvitationAsync(userId, invitationId, ct);
        if (invite.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot reject invitation with status: {invite.Status}");
        }

        invite.Status = RequestStatus.Rejected;
        invite.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        await NotifyInviterResultAsync(invite, false, ct);
        return await ToInvitationDtoAsync(invitationId, ct);
    }

    private async Task<Request> LoadInvitationAsync(Guid userId, Guid invitationId, CancellationToken ct)
    {
        var invite = await _context.Requests.FirstOrDefaultAsync(r =>
            r.Id == invitationId &&
            r.SenderId == userId &&
            r.RequestType == RequestType.Other &&
            r.Title == InvitationMarkers.InvitationTitle, ct);

        if (invite == null)
        {
            throw new KeyNotFoundException("Invitation not found");
        }
        return invite;
    }

    private async Task<Role> ResolveMemberRoleAsync(Guid orgId, CancellationToken ct)
    {
        var memberRole = await _context.Roles
            .Where(r => r.OrgId == orgId && r.RoleName == SeedConstants.MemberRoleName)
            .OrderByDescending(r => r.IsDefault)
            .ThenByDescending(r => r.Level ?? int.MinValue)
            .FirstOrDefaultAsync(ct);

        if (memberRole == null)
        {
            memberRole = await _context.Roles
                .Where(r =>
                    r.OrgId == orgId &&
                    r.IsDefault &&
                    r.RoleName != null &&
                    r.RoleName.ToLower() != "president" &&
                    r.RoleName.ToLower() != "vice president" &&
                    r.RoleName.ToLower() != "vicepresident")
                .OrderByDescending(r => r.Level ?? int.MinValue)
                .FirstOrDefaultAsync(ct);
        }

        if (memberRole == null)
        {
            throw new InvalidOperationException("Cannot accept invitation because no valid member role is configured in organization");
        }

        return memberRole;
    }

    private async Task NotifyInviterResultAsync(Request invite, bool accepted, CancellationToken ct)
    {
        if (!invite.ReviewedByMemberId.HasValue)
        {
            return;
        }

        var inviter = await _context.Members
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == invite.ReviewedByMemberId.Value, ct);
        if (inviter == null)
        {
            return;
        }

        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.SenderId, ct);
        var orgName = await _context.Organizations
            .Where(o => o.Id == invite.OrgId)
            .Select(o => o.OrgName)
            .FirstOrDefaultAsync(ct) ?? "organization";

        _context.Notifications.Add(new Notification
        {
            ReceiverId = inviter.UserId,
            ActorId = invite.SenderId,
            Title = accepted ? "Invitation accepted" : "Invitation rejected",
            Message = $"{receiver?.FullName ?? "User"} {(accepted ? "accepted" : "rejected")} your invitation to {orgName}.",
            Type = NotificationType.System,
            RelatedEntityType = nameof(Request),
            RelatedEntityId = invite.Id,
            ActionUrl = $"/org/members?orgId={invite.OrgId}",
            IsRead = false
        });

        await _context.SaveChangesAsync(ct);
    }

    private async Task<InvitationDto> ToInvitationDtoAsync(Guid invitationId, CancellationToken ct)
    {
        var data = await _context.Requests
            .Where(r => r.Id == invitationId)
            .Select(r => new
            {
                r.Id,
                r.OrgId,
                OrgName = r.Organization.OrgName,
                ReceiverUserId = r.SenderId,
                r.Status,
                Message = r.ReviewNote ?? r.Content,
                CreatedAtUtc = r.CreatedAt,
                RespondedAt = r.ReviewedAt,
                InviterMemberId = r.ReviewedByMemberId
            })
            .FirstAsync(ct);

        var inviter = await _context.Members
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => data.InviterMemberId.HasValue && m.Id == data.InviterMemberId.Value, ct);

        return new InvitationDto
        {
            InvitationId = data.Id,
            OrganizationId = data.OrgId,
            OrganizationName = data.OrgName,
            ReceiverUserId = data.ReceiverUserId,
            InviterUserId = inviter?.UserId ?? Guid.Empty,
            InviterName = inviter?.User?.FullName ?? "Unknown",
            Status = data.Status.ToString(),
            Message = data.Message,
            CreatedAtUtc = data.CreatedAtUtc,
            RespondedAt = data.RespondedAt
        };
    }
}
