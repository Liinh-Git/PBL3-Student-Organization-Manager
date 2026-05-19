using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.EventMembers;

namespace Org.Backend.Features.EventMembers.Services;

public class EventMemberService : IEventMemberService
{
    private readonly AppDbContext _context;

    public EventMemberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventMemberDto>> GetEventMembersAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        await EnsureOrgMemberAsync(evt.OrgId, userId, ct);

        return await _context.EventMembers
            .Include(em => em.Member).ThenInclude(m => m.User)
            .Where(em => em.EventId == eventId && em.Member.Status == MemberStatus.Active)
            .OrderBy(em => em.AssignedAt)
            .Select(em => new EventMemberDto
            {
                Id = em.Id,
                EventId = em.EventId,
                MemberId = em.MemberId,
                FullName = em.Member.User.FullName,
                Email = em.Member.User.Email,
                AssignedAtUtc = em.AssignedAt
            })
            .ToListAsync(ct);
    }

    public async Task<List<EventMemberDto>> AddEventMembersAsync(Guid eventId, Guid userId, AddEventMembersRequest request, CancellationToken ct = default)
    {
        if (request.MemberIds == null || request.MemberIds.Count == 0)
        {
            throw new InvalidOperationException("MemberIds is required");
        }

        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        await EnsureUserCanManageEventAsync(evt, userId, ct);

        var distinctMemberIds = request.MemberIds.Distinct().ToList();
        var orgMembers = await _context.Members
            .Where(m => distinctMemberIds.Contains(m.Id) && m.OrgId == evt.OrgId && m.Status == MemberStatus.Active)
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (orgMembers.Count != distinctMemberIds.Count)
        {
            throw new InvalidOperationException("Some members are invalid or not active in this organization");
        }

        var existingMemberIds = await _context.EventMembers
            .Where(em => em.EventId == eventId)
            .Select(em => em.MemberId)
            .ToListAsync(ct);

        var toAdd = orgMembers.Except(existingMemberIds).ToList();
        if (toAdd.Count > 0)
        {
            var now = DateTime.UtcNow;
            var entities = toAdd.Select(memberId => new EventMember
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                MemberId = memberId,
                EventRole = EventRole.Manager,
                AssignedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            });
            _context.EventMembers.AddRange(entities);
            await _context.SaveChangesAsync(ct);
        }

        return await GetEventMembersAsync(eventId, userId, ct);
    }

    public async Task<bool> RemoveEventMemberAsync(Guid eventMemberId, Guid userId, CancellationToken ct = default)
    {
        var eventMember = await _context.EventMembers
            .Include(em => em.Event)
            .Include(em => em.Member)
            .FirstOrDefaultAsync(em => em.Id == eventMemberId, ct);

        if (eventMember == null)
        {
            throw new KeyNotFoundException("Event member not found");
        }

        await EnsureUserCanManageEventAsync(eventMember.Event, userId, ct);

        var activeCount = await _context.EventMembers
            .CountAsync(em => em.EventId == eventMember.EventId && em.Member.Status == MemberStatus.Active, ct);

        if (activeCount <= 1)
        {
            throw new InvalidOperationException("Event must have at least one organizer");
        }

        var now = DateTime.UtcNow;
        var assignedTasks = await _context.OrgTasks
            .Where(t =>
                t.AssigneeId == eventMember.MemberId &&
                t.EventCategory.Milestone.EventId == eventMember.EventId)
            .ToListAsync(ct);

        foreach (var task in assignedTasks)
        {
            task.AssigneeId = null;
            task.UpdatedAt = now;
        }

        _context.EventMembers.Remove(eventMember);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task EnsureOrgMemberAsync(Guid orgId, Guid userId, CancellationToken ct)
    {
        var isOrgMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);
        if (!isOrgMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }
    }

    private async Task EnsureUserCanManageEventAsync(Event evt, Guid userId, CancellationToken ct)
    {
        await EnsureOrgMemberAsync(evt.OrgId, userId, ct);

        var isEventMember = await _context.EventMembers
            .AnyAsync(
                em => em.EventId == evt.Id &&
                      em.Member.UserId == userId &&
                      em.Member.Status == MemberStatus.Active,
                ct);

        if (!isEventMember)
        {
            throw new UnauthorizedAccessException("Only event organizers can manage this event");
        }
    }
}
