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
        EnsureDistinct(data.OrganizationRoles.Select(x => x.Id), "organizationRoles.id");
        EnsureDistinct(data.Departments.Select(x => x.Id), "departments.id");
        EnsureDistinct(data.Members.Select(x => x.Id), "members.id");
        EnsureDistinct(data.Events.Select(x => x.Id), "events.id");
        EnsureDistinct(data.Attendees.Select(x => x.Id), "attendees.id");
        EnsureDistinct(data.Milestones.Select(x => x.Id), "milestones.id");
        EnsureDistinct(data.EventCategories.Select(x => x.Id), "eventCategories.id");
        EnsureDistinct(data.Tasks.Select(x => x.Id), "tasks.id");
        EnsureDistinct(data.DepartmentTasks.Select(x => x.Id), "departmentTasks.id");
        EnsureDistinct(data.Requests.Select(x => x.Id), "requests.id");
        EnsureDistinct(data.Notifications.Select(x => x.Id), "notifications.id");
        EnsureDistinct(data.FriendRequests.Select(x => x.Id), "friendRequests.id");
        EnsureDistinct(data.Friendships.Select(x => x.Id), "friendships.id");
        EnsureDistinct(data.Posts.Select(x => x.Id), "posts.id");
        EnsureDistinct(data.PostComments.Select(x => x.Id), "postComments.id");
        EnsureDistinct(data.Conversations.Select(x => x.Id), "conversations.id");
        EnsureDistinct(data.Messages.Select(x => x.Id), "messages.id");

        var organizationIds = data.Organizations.Select(x => x.Id).ToHashSet();
        var organizationRoleIds = data.OrganizationRoles.Select(x => x.Id).ToHashSet();
        var userIds = data.Users.Select(x => x.Id).ToHashSet();
        var memberIds = data.Members.Select(x => x.Id).ToHashSet();
        var departmentIds = data.Departments.Select(x => x.Id).ToHashSet();
        var eventIds = data.Events.Select(x => x.Id).ToHashSet();
        var milestoneIds = data.Milestones.Select(x => x.Id).ToHashSet();
        var categoryIds = data.EventCategories.Select(x => x.Id).ToHashSet();
        var requestIds = data.Requests.Select(x => x.Id).ToHashSet();
        var postIds = data.Posts.Select(x => x.Id).ToHashSet();
        var conversationIds = data.Conversations.Select(x => x.Id).ToHashSet();

        var departmentsById = data.Departments.ToDictionary(x => x.Id);
        var membersById = data.Members.ToDictionary(x => x.Id);
        var eventsById = data.Events.ToDictionary(x => x.Id);
        var milestonesById = data.Milestones.ToDictionary(x => x.Id);
        var categoriesById = data.EventCategories.ToDictionary(x => x.Id);

        Ensure(data.Departments.All(x => organizationIds.Contains(x.OrgId)), "Department.OrgId must exist in organizations.");
        Ensure(data.OrganizationRoles.All(x => organizationIds.Contains(x.OrgId)), "OrganizationRole.OrgId must exist in organizations.");
        Ensure(data.OrganizationRoles.All(x => !string.IsNullOrWhiteSpace(x.RoleName)), "OrganizationRole.RoleName is required.");
        Ensure(data.Members.All(x => organizationIds.Contains(x.OrgId)), "Member.OrgId must exist in organizations.");
        Ensure(data.Members.All(x => userIds.Contains(x.UserId)), "Member.UserId must exist in users.");
        Ensure(data.Members.All(x => x.RoleId is null || organizationRoleIds.Contains(x.RoleId.Value)), "Member.RoleId must exist in organization roles when present.");
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
        Ensure(data.Tasks.All(x => x.CoAssigneeMemberIds.All(memberIds.Contains)), "Task.CoAssigneeMemberIds must reference members.");
        Ensure(data.DepartmentTasks.All(x => organizationIds.Contains(x.OrganizationId)), "DepartmentTask.OrganizationId must exist in organizations.");
        Ensure(data.DepartmentTasks.All(x => departmentIds.Contains(x.DepartmentId)), "DepartmentTask.DepartmentId must exist in departments.");
        Ensure(data.DepartmentTasks.All(x => userIds.Contains(x.CreatedByUserId)), "DepartmentTask.CreatedByUserId must exist in users.");
        Ensure(data.DepartmentTasks.All(x => x.AssigneeMemberIds.All(memberIds.Contains)), "DepartmentTask.AssigneeMemberIds must reference members.");
        Ensure(data.Requests.All(x => organizationIds.Contains(x.OrgId)), "Request.OrgId must exist in organizations.");
        Ensure(data.Requests.All(x => userIds.Contains(x.UserId)), "Request.UserId must exist in users.");
        Ensure(data.Notifications.All(x => userIds.Contains(x.ReceiverId)), "Notification.ReceiverId must exist in users.");
        Ensure(data.Notifications.All(x => x.ActorId is null || userIds.Contains(x.ActorId.Value)), "Notification.ActorId must exist in users when present.");
        Ensure(data.FriendRequests.All(x => userIds.Contains(x.SenderId) && userIds.Contains(x.ReceiverId)), "FriendRequest sender/receiver must exist in users.");
        Ensure(data.Friendships.All(x => userIds.Contains(x.UserAId) && userIds.Contains(x.UserBId)), "Friendship users must exist in users.");
        Ensure(data.Posts.All(x => organizationIds.Contains(x.OrgId)), "Post.OrgId must exist in organizations.");
        Ensure(data.Posts.All(x => userIds.Contains(x.AuthorUserId)), "Post.AuthorUserId must exist in users.");
        Ensure(data.Posts.All(x => x.RelatedEventId is null || eventIds.Contains(x.RelatedEventId.Value)), "Post.RelatedEventId must exist in events when present.");
        Ensure(data.PostComments.All(x => postIds.Contains(x.PostId)), "PostComment.PostId must exist in posts.");
        Ensure(data.PostComments.All(x => userIds.Contains(x.AuthorUserId)), "PostComment.AuthorUserId must exist in users.");
        Ensure(data.ConversationParticipants.All(x => conversationIds.Contains(x.ConversationId)), "ConversationParticipant.ConversationId must exist in conversations.");
        Ensure(data.ConversationParticipants.All(x => userIds.Contains(x.UserId)), "ConversationParticipant.UserId must exist in users.");
        Ensure(data.Messages.All(x => conversationIds.Contains(x.ConversationId)), "Message.ConversationId must exist in conversations.");
        Ensure(data.Messages.All(x => userIds.Contains(x.SenderId)), "Message.SenderId must exist in users.");
        Ensure(data.Messages.All(x => !string.IsNullOrWhiteSpace(x.Content)), "Message.Content is required.");
        Ensure(data.Messages.All(x => x.ReadByUserIds.All(userIds.Contains)), "Message.ReadByUserIds must reference users.");

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
        EnsureDistinct(data.OrganizationRoles.Select(x => CompositeKey(x.OrgId, x.RoleName.Trim().ToUpperInvariant())), "organizationRoles (orgId, roleName)");
        EnsureDistinct(data.EventMembers.Select(x => CompositeKey(x.EventId, x.MemberId)), "eventMembers (eventId, memberId)");
        EnsureDistinct(
            data.Attendees
                .Where(x => x.UserId.HasValue)
                .Select(x => CompositeKey(x.EventId, x.UserId!.Value)),
            "attendees (eventId, userId)");
        EnsureDistinct(data.EventCategories.Select(x => CompositeKey(x.MilestoneId, x.Name?.Trim().ToUpperInvariant() ?? string.Empty)), "eventCategories (milestoneId, name)");
        EnsureDistinct(data.Friendships.Select(x => CompositeKey(SortGuidPair(x.UserAId, x.UserBId))), "friendships (userAId, userBId)");
        EnsureDistinct(data.ConversationParticipants.Select(x => CompositeKey(x.ConversationId, x.UserId)), "conversationParticipants (conversationId, userId)");

        Ensure(data.Events.All(x => x.StartDate <= x.EndDate), "Event.StartDate must be less than or equal to Event.EndDate.");
        Ensure(data.Attendees.All(x => !string.IsNullOrWhiteSpace(x.Status)), "Attendee.Status is required.");
        Ensure(data.Milestones.All(x => x.StartDate <= x.EndDate), "Milestone.StartDate must be less than or equal to Milestone.EndDate.");
        Ensure(data.EventCategories.All(x => !string.IsNullOrWhiteSpace(x.Name)), "EventCategory.Name is required.");
        Ensure(data.FriendRequests.All(x => x.SenderId != x.ReceiverId), "FriendRequest sender and receiver must be different users.");
        Ensure(data.Friendships.All(x => x.UserAId != x.UserBId), "Friendship participants must be different users.");
        Ensure(data.Posts.All(x => !string.IsNullOrWhiteSpace(x.PostType)), "Post.PostType is required.");
        Ensure(data.Posts.All(x => !string.IsNullOrWhiteSpace(x.Visibility)), "Post.Visibility is required.");
        Ensure(data.Conversations.All(x => !string.IsNullOrWhiteSpace(x.Type)), "Conversation.Type is required.");
        Ensure(data.Conversations.All(x => x.CreatedAt <= x.UpdatedAt), "Conversation.CreatedAt must be less than or equal to UpdatedAt.");
        Ensure(data.Messages.All(x => x.CreatedAt <= x.SentAt), "Message.CreatedAt must be less than or equal to SentAt.");

        var validProfileVisibilities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Public",
            "OrganizationOnly",
            "Private"
        };
        var invalidUserProfileVisibility = data.Users.FirstOrDefault(x => !validProfileVisibilities.Contains(x.ProfileVisibility));
        Ensure(invalidUserProfileVisibility is null,
            $"User {invalidUserProfileVisibility?.Id} has unsupported profile visibility '{invalidUserProfileVisibility?.ProfileVisibility}'.");

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

        foreach (var member in data.Members.Where(x => x.RoleId.HasValue))
        {
            var role = data.OrganizationRoles.First(x => x.Id == member.RoleId!.Value);
            Ensure(role.OrgId == member.OrgId,
                $"Member {member.Id} role must belong to the same organization.");
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

        foreach (var task in data.Tasks)
        {
            var category = categoriesById[task.CategoryId];
            var milestone = milestonesById[category.MilestoneId];
            var eventItem = eventsById[milestone.EventId];
            var coAssignees = task.CoAssigneeMemberIds;

            Ensure(coAssignees.Distinct().Count() == coAssignees.Count,
                $"Task {task.Id} contains duplicate CoAssigneeMemberIds.");

            foreach (var coAssigneeMemberId in coAssignees)
            {
                var coAssignee = membersById[coAssigneeMemberId];
                Ensure(coAssignee.OrgId == eventItem.OrgId,
                    $"Task {task.Id} co-assignee {coAssigneeMemberId} must belong to the same organization as its event.");
            }
        }

        foreach (var conversation in data.Conversations)
        {
            var participantIds = data.ConversationParticipants
                .Where(x => x.ConversationId == conversation.Id)
                .Select(x => x.UserId)
                .Distinct()
                .ToList();

            Ensure(participantIds.Count >= 2, $"Conversation {conversation.Id} must have at least 2 participants.");

            if (string.Equals(conversation.Type, "DIRECT", StringComparison.OrdinalIgnoreCase))
            {
                Ensure(participantIds.Count == 2,
                    $"DIRECT conversation {conversation.Id} must have exactly 2 participants.");
            }

            if (conversation.LastMessageId.HasValue)
            {
                var lastMessage = data.Messages.FirstOrDefault(x => x.Id == conversation.LastMessageId.Value);
                Ensure(lastMessage is not null,
                    $"Conversation {conversation.Id} LastMessageId must reference an existing message.");
                Ensure(lastMessage!.ConversationId == conversation.Id,
                    $"Conversation {conversation.Id} LastMessageId must belong to the same conversation.");
            }
        }

        foreach (var departmentTask in data.DepartmentTasks)
        {
            var department = departmentsById[departmentTask.DepartmentId];
            Ensure(department.OrgId == departmentTask.OrganizationId,
                $"DepartmentTask {departmentTask.Id} must belong to the same organization as its department.");

            var creatorMembershipExists = data.Members.Any(x =>
                x.OrgId == departmentTask.OrganizationId
                && x.UserId == departmentTask.CreatedByUserId);
            Ensure(creatorMembershipExists,
                $"DepartmentTask {departmentTask.Id} creator must be a member of the same organization.");

            Ensure(departmentTask.AssigneeMemberIds.Distinct().Count() == departmentTask.AssigneeMemberIds.Count,
                $"DepartmentTask {departmentTask.Id} has duplicate assignee members.");

            foreach (var assigneeMemberId in departmentTask.AssigneeMemberIds)
            {
                var assignee = membersById[assigneeMemberId];
                Ensure(assignee.OrgId == departmentTask.OrganizationId,
                    $"DepartmentTask {departmentTask.Id} assignee {assigneeMemberId} must belong to same organization.");

                Ensure(!assignee.DepartmentId.HasValue || assignee.DepartmentId.Value == departmentTask.DepartmentId,
                    $"DepartmentTask {departmentTask.Id} assignee {assigneeMemberId} must belong to target department when department is assigned.");
            }
        }

        foreach (var message in data.Messages)
        {
            var participantIds = data.ConversationParticipants
                .Where(x => x.ConversationId == message.ConversationId)
                .Select(x => x.UserId)
                .ToHashSet();

            Ensure(participantIds.Contains(message.SenderId),
                $"Message {message.Id} sender must be a participant of conversation {message.ConversationId}.");
            Ensure(message.ReadByUserIds.All(participantIds.Contains),
                $"Message {message.Id} readBy must be a subset of conversation participants.");
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

        var validTaskPriorities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "LOW",
            "MEDIUM",
            "HIGH",
            "URGENT"
        };
        var invalidPriority = data.Tasks.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(x.Priority)
            && !validTaskPriorities.Contains(x.Priority));
        Ensure(invalidPriority is null,
            $"Task {invalidPriority?.Id} has unsupported priority '{invalidPriority?.Priority}'.");

        var validDepartmentTaskStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TODO",
            "DONE"
        };
        var invalidDepartmentTaskStatus = data.DepartmentTasks.FirstOrDefault(x => !validDepartmentTaskStatuses.Contains(x.Status));
        Ensure(invalidDepartmentTaskStatus is null,
            $"DepartmentTask {invalidDepartmentTaskStatus?.Id} has unsupported status '{invalidDepartmentTaskStatus?.Status}'.");

        var validRequestTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "JoinClub",
            "ApproveEvent",
            "ResourceBorrow",
            "JOIN", // backward compatibility for old mock records
            "GeneralOrgRequest",
            "RoleChangeRequest",
            "SupportRequest"
        };
        var invalidRequestType = data.Requests.FirstOrDefault(x => !validRequestTypes.Contains(x.RequestType));
        Ensure(invalidRequestType is null,
            $"Request {invalidRequestType?.Id} has unsupported type '{invalidRequestType?.RequestType}'.");

        var validRequestStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Pending",
            "Approved",
            "Rejected",
            "PENDING",
            "APPROVED",
            "REJECTED"
        };
        var invalidRequestStatus = data.Requests.FirstOrDefault(x => !validRequestStatuses.Contains(x.Status));
        Ensure(invalidRequestStatus is null,
            $"Request {invalidRequestStatus?.Id} has unsupported status '{invalidRequestStatus?.Status}'.");

        var validFriendRequestStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Pending",
            "Accepted",
            "Rejected",
            "Cancelled"
        };
        var invalidFriendRequestStatus = data.FriendRequests.FirstOrDefault(x => !validFriendRequestStatuses.Contains(x.Status));
        Ensure(invalidFriendRequestStatus is null,
            $"FriendRequest {invalidFriendRequestStatus?.Id} has unsupported status '{invalidFriendRequestStatus?.Status}'.");

        var validPostTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "General",
            "Recruitment",
            "Event",
            "Announcement"
        };
        var invalidPostType = data.Posts.FirstOrDefault(x => !validPostTypes.Contains(x.PostType));
        Ensure(invalidPostType is null,
            $"Post {invalidPostType?.Id} has unsupported type '{invalidPostType?.PostType}'.");

        var validPostVisibility = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Private",
            "MembersOnly",
            "Public"
        };
        var invalidPostVisibility = data.Posts.FirstOrDefault(x => !validPostVisibility.Contains(x.Visibility));
        Ensure(invalidPostVisibility is null,
            $"Post {invalidPostVisibility?.Id} has unsupported visibility '{invalidPostVisibility?.Visibility}'.");

        var validNotificationTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FriendRequestReceived","FriendRequestAccepted","FriendRequestRejected",
            "JoinRequestReceived","JoinRequestApproved","JoinRequestRejected","InvitationReceived","MemberRoleChanged","MemberRemoved",
            "EventInvitation","EventRegistrationApproved","EventRegistrationRejected","EventUpdated","EventCancelled","EventReminder",
            "TaskAssigned","TaskDeadlineReminder","TaskStatusChanged","TaskCommentAdded",
            "PostLiked","PostCommented","PostMentioned",
            "SystemAnnouncement","SystemMaintenance","General"
        };
        var invalidNotificationType = data.Notifications.FirstOrDefault(x => !validNotificationTypes.Contains(x.Type));
        Ensure(invalidNotificationType is null,
            $"Notification {invalidNotificationType?.Id} has unsupported type '{invalidNotificationType?.Type}'.");

        var validConversationTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DIRECT",
            "GROUP"
        };
        var invalidConversationType = data.Conversations.FirstOrDefault(x => !validConversationTypes.Contains(x.Type));
        Ensure(invalidConversationType is null,
            $"Conversation {invalidConversationType?.Id} has unsupported type '{invalidConversationType?.Type}'.");

        var validMessageTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TEXT"
        };
        var invalidMessageType = data.Messages.FirstOrDefault(x => !validMessageTypes.Contains(x.MessageType));
        Ensure(invalidMessageType is null,
            $"Message {invalidMessageType?.Id} has unsupported message type '{invalidMessageType?.MessageType}'.");

        var validMessageStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SENT",
            "DELIVERED",
            "READ"
        };
        var invalidMessageStatus = data.Messages.FirstOrDefault(x => !validMessageStatuses.Contains(x.Status));
        Ensure(invalidMessageStatus is null,
            $"Message {invalidMessageStatus?.Id} has unsupported status '{invalidMessageStatus?.Status}'.");

        var validOrganizationRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "President",
            "VicePresident",
            "Manager",
            "Member"
        };
        var invalidOrganizationRole = data.OrganizationRoles.FirstOrDefault(x => !validOrganizationRoles.Contains(x.RoleName));
        Ensure(invalidOrganizationRole is null,
            $"OrganizationRole {invalidOrganizationRole?.Id} has unsupported role '{invalidOrganizationRole?.RoleName}'.");

        foreach (var organization in data.Organizations)
        {
            var memberCount = data.Members.Count(x => x.OrgId == organization.Id);
            Ensure(organization.TotalMembers == memberCount,
                $"Organization {organization.Id} TotalMembers ({organization.TotalMembers}) must match actual members ({memberCount}).");
        }
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

    private static string SortGuidPair(Guid first, Guid second)
        => first.CompareTo(second) <= 0 ? $"{first}::{second}" : $"{second}::{first}";
}
