// ---- Seeder dữ liệu mẫu (idempotent) cho môi trường local/integration ----
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Shared;
using TaskStatus = Org.Shared.TaskStatus;

namespace Org.Backend.Infrastructure.Database;

public static class DatabaseSeeder
{
    // Số lượng bản ghi chuẩn cho mỗi nhóm seed.
    private const int SeedCount = 10;

    // ---- Orchestrator: chạy seed theo từng stage để đảm bảo đúng thứ tự FK ----
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var seedContext = await SeedIdentityDataAsync(db, cancellationToken);
        await SeedOrgCoreDataAsync(db, seedContext, cancellationToken);
        await SeedMembershipAndMilestoneDataAsync(db, seedContext, cancellationToken);
        await SeedOperationalDataAsync(db, seedContext, cancellationToken);
    }

    // ---- Stage 1: seed thực thể gốc độc lập (Organization, Permission, User) ----
    private static async Task<SeedContext> SeedIdentityDataAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var orgNames = Enumerable.Range(1, SeedCount).Select(i => $"Organization {i}").ToList();
        var permissionKeys = Enumerable.Range(1, SeedCount).Select(i => $"module.permission.{i}").ToList();
        var userEmails = Enumerable.Range(1, SeedCount).Select(i => $"user{i}@example.com").ToList();

        // Tải khóa hiện có một lần để tránh gọi AnyAsync lặp theo từng dòng.
        var existingOrgNames = await db.Organizations
            .Where(x => orgNames.Contains(x.OrgName))
            .Select(x => x.OrgName)
            .ToListAsync(cancellationToken);

        var existingPermissionKeys = await db.Permissions
            .Where(x => permissionKeys.Contains(x.PermissionKey))
            .Select(x => x.PermissionKey)
            .ToListAsync(cancellationToken);

        var existingUserEmails = await db.Users
            .Where(x => userEmails.Contains(x.Email))
            .Select(x => x.Email)
            .ToListAsync(cancellationToken);

        var orgNameSet = existingOrgNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var permissionKeySet = existingPermissionKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var userEmailSet = existingUserEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i <= SeedCount; i++)
        {
            var orgName = $"Organization {i}";
            if (!orgNameSet.Contains(orgName))
            {
                db.Organizations.Add(new Organization
                {
                    OrgName = orgName,
                    Description = $"Description for organization {i}",
                    AvatarUrl = $"https://example.com/org-{i}.png",
                    CoverUrl = $"https://example.com/org-cover-{i}.png",
                    FoundingDate = DateTime.UtcNow.Date.AddYears(-i),
                    Location = $"Campus {i}",
                    TotalMembers = 10,
                    Status = i % 2 == 0 ? OrgStatus.Inactive : OrgStatus.Active
                });
            }

            var permissionKey = $"module.permission.{i}";
            if (!permissionKeySet.Contains(permissionKey))
            {
                db.Permissions.Add(new Permission
                {
                    PermissionKey = permissionKey,
                    DisplayName = $"Permission {i}",
                    ModuleGroup = i % 2 == 0 ? "Events" : "Members"
                });
            }

            var userEmail = $"user{i}@example.com";
            if (!userEmailSet.Contains(userEmail))
            {
                db.Users.Add(new User
                {
                    FullName = $"User {i}",
                    Email = userEmail,
                    PasswordHash = $"hash-user-{i}",
                    PhoneNumber = $"09000000{i:00}",
                    Dob = DateTime.UtcNow.Date.AddYears(-20).AddDays(i),
                    Gender = i % 2 == 0 ? "Female" : "Male",
                    Address = $"Address {i}",
                    AvatarUrl = $"https://example.com/user-{i}.png",
                    Bio = $"Bio for user {i}",
                    SocialLinks = "{\"facebook\":\"https://facebook.com/example\",\"linkedin\":\"https://linkedin.com\"}",
                    Status = UserStatus.Active,
                    LastLogin = DateTime.UtcNow.AddHours(-i)
                });
            }
        }

        // Flush stage 1 trước khi sang stage phụ thuộc.
        await db.SaveChangesAsync(cancellationToken);

        var organizations = await db.Organizations.Where(x => orgNames.Contains(x.OrgName)).ToListAsync(cancellationToken);
        var users = await db.Users.Where(x => userEmails.Contains(x.Email)).ToListAsync(cancellationToken);
        var permissions = await db.Permissions.Where(x => permissionKeys.Contains(x.PermissionKey)).ToListAsync(cancellationToken);

        return new SeedContext
        {
            Organizations = OrderByExpected(organizations, orgNames, x => x.OrgName),
            Users = OrderByExpected(users, userEmails, x => x.Email),
            Permissions = OrderByExpected(permissions, permissionKeys, x => x.PermissionKey)
        };
    }

    // ---- Stage 2: seed dữ liệu theo tổ chức (Role, Department, Event, Request, Resource, Activity) ----
    private static async Task SeedOrgCoreDataAsync(AppDbContext db, SeedContext seedContext, CancellationToken cancellationToken)
    {
        var orgIds = seedContext.Organizations.Select(x => x.Id).ToList();

        // Dùng composite key để kiểm tra trùng nhanh theo natural key từng bảng.
        var roleKeySet = (await db.Roles
                .Where(x => orgIds.Contains(x.OrgId))
                .Select(x => new { x.OrgId, x.RoleName })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.OrgId, x.RoleName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var departmentKeySet = (await db.Departments
                .Where(x => orgIds.Contains(x.OrgId))
                .Select(x => new { x.OrgId, x.DeptName })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.OrgId, x.DeptName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var eventKeySet = (await db.Events
                .Where(x => orgIds.Contains(x.OrgId))
                .Select(x => new { x.OrgId, x.EventName })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.OrgId, x.EventName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var requestKeySet = (await db.Requests
                .Where(x => orgIds.Contains(x.OrgId))
                .Select(x => new { x.SenderId, x.Content })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.SenderId, x.Content))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var resourceKeySet = (await db.Resources
                .Where(x => orgIds.Contains(x.OrgId))
                .Select(x => new { x.OrgId, x.ResourceName })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.OrgId, x.ResourceName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var activityKeySet = (await db.ActivityHistories
                .Where(x => orgIds.Contains(x.OrgId))
                .Select(x => new { x.OrgId, x.Title })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.OrgId, x.Title))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expectedRoleKeys = new List<string>(SeedCount);
        var expectedDepartmentKeys = new List<string>(SeedCount);
        var expectedEventKeys = new List<string>(SeedCount);

        for (var i = 0; i < SeedCount; i++)
        {
            var org = seedContext.Organizations[i];
            var user = seedContext.Users[i];

            var roleName = $"Role {i + 1}";
            var roleKey = CompositeKey(org.Id, roleName);
            expectedRoleKeys.Add(roleKey);
            if (!roleKeySet.Contains(roleKey))
            {
                db.Roles.Add(new Role
                {
                    OrgId = org.Id,
                    RoleName = roleName,
                    Description = $"Role description {i + 1}",
                    IsDefault = i == 0
                });
            }

            var departmentName = $"Department {i + 1}";
            var departmentKey = CompositeKey(org.Id, departmentName);
            expectedDepartmentKeys.Add(departmentKey);
            if (!departmentKeySet.Contains(departmentKey))
            {
                db.Departments.Add(new Department
                {
                    OrgId = org.Id,
                    DeptName = departmentName,
                    Function = $"Function {i + 1}"
                });
            }

            var eventName = $"Event {i + 1}";
            var eventKey = CompositeKey(org.Id, eventName);
            expectedEventKeys.Add(eventKey);
            if (!eventKeySet.Contains(eventKey))
            {
                db.Events.Add(new Event
                {
                    OrgId = org.Id,
                    EventName = eventName,
                    Description = $"Event description {i + 1}",
                    StartDate = DateTime.UtcNow.Date.AddDays(i),
                    EndDate = DateTime.UtcNow.Date.AddDays(i + 2),
                    Budget = 1000 + i * 100,
                    Location = $"Hall {i + 1}",
                    TargetParticipants = 50 + i,
                    Tags = $"[\"club\",\"student\",\"tag{i + 1}\"]",
                    Status = EventStatus.Planning,
                    AverageRating = 4.0f
                });
            }

            var requestContent = $"Request content {i + 1}";
            var requestKey = CompositeKey(user.Id, requestContent);
            if (!requestKeySet.Contains(requestKey))
            {
                db.Requests.Add(new Request
                {
                    SenderId = user.Id,
                    OrgId = org.Id,
                    RequestType = (i % 3) switch
                    {
                        0 => RequestType.JoinClub,
                        1 => RequestType.ApproveEvent,
                        _ => RequestType.ResourceBorrow
                    },
                    Content = requestContent,
                    RequestDate = DateTime.UtcNow.AddDays(-i),
                    Status = RequestStatus.Pending
                });
            }

            var resourceName = $"Resource {i + 1}";
            var resourceKey = CompositeKey(org.Id, resourceName);
            if (!resourceKeySet.Contains(resourceKey))
            {
                db.Resources.Add(new Resource
                {
                    OrgId = org.Id,
                    ResourceName = resourceName,
                    Type = i % 2 == 0 ? "Equipment" : "Room",
                    Quantity = 5 + i,
                    Status = ResourceStatus.Available
                });
            }

            var activityTitle = $"Activity {i + 1}";
            var activityKey = CompositeKey(org.Id, activityTitle);
            if (!activityKeySet.Contains(activityKey))
            {
                db.ActivityHistories.Add(new ActivityHistory
                {
                    OrgId = org.Id,
                    Title = activityTitle,
                    Content = $"Activity content {i + 1}",
                    CoverUrl = $"https://example.com/activity-{i + 1}.png",
                    ActivityDate = DateTime.UtcNow.AddDays(-i),
                    Type = ActivityType.Other,
                    IsPublic = true
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var roles = await db.Roles.Where(x => orgIds.Contains(x.OrgId)).ToListAsync(cancellationToken);
        var departments = await db.Departments.Where(x => orgIds.Contains(x.OrgId)).ToListAsync(cancellationToken);
        var events = await db.Events.Where(x => orgIds.Contains(x.OrgId)).ToListAsync(cancellationToken);

        seedContext.Roles = OrderByExpected(roles, expectedRoleKeys, x => CompositeKey(x.OrgId, x.RoleName));
        seedContext.Departments = OrderByExpected(departments, expectedDepartmentKeys, x => CompositeKey(x.OrgId, x.DeptName));
        seedContext.Events = OrderByExpected(events, expectedEventKeys, x => CompositeKey(x.OrgId, x.EventName));
    }

    // ---- Stage 3: seed quan hệ membership + report + milestone (phụ thuộc stage 1-2) ----
    private static async Task SeedMembershipAndMilestoneDataAsync(AppDbContext db, SeedContext seedContext, CancellationToken cancellationToken)
    {
        var userIds = seedContext.Users.Select(x => x.Id).ToList();
        var orgIds = seedContext.Organizations.Select(x => x.Id).ToList();
        var roleIds = seedContext.Roles.Select(x => x.Id).ToList();
        var permissionIds = seedContext.Permissions.Select(x => x.Id).ToList();
        var eventIds = seedContext.Events.Select(x => x.Id).ToList();

        var memberKeySet = (await db.Members
                .Where(x => userIds.Contains(x.UserId) && orgIds.Contains(x.OrgId))
                .Select(x => new { x.UserId, x.OrgId })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.UserId, x.OrgId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rolePermissionKeySet = (await db.RolePermissions
                .Where(x => roleIds.Contains(x.RoleId) && permissionIds.Contains(x.PermissionId))
                .Select(x => new { x.RoleId, x.PermissionId })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.RoleId, x.PermissionId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var eventReportKeySet = (await db.EventReports
                .Where(x => eventIds.Contains(x.EventId))
                .Select(x => x.EventId)
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var milestoneKeySet = (await db.Milestones
                .Where(x => eventIds.Contains(x.EventId))
                .Select(x => new { x.EventId, x.Title })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.EventId, x.Title))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expectedMemberKeys = new List<string>(SeedCount);
        var expectedMilestoneKeys = new List<string>(SeedCount);

        for (var i = 0; i < SeedCount; i++)
        {
            var user = seedContext.Users[i];
            var org = seedContext.Organizations[i];
            var role = seedContext.Roles[i];
            var department = seedContext.Departments[i];
            var permission = seedContext.Permissions[i];
            var @event = seedContext.Events[i];

            var memberKey = CompositeKey(user.Id, org.Id);
            expectedMemberKeys.Add(memberKey);
            if (!memberKeySet.Contains(memberKey))
            {
                db.Members.Add(new Member
                {
                    UserId = user.Id,
                    OrgId = org.Id,
                    DepartmentId = department.Id,
                    RoleId = role.Id,
                    JoinDate = DateTime.UtcNow.Date.AddDays(-30 + i)
                });
            }

            var rolePermissionKey = CompositeKey(role.Id, permission.Id);
            if (!rolePermissionKeySet.Contains(rolePermissionKey))
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }

            var reportKey = CompositeKey(@event.Id);
            if (!eventReportKeySet.Contains(reportKey))
            {
                db.EventReports.Add(new EventReport
                {
                    EventId = @event.Id,
                    ActualAttendance = 40 + i,
                    ActualBudget = 800 + i * 50,
                    RatingAverage = 4.0f,
                    Summary = $"Report summary {i + 1}"
                });
            }

            var milestoneTitle = $"Milestone {i + 1}";
            var milestoneKey = CompositeKey(@event.Id, milestoneTitle);
            expectedMilestoneKeys.Add(milestoneKey);
            if (!milestoneKeySet.Contains(milestoneKey))
            {
                db.Milestones.Add(new Milestone
                {
                    EventId = @event.Id,
                    Title = milestoneTitle,
                    OrderIndex = i + 1,
                    StartDate = DateTime.UtcNow.Date.AddDays(i + 1),
                    EndDate = DateTime.UtcNow.Date.AddDays(i + 7),
                    Status = MilestoneStatus.InProgress
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var members = await db.Members
            .Where(x => userIds.Contains(x.UserId) && orgIds.Contains(x.OrgId))
            .ToListAsync(cancellationToken);

        var milestones = await db.Milestones
            .Where(x => eventIds.Contains(x.EventId))
            .ToListAsync(cancellationToken);

        seedContext.Members = OrderByExpected(members, expectedMemberKeys, x => CompositeKey(x.UserId, x.OrgId));
        seedContext.Milestones = OrderByExpected(milestones, expectedMilestoneKeys, x => CompositeKey(x.EventId, x.Title));
    }

    // ---- Stage 4: seed dữ liệu vận hành (EventMember, Attendee, EventCategory, Task, Asset, link Resource) ----
    private static async Task SeedOperationalDataAsync(AppDbContext db, SeedContext seedContext, CancellationToken cancellationToken)
    {
        var eventIds = seedContext.Events.Select(x => x.Id).ToList();
        var milestoneIds = seedContext.Milestones.Select(x => x.Id).ToList();
        var memberIds = seedContext.Members.Select(x => x.Id).ToList();
        var userIds = seedContext.Users.Select(x => x.Id).ToList();

        var eventMemberKeySet = (await db.EventMembers
                .Where(x => eventIds.Contains(x.EventId) && memberIds.Contains(x.MemberId))
                .Select(x => new { x.EventId, x.MemberId })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.EventId, x.MemberId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var attendeeKeySet = (await db.Attendees
                .Where(x => eventIds.Contains(x.EventId) && x.UserId != null && userIds.Contains(x.UserId.Value))
                .Select(x => new { x.EventId, x.UserId })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.EventId, x.UserId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Tạo EventCategory trước vì Task hiện phụ thuộc FK EventCategoryId.
        var eventCategoryKeySet = (await db.EventCategories
                .Where(x => milestoneIds.Contains(x.MilestoneId))
                .Select(x => new { x.MilestoneId, x.CategoryName })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.MilestoneId, x.CategoryName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expectedCategoryKeys = new List<string>(SeedCount);
        for (var i = 0; i < SeedCount; i++)
        {
            var milestone = seedContext.Milestones[i];
            var department = seedContext.Departments[i];
            var categoryName = i % 2 == 0 ? "Logistics" : "Technical";
            var categoryKey = CompositeKey(milestone.Id, categoryName);
            expectedCategoryKeys.Add(categoryKey);

            if (!eventCategoryKeySet.Contains(categoryKey))
            {
                db.EventCategories.Add(new EventCategory
                {
                    MilestoneId = milestone.Id,
                    CategoryName = categoryName,
                    OrderIndex = 1,
                    Description = $"Auto-seeded {categoryName} workstream",
                    OwnerDepartmentId = department.Id
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var eventCategories = await db.EventCategories
            .Where(x => milestoneIds.Contains(x.MilestoneId))
            .ToListAsync(cancellationToken);

        seedContext.EventCategories = OrderByExpected(eventCategories, expectedCategoryKeys, x => CompositeKey(x.MilestoneId, x.CategoryName));

        var categoryIds = seedContext.EventCategories.Select(x => x.Id).ToList();

        // Sau khi có category, mới seed Task để tránh vi phạm khóa ngoại.
        var taskKeySet = (await db.Tasks
                .Where(x => categoryIds.Contains(x.EventCategoryId))
                .Select(x => new { x.EventCategoryId, x.TaskName })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.EventCategoryId, x.TaskName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var assetKeySet = (await db.DigitalAssets
                .Where(x => eventIds.Contains(x.EventId))
                .Select(x => new { x.EventId, x.FileName })
                .ToListAsync(cancellationToken))
            .Select(x => CompositeKey(x.EventId, x.FileName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var resources = await db.Resources
            .Where(x => seedContext.Organizations.Select(o => o.Id).Contains(x.OrgId))
            .ToListAsync(cancellationToken);

        for (var i = 0; i < SeedCount; i++)
        {
            var @event = seedContext.Events[i];
            var member = seedContext.Members[i];
            var user = seedContext.Users[i];
            var department = seedContext.Departments[i];
            var category = seedContext.EventCategories[i];

            if (department.ManagerId != member.Id)
            {
                department.ManagerId = member.Id;
            }

            var eventMemberKey = CompositeKey(@event.Id, member.Id);
            if (!eventMemberKeySet.Contains(eventMemberKey))
            {
                db.EventMembers.Add(new EventMember
                {
                    EventId = @event.Id,
                    MemberId = member.Id,
                    EventRole = i % 2 == 0 ? "Logistics" : "MC",
                    AssignedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            var attendeeKey = CompositeKey(@event.Id, user.Id);
            if (!attendeeKeySet.Contains(attendeeKey))
            {
                db.Attendees.Add(new Attendee
                {
                    EventId = @event.Id,
                    UserId = user.Id,
                    GuestName = user.FullName,
                    Email = user.Email,
                    TicketType = "Standard",
                    CheckInTime = DateTime.UtcNow.AddHours(-i),
                    Status = AttendeeStatus.Attended
                });
            }

            var taskName = $"Task {i + 1}";
            var taskKey = CompositeKey(category.Id, taskName);
            if (!taskKeySet.Contains(taskKey))
            {
                db.Tasks.Add(new OrgTask
                {
                    EventCategoryId = category.Id,
                    TaskName = taskName,
                    AssigneeId = member.Id,
                    DeptId = department.Id,
                    Priority = i % 2 == 0 ? TaskPriority.Medium : TaskPriority.High,
                    Deadline = DateTime.UtcNow.Date.AddDays(i + 5),
                    Status = i % 2 == 0 ? TaskStatus.Todo : TaskStatus.InProgress,
                    Note = $"Task note {i + 1}"
                });
            }

            var fileName = $"asset-{i + 1}.pdf";
            var assetKey = CompositeKey(@event.Id, fileName);
            if (!assetKeySet.Contains(assetKey))
            {
                db.DigitalAssets.Add(new DigitalAsset
                {
                    EventId = @event.Id,
                    FileName = fileName,
                    FileUrl = $"https://example.com/assets/{i + 1}.pdf",
                    FileType = FileType.Document,
                    UploadedBy = member.Id
                });
            }

            var linkedResource = resources.FirstOrDefault(x => x.OrgId == seedContext.Organizations[i].Id && x.ResourceName == $"Resource {i + 1}");
            if (linkedResource is not null && linkedResource.EventId != @event.Id)
            {
                linkedResource.EventId = @event.Id;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    // ---- Trả về danh sách đã sắp theo thứ tự expected key để map index ổn định giữa các stage ----
    private static List<T> OrderByExpected<T>(IEnumerable<T> source, IEnumerable<string> expectedKeys, Func<T, string> selector)
    {
        var items = source.ToList();
        var ordered = new List<T>();

        foreach (var key in expectedKeys)
        {
            var match = items.FirstOrDefault(item => string.Equals(selector(item), key, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                throw new InvalidOperationException($"Seed reference not found for key '{key}'.");
            }

            ordered.Add(match);
        }

        return ordered;
    }

    // ---- Utility tạo khóa ghép chuẩn để kiểm tra trùng dữ liệu ----
    private static string CompositeKey(params object?[] parts)
    {
        return string.Join("::", parts.Select(x => x?.ToString() ?? string.Empty));
    }

    // ---- Context trung gian chia sẻ kết quả seed giữa các stage ----
    private sealed class SeedContext
    {
        public List<Organization> Organizations { get; set; } = [];
        public List<User> Users { get; set; } = [];
        public List<Permission> Permissions { get; set; } = [];
        public List<Role> Roles { get; set; } = [];
        public List<Department> Departments { get; set; } = [];
        public List<Event> Events { get; set; } = [];
        public List<Member> Members { get; set; } = [];
        public List<Milestone> Milestones { get; set; } = [];
        public List<EventCategory> EventCategories { get; set; } = [];
    }
}
