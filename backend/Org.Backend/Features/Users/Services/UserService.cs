using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Users.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Users;
using DomainTaskStatus = Org.Backend.Domain.Enums.TaskStatus;

namespace Org.Backend.Features.Users.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserProfileDto> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return user.ToUserProfileDto();
    }

    public async Task<List<MyOrganizationDto>> GetMyOrganizationsAsync(Guid userId, CancellationToken ct = default)
    {
        var members = await _context.Members
            .Include(m => m.Organization)
            .Include(m => m.Role)
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .ToListAsync(ct);

        var orgIds = members.Select(m => m.OrgId).ToList();
        var activeMemberCounts = await _context.Members
            .Where(m => orgIds.Contains(m.OrgId) && m.Status == MemberStatus.Active)
            .GroupBy(m => m.OrgId)
            .Select(g => new { OrgId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrgId, x => x.Count, ct);

        foreach (var member in members)
        {
            member.Organization.TotalMembers = activeMemberCounts.GetValueOrDefault(member.OrgId, 0);
        }

        return members.Select(m => m.ToMyOrganizationDto()).ToList();
    }

    public async Task<List<MyEventDto>> GetMyEventsAsync(Guid userId, CancellationToken ct = default)
    {
        // Events from organizations where user is a member (role-aware),
        // plus outside-org events where user registered as attendee.
        var memberOrgIds = await _context.Members
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .Select(m => m.OrgId)
            .ToListAsync(ct);

        var organizerEventIds = await _context.EventMembers
            .Where(em => em.Member.UserId == userId && em.Member.Status == MemberStatus.Active)
            .Select(em => em.EventId)
            .Distinct()
            .ToListAsync(ct);

        var memberEvents = await _context.Events
            .Include(e => e.Organization)
            .Where(e => memberOrgIds.Contains(e.OrgId))
            .OrderBy(e => e.StartDate)
            .ToListAsync(ct);

        var attendeeEvents = await _context.Attendees
            .Include(a => a.Event)
                .ThenInclude(e => e.Organization)
            .Where(a =>
                a.UserId == userId &&
                a.Status != AttendeeStatus.Cancelled &&
                !memberOrgIds.Contains(a.Event.OrgId))
            .OrderBy(a => a.Event.StartDate)
            .ToListAsync(ct);

        var results = memberEvents
            .Select(e => e.ToMyEventDto(
                participationRole: "OrganizationMember",
                attendanceStatus: null,
                eventRelation: organizerEventIds.Contains(e.Id) ? "EventMember" : "OrgViewer"))
            .ToList();

        results.AddRange(
            attendeeEvents.Select(a =>
                a.Event.ToMyEventDto(
                    participationRole: "Attendee",
                    attendanceStatus: a.Status.ToString(),
                    eventRelation: "Attendee")));

        return results
            .GroupBy(e => e.Id)
            .Select(g => g.First())
            .OrderBy(e => e.StartDate)
            .ToList();
    }

    public async Task<List<MyTaskDto>> GetMyTasksAsync(
        Guid userId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken ct = default)
    {
        var from = fromUtc.HasValue
            ? DateTime.SpecifyKind(fromUtc.Value, DateTimeKind.Utc)
            : (DateTime?)null;
        var to = toUtc.HasValue
            ? DateTime.SpecifyKind(toUtc.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var query = _context.OrgTasks
            .Include(t => t.EventCategory)
                .ThenInclude(c => c.Milestone)
                    .ThenInclude(m => m.Event)
                        .ThenInclude(e => e.Organization)
            .Include(t => t.Department)
            .Include(t => t.Assignee)
                .ThenInclude(a => a!.User)
            .Where(t =>
                !t.IsDeleted &&
                !t.EventCategory.IsDeleted &&
                !t.EventCategory.Milestone.IsDeleted &&
                !t.EventCategory.Milestone.Event.IsDeleted &&
                t.AssigneeId.HasValue &&
                t.Assignee != null &&
                t.Assignee.UserId == userId &&
                t.Assignee.Status == MemberStatus.Active);

        if (from.HasValue)
        {
            query = query.Where(t => t.Deadline.HasValue && t.Deadline.Value >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.Deadline.HasValue && t.Deadline.Value <= to.Value);
        }

        var tasks = await query
            .OrderBy(t => t.Deadline == null)
            .ThenBy(t => t.Deadline)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var nowUtc = DateTime.UtcNow;

        return tasks.Select(t => new MyTaskDto
        {
            Id = t.Id,
            TaskName = t.TaskName,
            Description = t.Description,
            Priority = t.Priority.ToString(),
            Status = t.Status.ToString(),
            Deadline = t.Deadline,
            CompletedAt = t.CompletedAt,
            IsOverdue = t.Deadline.HasValue && t.Deadline.Value < nowUtc && t.Status != DomainTaskStatus.Done,
            OrganizationId = t.EventCategory.Milestone.Event.OrgId,
            OrganizationName = t.EventCategory.Milestone.Event.Organization.OrgName,
            EventId = t.EventCategory.Milestone.EventId,
            EventName = t.EventCategory.Milestone.Event.EventName,
            MilestoneId = t.EventCategory.MilestoneId,
            MilestoneTitle = t.EventCategory.Milestone.Title,
            CategoryId = t.EventCategoryId,
            CategoryName = t.EventCategory.CategoryName,
            DepartmentId = t.DeptId,
            DepartmentName = t.Department != null ? t.Department.DeptName : null,
            TaskSource = t.DeptId.HasValue ? "Department" : "Event"
        }).ToList();
    }

    public async Task<List<DiscoverOrganizationDto>> DiscoverOrganizationsAsync(Guid userId, CancellationToken ct = default)
    {
        // Get organizations where user is NOT currently an active member.
        // Users who previously left (inactive membership) should still discover again.
        var memberOrgIds = await _context.Members
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .Select(m => m.OrgId)
            .ToListAsync(ct);

        var organizations = await _context.Organizations
            .Where(o => o.Status == OrgStatus.Active && !memberOrgIds.Contains(o.Id))
            .OrderByDescending(o => o.TotalMembers)
            .ToListAsync(ct);

        return organizations.Select(o => o.ToDiscoverOrganizationDto()).ToList();
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Update user profile fields
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.Dob = request.Dob;
        user.Gender = request.Gender?.Trim();
        user.Address = request.Address?.Trim();
        user.AvatarUrl = request.AvatarUrl?.Trim();
        user.Bio = request.Bio?.Trim();
        user.SocialLinks = request.SocialLinks?.Trim();
        
        // Parse and set profile visibility
        if (!string.IsNullOrWhiteSpace(request.ProfileVisibility))
        {
            if (Enum.TryParse<ProfileVisibility>(request.ProfileVisibility, out var visibility))
            {
                user.ProfileVisibility = visibility;
            }
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return user.ToUserProfileDto();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Verify current password
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        // Hash new password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }
}
