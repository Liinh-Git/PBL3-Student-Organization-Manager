using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Milestones.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones.Services;

public class MilestoneService : IMilestoneService
{
    private readonly AppDbContext _context;

    public MilestoneService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MilestoneDto>> GetEventMilestonesAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        // Get event and verify membership
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        await VerifyMembershipAsync(evt.OrgId, userId, ct);

        // Get milestones for this event
        var milestones = await _context.Milestones
            .Where(m => m.EventId == eventId)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(ct);

        return milestones.Select(m => m.ToMilestoneDto()).ToList();
    }

    public async Task<MilestoneDto> GetMilestoneByIdAsync(Guid milestoneId, Guid userId, CancellationToken ct = default)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Event)
            .FirstOrDefaultAsync(m => m.Id == milestoneId, ct);

        if (milestone == null)
        {
            throw new KeyNotFoundException("Milestone not found");
        }

        await VerifyMembershipAsync(milestone.Event.OrgId, userId, ct);

        return milestone.ToMilestoneDto();
    }

    public async Task<MilestoneDto> CreateMilestoneAsync(Guid eventId, CreateMilestoneRequest request, Guid userId, CancellationToken ct = default)
    {
        // Get event and verify membership + permission
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (evt == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        await VerifyMembershipAsync(evt.OrgId, userId, ct);
        await EnsureEventManagerAsync(eventId, userId, ct);

        // Auto-increment OrderIndex if not provided
        var orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(eventId, ct);

        // Convert to UTC if provided
        var utcStartDate = request.StartDate.HasValue ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc) : (DateTime?)null;
        var utcEndDate = request.EndDate.HasValue ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null;

        var milestone = new Milestone
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Title = request.Title,
            Description = request.Description,
            StartDate = utcStartDate,
            EndDate = utcEndDate,
            OrderIndex = orderIndex,
            Status = MilestoneStatus.Planned,
            CreatedAt = DateTime.UtcNow
        };

        _context.Milestones.Add(milestone);
        await _context.SaveChangesAsync(ct);

        return milestone.ToMilestoneDto();
    }

    public async Task<MilestoneDto> UpdateMilestoneAsync(Guid milestoneId, UpdateMilestoneRequest request, Guid userId, CancellationToken ct = default)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Event)
            .FirstOrDefaultAsync(m => m.Id == milestoneId, ct);

        if (milestone == null)
        {
            throw new KeyNotFoundException("Milestone not found");
        }

        await VerifyMembershipAsync(milestone.Event.OrgId, userId, ct);
        await EnsureEventManagerAsync(milestone.EventId, userId, ct);

        // Parse status
        if (!Enum.TryParse<MilestoneStatus>(request.Status, out var status))
        {
            throw new ArgumentException($"Invalid status: {request.Status}");
        }

        // Convert to UTC if provided
        var utcStartDate = request.StartDate.HasValue ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc) : (DateTime?)null;
        var utcEndDate = request.EndDate.HasValue ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null;

        milestone.Title = request.Title;
        milestone.Description = request.Description;
        milestone.StartDate = utcStartDate;
        milestone.EndDate = utcEndDate;
        milestone.Status = status;
        milestone.OrderIndex = request.OrderIndex ?? milestone.OrderIndex;
        milestone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return milestone.ToMilestoneDto();
    }

    public async Task DeleteMilestoneAsync(Guid milestoneId, Guid userId, CancellationToken ct = default)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Event)
            .Include(m => m.Categories)
            .FirstOrDefaultAsync(m => m.Id == milestoneId, ct);

        if (milestone == null)
        {
            throw new KeyNotFoundException("Milestone not found");
        }

        await VerifyMembershipAsync(milestone.Event.OrgId, userId, ct);
        await EnsureEventManagerAsync(milestone.EventId, userId, ct);

        // Check if milestone has categories
        if (milestone.Categories.Any(c => !c.IsDeleted))
        {
            throw new InvalidOperationException("Cannot delete milestone with existing categories");
        }

        milestone.IsDeleted = true;
        milestone.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    private async Task<int> GetNextOrderIndexAsync(Guid eventId, CancellationToken ct)
    {
        var maxOrder = await _context.Milestones
            .Where(m => m.EventId == eventId)
            .MaxAsync(m => (int?)m.OrderIndex, ct);

        return (maxOrder ?? -1) + 1;
    }

    private async Task VerifyMembershipAsync(Guid orgId, Guid userId, CancellationToken ct)
    {
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }
    }

    private async Task EnsureEventManagerAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        var isEventMember = await _context.EventMembers
            .AnyAsync(
                em => em.EventId == eventId &&
                      em.Member.UserId == userId &&
                      em.Member.Status == MemberStatus.Active,
                ct);

        if (!isEventMember)
        {
            throw new UnauthorizedAccessException("Only event organizers can manage milestones");
        }
    }
}
