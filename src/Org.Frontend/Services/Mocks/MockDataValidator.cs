using Org.Frontend.Services.Mocks.Models;

namespace Org.Frontend.Services.Mocks;

public static class MockDataValidator
{
    public static void Validate(MockDataSet data)
    {
        ArgumentNullException.ThrowIfNull(data);

        Ensure(data.Users.Count > 0, "Dataset 'users' must not be empty.");
        Ensure(data.Organizations.Count > 0, "Dataset 'organizations' must not be empty.");
        Ensure(data.Members.Count > 0, "Dataset 'members' must not be empty.");
        Ensure(data.Events.Count > 0, "Dataset 'events' must not be empty.");
        Ensure(data.Attendees.Count > 0, "Dataset 'attendees' must not be empty.");

        EnsureDistinct(data.Users.Select(x => x.Id), "users.id");
        EnsureDistinct(data.Organizations.Select(x => x.Id), "organizations.id");
        EnsureDistinct(data.Departments.Select(x => x.Id), "departments.id");
        EnsureDistinct(data.Members.Select(x => x.Id), "members.id");
        EnsureDistinct(data.Events.Select(x => x.Id), "events.id");
        EnsureDistinct(data.Attendees.Select(x => x.Id), "attendees.id");
        EnsureDistinct(data.Milestones.Select(x => x.Id), "milestones.id");
        EnsureDistinct(data.EventCategories.Select(x => x.Id), "eventCategories.id");
        EnsureDistinct(data.Tasks.Select(x => x.Id), "tasks.id");

        var organizationIds = data.Organizations.Select(x => x.Id).ToHashSet();
        var userIds = data.Users.Select(x => x.Id).ToHashSet();
        var memberIds = data.Members.Select(x => x.Id).ToHashSet();
        var departmentIds = data.Departments.Select(x => x.Id).ToHashSet();
        var eventIds = data.Events.Select(x => x.Id).ToHashSet();
        var milestoneIds = data.Milestones.Select(x => x.Id).ToHashSet();
        var categoryIds = data.EventCategories.Select(x => x.Id).ToHashSet();

        var departmentsById = data.Departments.ToDictionary(x => x.Id);
        var membersById = data.Members.ToDictionary(x => x.Id);
        var eventsById = data.Events.ToDictionary(x => x.Id);
        var milestonesById = data.Milestones.ToDictionary(x => x.Id);
        var categoriesById = data.EventCategories.ToDictionary(x => x.Id);

        Ensure(data.Departments.All(x => organizationIds.Contains(x.OrgId)), "Department.OrgId must exist in organizations.");
        Ensure(data.Members.All(x => organizationIds.Contains(x.OrgId)), "Member.OrgId must exist in organizations.");
        Ensure(data.Members.All(x => userIds.Contains(x.UserId)), "Member.UserId must exist in users.");
        Ensure(data.Members.All(x => x.DepartmentId is null || departmentIds.Contains(x.DepartmentId.Value)), "Member.DepartmentId must exist in departments when present.");
        Ensure(data.Events.All(x => organizationIds.Contains(x.OrgId)), "Event.OrgId must exist in organizations.");
        Ensure(data.EventMembers.All(x => eventIds.Contains(x.EventId)), "EventMember.EventId must exist in events.");
        Ensure(data.EventMembers.All(x => memberIds.Contains(x.MemberId)), "EventMember.MemberId must exist in members.");
        Ensure(data.Attendees.All(x => eventIds.Contains(x.EventId)), "Attendee.EventId must exist in events.");
        Ensure(data.Attendees.All(x => x.UserId is null || userIds.Contains(x.UserId.Value)), "Attendee.UserId must exist in users when present.");
        Ensure(data.Milestones.All(x => eventIds.Contains(x.EventId)), "Milestone.EventId must exist in events.");
        Ensure(data.EventCategories.All(x => milestoneIds.Contains(x.MilestoneId)), "EventCategory.MilestoneId must exist in milestones.");
        Ensure(data.EventCategories.All(x => x.LeadMemberId is null || memberIds.Contains(x.LeadMemberId.Value)), "EventCategory.LeadMemberId must exist in members when present.");
        Ensure(data.Tasks.All(x => categoryIds.Contains(x.CategoryId)), "Task.CategoryId must exist in event categories.");
        Ensure(data.Tasks.All(x => x.AssigneeMemberId is null || memberIds.Contains(x.AssigneeMemberId.Value)), "Task.AssigneeMemberId must exist in members when present.");

        var organizationsWithMembers = data.Members.Select(x => x.OrgId).ToHashSet();
        var organizationsWithEvents = data.Events.Select(x => x.OrgId).ToHashSet();
        var organizationsWithDepartments = data.Departments.Select(x => x.OrgId).ToHashSet();

        Ensure(organizationsWithMembers.Overlaps(organizationsWithEvents),
            "At least one organization must have both members and events.");

        foreach (var organizationId in organizationsWithEvents)
        {
            Ensure(organizationsWithMembers.Contains(organizationId),
                $"Organization {organizationId} has events but no members.");
            Ensure(organizationsWithDepartments.Contains(organizationId),
                $"Organization {organizationId} has events but no departments.");
        }

        // Match backend uniqueness semantics used by FE scope.
        EnsureDistinct(data.Members.Select(x => CompositeKey(x.UserId, x.OrgId)), "members (userId, orgId)");
        EnsureDistinct(data.EventMembers.Select(x => CompositeKey(x.EventId, x.MemberId)), "eventMembers (eventId, memberId)");
        EnsureDistinct(
            data.Attendees
                .Where(x => x.UserId.HasValue)
                .Select(x => CompositeKey(x.EventId, x.UserId!.Value)),
            "attendees (eventId, userId)");
        EnsureDistinct(data.EventCategories.Select(x => CompositeKey(x.MilestoneId, x.Name?.Trim().ToUpperInvariant() ?? string.Empty)), "eventCategories (milestoneId, name)");

        Ensure(data.Events.All(x => x.StartDate <= x.EndDate), "Event.StartDate must be less than or equal to Event.EndDate.");
        Ensure(data.Attendees.All(x => !string.IsNullOrWhiteSpace(x.Status)), "Attendee.Status is required.");
        Ensure(data.Milestones.All(x => x.StartDate <= x.EndDate), "Milestone.StartDate must be less than or equal to Milestone.EndDate.");
        Ensure(data.EventCategories.All(x => !string.IsNullOrWhiteSpace(x.Name)), "EventCategory.Name is required.");

        foreach (var department in data.Departments)
        {
            if (department.ManagerId is null)
            {
                continue;
            }

            var manager = data.Members.FirstOrDefault(x => x.Id == department.ManagerId.Value);
            Ensure(manager is not null, $"Department {department.DeptName} manager must reference an existing member.");
            Ensure(manager!.OrgId == department.OrgId,
                $"Department {department.DeptName} manager must belong to the same organization.");
        }

        foreach (var member in data.Members.Where(x => x.DepartmentId.HasValue))
        {
            var department = departmentsById[member.DepartmentId!.Value];
            Ensure(department.OrgId == member.OrgId,
                $"Member {member.Id} department must belong to the same organization.");
        }

        foreach (var eventMember in data.EventMembers)
        {
            var member = membersById[eventMember.MemberId];
            var eventItem = eventsById[eventMember.EventId];
            Ensure(member.OrgId == eventItem.OrgId,
                $"EventMember ({eventMember.EventId}, {eventMember.MemberId}) links different organizations.");
        }

        foreach (var category in data.EventCategories.Where(x => x.LeadMemberId.HasValue))
        {
            var lead = membersById[category.LeadMemberId!.Value];
            var milestone = milestonesById[category.MilestoneId];
            var eventItem = eventsById[milestone.EventId];
            Ensure(lead.OrgId == eventItem.OrgId,
                $"Category {category.Id} lead member must belong to the same organization as its event.");
        }

        foreach (var task in data.Tasks.Where(x => x.AssigneeMemberId.HasValue))
        {
            var assignee = membersById[task.AssigneeMemberId!.Value];
            var category = categoriesById[task.CategoryId];
            var milestone = milestonesById[category.MilestoneId];
            var eventItem = eventsById[milestone.EventId];
            Ensure(assignee.OrgId == eventItem.OrgId,
                $"Task {task.Id} assignee must belong to the same organization as its event.");
        }

        // Global check to keep status vocabulary consistent across board UI.
        var validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TODO",
            "IN_PROGRESS",
            "DONE"
        };

        var invalidStatus = data.Tasks.FirstOrDefault(x => !validStatuses.Contains(x.Status));
        Ensure(invalidStatus is null,
            $"Task {invalidStatus?.Id} has unsupported status '{invalidStatus?.Status}'.");
    }

    private static void EnsureDistinct(IEnumerable<string> keys, string label)
    {
        var duplicate = keys
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        Ensure(duplicate is null, $"Dataset '{label}' contains duplicate key '{duplicate?.Key}'.");
    }

    private static void EnsureDistinct(IEnumerable<Guid> keys, string label)
    {
        var duplicate = keys
            .GroupBy(x => x)
            .FirstOrDefault(x => x.Count() > 1);

        Ensure(duplicate is null, $"Dataset '{label}' contains duplicate key '{duplicate?.Key}'.");
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Invalid mock dataset: {message}");
        }
    }

    private static string CompositeKey(params object?[] parts)
        => string.Join("::", parts.Select(x => x?.ToString() ?? string.Empty));
}
