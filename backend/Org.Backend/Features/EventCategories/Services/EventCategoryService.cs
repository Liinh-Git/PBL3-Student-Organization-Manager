using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.EventCategories.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories.Services;

public class EventCategoryService : IEventCategoryService
{
    private readonly AppDbContext _context;

    public EventCategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventCategoryDto>> GetMilestoneCategoriesAsync(Guid milestoneId, Guid userId, CancellationToken ct = default)
    {
        // Get milestone and verify membership
        var milestone = await _context.Milestones
            .Include(m => m.Event)
            .FirstOrDefaultAsync(m => m.Id == milestoneId, ct);

        if (milestone == null)
        {
            throw new KeyNotFoundException("Milestone not found");
        }

        await VerifyMembershipAsync(milestone.Event.OrgId, userId, ct);

        // Get categories with tasks
        var categories = await _context.EventCategories
            .Include(c => c.OwnerDepartment)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.Assignee)
                    .ThenInclude(a => a!.User)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.Department)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.CreatedByMember)
                    .ThenInclude(cb => cb!.User)
            .Where(c => c.MilestoneId == milestoneId)
            .OrderBy(c => c.OrderIndex)
            .ToListAsync(ct);

        return categories.Select(c => c.ToEventCategoryDto(includeTasks: true)).ToList();
    }

    public async Task<EventCategoryDto> GetCategoryByIdAsync(Guid categoryId, Guid userId, CancellationToken ct = default)
    {
        var category = await _context.EventCategories
            .Include(c => c.Milestone)
                .ThenInclude(m => m.Event)
            .Include(c => c.OwnerDepartment)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.Assignee)
                    .ThenInclude(a => a!.User)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.Department)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.CreatedByMember)
                    .ThenInclude(cb => cb!.User)
            .FirstOrDefaultAsync(c => c.Id == categoryId, ct);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        await VerifyMembershipAsync(category.Milestone.Event.OrgId, userId, ct);

        return category.ToEventCategoryDto(includeTasks: true);
    }

    public async Task<EventCategoryDto> CreateCategoryAsync(Guid milestoneId, CreateEventCategoryRequest request, Guid userId, CancellationToken ct = default)
    {
        // Get milestone and verify membership + permission
        var milestone = await _context.Milestones
            .Include(m => m.Event)
            .FirstOrDefaultAsync(m => m.Id == milestoneId, ct);

        if (milestone == null)
        {
            throw new KeyNotFoundException("Milestone not found");
        }

        await VerifyMembershipAsync(milestone.Event.OrgId, userId, ct);
        await EnsureEventManagerAsync(milestone.EventId, userId, ct);

        // Verify department belongs to same organization if provided
        if (request.OwnerDepartmentId.HasValue)
        {
            var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.OwnerDepartmentId.Value, ct);
            if (dept == null)
            {
                throw new KeyNotFoundException("Department not found");
            }
            if (dept.OrgId != milestone.Event.OrgId)
            {
                throw new InvalidOperationException("Department must belong to the same organization");
            }
        }

        // Auto-increment OrderIndex if not provided
        var orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(milestoneId, ct);

        var category = new EventCategory
        {
            Id = Guid.NewGuid(),
            MilestoneId = milestoneId,
            CategoryName = request.CategoryName,
            Description = request.Description,
            OwnerDepartmentId = request.OwnerDepartmentId,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };

        _context.EventCategories.Add(category);
        await _context.SaveChangesAsync(ct);

        // Reload with navigation properties
        category = await _context.EventCategories
            .Include(c => c.OwnerDepartment)
            .FirstAsync(c => c.Id == category.Id, ct);

        return category.ToEventCategoryDto(includeTasks: true);
    }

    public async Task<EventCategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateEventCategoryRequest request, Guid userId, CancellationToken ct = default)
    {
        var category = await _context.EventCategories
            .Include(c => c.Milestone)
                .ThenInclude(m => m.Event)
            .Include(c => c.OwnerDepartment)
            .FirstOrDefaultAsync(c => c.Id == categoryId, ct);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        await VerifyMembershipAsync(category.Milestone.Event.OrgId, userId, ct);
        await EnsureEventManagerAsync(category.Milestone.EventId, userId, ct);

        // Verify department belongs to same organization if provided
        if (request.OwnerDepartmentId.HasValue)
        {
            var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.OwnerDepartmentId.Value, ct);
            if (dept == null)
            {
                throw new KeyNotFoundException("Department not found");
            }
            if (dept.OrgId != category.Milestone.Event.OrgId)
            {
                throw new InvalidOperationException("Department must belong to the same organization");
            }
        }

        category.CategoryName = request.CategoryName;
        category.Description = request.Description;
        category.OwnerDepartmentId = request.OwnerDepartmentId;
        category.OrderIndex = request.OrderIndex ?? category.OrderIndex;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        // Reload with navigation properties
        category = await _context.EventCategories
            .Include(c => c.OwnerDepartment)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.Assignee)
                    .ThenInclude(a => a!.User)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.Department)
            .Include(c => c.Tasks.Where(t => !t.IsDeleted && t.DeptId == null))
                .ThenInclude(t => t.CreatedByMember)
                    .ThenInclude(cb => cb!.User)
            .FirstAsync(c => c.Id == categoryId, ct);

        return category.ToEventCategoryDto(includeTasks: true);
    }

    public async Task DeleteCategoryAsync(Guid categoryId, Guid userId, CancellationToken ct = default)
    {
        var category = await _context.EventCategories
            .Include(c => c.Milestone)
                .ThenInclude(m => m.Event)
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == categoryId, ct);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        await VerifyMembershipAsync(category.Milestone.Event.OrgId, userId, ct);
        await EnsureEventManagerAsync(category.Milestone.EventId, userId, ct);

        var now = DateTime.UtcNow;
        foreach (var task in category.Tasks)
        {
            task.IsDeleted = true;
            task.DeletedAt = now;
            task.UpdatedAt = now;
        }

        category.IsDeleted = true;
        category.DeletedAt = now;
        category.UpdatedAt = now;
        await _context.SaveChangesAsync(ct);
    }

    private async Task<int> GetNextOrderIndexAsync(Guid milestoneId, CancellationToken ct)
    {
        var maxOrder = await _context.EventCategories
            .Where(c => c.MilestoneId == milestoneId)
            .MaxAsync(c => (int?)c.OrderIndex, ct);

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
            throw new UnauthorizedAccessException("Only event organizers can manage event categories");
        }
    }
}
