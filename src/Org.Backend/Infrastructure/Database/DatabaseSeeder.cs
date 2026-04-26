// ---- Seeder dữ liệu mẫu (idempotent) cho môi trường local/integration ----
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Shared;
using TaskStatus = Org.Shared.TaskStatus;

namespace Org.Backend.Infrastructure.Database;

public static class DatabaseSeeder
{
    private const int Example1ExtraCount = 10;

    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var seedContext = await SeedIdentityDataAsync(db, cancellationToken);
        await SeedOrgCoreDataAsync(db, seedContext, cancellationToken);
        await SeedMembershipAndMilestoneDataAsync(db, seedContext, cancellationToken);
        await SeedOperationalDataAsync(db, seedContext, cancellationToken);
    }

    private static async Task<SeedContext> SeedIdentityDataAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var orgNames = Enumerable.Range(1, 6).Select(i => $"Organization {i}").ToList();
        var permissionKeys = Enumerable.Range(1, 20).Select(i => $"module.permission.{i}").ToList();
        var userEmails = Enumerable.Range(1, 40).Select(i => $"example{i}@gmail.com").ToList();
        userEmails.AddRange(Enumerable.Range(1, Example1ExtraCount).Select(i => $"example1.member{i}@example.com"));

        var existingOrgNames = await db.Organizations.Where(x => orgNames.Contains(x.OrgName)).Select(x => x.OrgName).ToListAsync(cancellationToken);
        var existingPermissionKeys = await db.Permissions.Where(x => permissionKeys.Contains(x.PermissionKey)).Select(x => x.PermissionKey).ToListAsync(cancellationToken);
        var existingUsers = await db.Users.Where(x => userEmails.Contains(x.Email)).ToListAsync(cancellationToken);

        var orgNameSet = existingOrgNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var permissionKeySet = existingPermissionKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var userEmailSet = existingUsers.Select(x => x.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i <= 6; i++)
        {
            var orgName = $"Organization {i}";
            if (!orgNameSet.Contains(orgName))
            {
                db.Organizations.Add(new Organization
                {
                    OrgName = orgName,
                    Description = $"Description for organization {i}",
                    AvatarUrl = $"/images/mockimages/org-{i}.jpg",
                    CoverUrl = $"/images/mockimages/org-cover-{i}.jpg",
                    FoundingDate = DateTime.UtcNow.Date.AddYears(-i),
                    Location = $"Campus {i}",
                    TotalMembers = 10,
                    Status = OrgStatus.Active
                });
            }
        }

        for (var i = 1; i <= 20; i++)
        {
            var permissionKey = $"module.permission.{i}";
            if (!permissionKeySet.Contains(permissionKey))
            {
                db.Permissions.Add(new Permission
                {
                    PermissionKey = permissionKey,
                    DisplayName = $"Permission {i}",
                    ModuleGroup = "General"
                });
            }
        }

        for (var i = 1; i <= 40; i++)
        {
            var userEmail = $"example{i}@gmail.com";
            var seedRawPassword = $"example{i}";
            if (!userEmailSet.Contains(userEmail))
            {
                db.Users.Add(new User
                {
                    FullName = $"User {i}",
                    Email = userEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedRawPassword),
                    PhoneNumber = $"09000000{i:00}",
                    Dob = DateTime.UtcNow.Date.AddYears(-20),
                    Gender = i % 2 == 0 ? "Female" : "Male",
                    Address = $"Address {i}",
                    AvatarUrl = $"/images/mockimages/user-{i}.jpg",
                    Bio = $"Bio for user {i}",
                    Status = UserStatus.Active,
                    LastLogin = DateTime.UtcNow
                });
            }
            else
            {
                var existingUser = existingUsers.First(x => string.Equals(x.Email, userEmail, StringComparison.OrdinalIgnoreCase));
                try { if (!BCrypt.Net.BCrypt.Verify(seedRawPassword, existingUser.PasswordHash)) existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedRawPassword); }
                catch { existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedRawPassword); }
            }
        }

        for (var i = 1; i <= Example1ExtraCount; i++)
        {
            var userEmail = $"example1.member{i}@example.com";
            var seedRawPassword = $"example1";
            if (!userEmailSet.Contains(userEmail))
            {
                db.Users.Add(new User { FullName = $"Example1 Member {i}", Email = userEmail, PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedRawPassword), PhoneNumber = $"09110000{i:00}", Dob = DateTime.UtcNow.Date.AddYears(-21), Gender = "Male", Status = UserStatus.Active });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SeedContext
        {
            Organizations = OrderByExpected(await db.Organizations.Where(x => orgNames.Contains(x.OrgName)).ToListAsync(cancellationToken), orgNames, x => x.OrgName),
            Users = OrderByExpected(await db.Users.Where(x => userEmails.Contains(x.Email)).ToListAsync(cancellationToken), userEmails, x => x.Email),
            Permissions = OrderByExpected(await db.Permissions.Where(x => permissionKeys.Contains(x.PermissionKey)).ToListAsync(cancellationToken), permissionKeys, x => x.PermissionKey)
        };
    }

    private static async Task SeedOrgCoreDataAsync(AppDbContext db, SeedContext seedContext, CancellationToken cancellationToken)
    {
        var orgIds = seedContext.Organizations.Select(x => x.Id).ToList();

        var roleKeySet = (await db.Roles.Where(x => orgIds.Contains(x.OrgId)).Select(x => new { x.OrgId, x.RoleName }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.OrgId, x.RoleName)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var departmentKeySet = (await db.Departments.Where(x => orgIds.Contains(x.OrgId)).Select(x => new { x.OrgId, x.DeptName }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.OrgId, x.DeptName)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var eventKeySet = (await db.Events.Where(x => orgIds.Contains(x.OrgId)).Select(x => new { x.OrgId, x.EventName }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.OrgId, x.EventName)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var requestKeySet = (await db.Requests.Where(x => orgIds.Contains(x.OrgId)).Select(x => new { x.OrgId, x.Content }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.OrgId, x.Content)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var resourceKeySet = (await db.Resources.Where(x => orgIds.Contains(x.OrgId)).Select(x => new { x.OrgId, x.ResourceName }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.OrgId, x.ResourceName)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activityKeySet = (await db.ActivityHistories.Where(x => orgIds.Contains(x.OrgId)).Select(x => new { x.OrgId, x.Title }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.OrgId, x.Title)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expectedRoleKeys = new List<string>();
        var expectedDepartmentKeys = new List<string>();
        var expectedEventKeys = new List<string>();

        int eventCounter = 1;
        for (var i = 0; i < 6; i++)
        {
            var org = seedContext.Organizations[i];

            var leaderKey = CompositeKey(org.Id, "President");
            var managerKey = CompositeKey(org.Id, "Manager");
            var memberKey = CompositeKey(org.Id, "Member");
            expectedRoleKeys.Add(leaderKey);
            expectedRoleKeys.Add(managerKey);
            expectedRoleKeys.Add(memberKey);
            if (!roleKeySet.Contains(leaderKey)) db.Roles.Add(new Role { OrgId = org.Id, RoleName = "President", Description = "President role" });
            if (!roleKeySet.Contains(managerKey)) db.Roles.Add(new Role { OrgId = org.Id, RoleName = "Manager", Description = "Manager role" });
            if (!roleKeySet.Contains(memberKey)) db.Roles.Add(new Role { OrgId = org.Id, RoleName = "Member", Description = "Member role", IsDefault = true });

            int deptCount = i < 2 ? 4 : 3;
            var deptNames = deptCount == 4 
                ? new[] { "Hậu cần", "Truyền thông", "Quản lí", "Chủ nhiệm" } 
                : new[] { "Hậu cần", "Quản lí", "Chủ nhiệm" };
                
            for (var j = 0; j < deptCount; j++)
            {
                var dName = deptNames[j];
                var dKey = CompositeKey(org.Id, dName);
                expectedDepartmentKeys.Add(dKey);
                if (!departmentKeySet.Contains(dKey)) db.Departments.Add(new Department { OrgId = org.Id, DeptName = dName, Function = "Func" });
            }

            int evtCount = i < 4 ? 7 : 6;
            for (var j = 0; j < evtCount; j++)
            {
                var eName = $"Event {eventCounter++} of Org {i + 1}";
                var eKey = CompositeKey(org.Id, eName);
                expectedEventKeys.Add(eKey);
                if (!eventKeySet.Contains(eKey)) db.Events.Add(new Event { OrgId = org.Id, EventName = eName, Description = "Desc", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), Budget = 1000, TargetParticipants = 50, Status = EventStatus.Planning });
            }

            for (var j = 0; j < 5; j++)
            {
                var rContent = $"Request {j + 1} of Org {i + 1}";
                var rKey = CompositeKey(org.Id, rContent);
                if (!requestKeySet.Contains(rKey)) db.Requests.Add(new Request { SenderId = seedContext.Users[i * 5 + j].Id, OrgId = org.Id, RequestType = RequestType.JoinClub, Content = rContent, Status = RequestStatus.Pending, RequestDate = DateTime.UtcNow });
            }

            for (var j = 0; j < 5; j++)
            {
                var resName = $"Resource {j + 1} of Org {i + 1}";
                var resKey = CompositeKey(org.Id, resName);
                if (!resourceKeySet.Contains(resKey)) db.Resources.Add(new Resource { OrgId = org.Id, ResourceName = resName, Type = "Type", Quantity = 5, Status = ResourceStatus.Available });
            }

            int actCount = i < 4 ? 7 : 6;
            for (var j = 0; j < actCount; j++)
            {
                var actTitle = $"Activity {j + 1} of Org {i + 1}";
                var actKey = CompositeKey(org.Id, actTitle);
                if (!activityKeySet.Contains(actKey)) db.ActivityHistories.Add(new ActivityHistory { OrgId = org.Id, Title = actTitle, Content = "Content", ActivityDate = DateTime.UtcNow, Type = ActivityType.Other, IsPublic = true });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        seedContext.Roles = OrderByExpected(await db.Roles.Where(x => orgIds.Contains(x.OrgId)).ToListAsync(cancellationToken), expectedRoleKeys, x => CompositeKey(x.OrgId, x.RoleName));
        seedContext.Departments = OrderByExpected(await db.Departments.Where(x => orgIds.Contains(x.OrgId)).ToListAsync(cancellationToken), expectedDepartmentKeys, x => CompositeKey(x.OrgId, x.DeptName));
        seedContext.Events = OrderByExpected(await db.Events.Where(x => orgIds.Contains(x.OrgId)).ToListAsync(cancellationToken), expectedEventKeys, x => CompositeKey(x.OrgId, x.EventName));
    }

    private static async Task SeedMembershipAndMilestoneDataAsync(AppDbContext db, SeedContext seedContext, CancellationToken cancellationToken)
    {
        var roleIds = seedContext.Roles.Select(x => x.Id).ToList();
        var permIds = seedContext.Permissions.Select(x => x.Id).ToList();
        var userIds = seedContext.Users.Select(x => x.Id).ToList();
        var orgIds = seedContext.Organizations.Select(x => x.Id).ToList();
        var evtIds = seedContext.Events.Select(x => x.Id).ToList();

        var rolePermKeySet = (await db.RolePermissions.Where(x => roleIds.Contains(x.RoleId)).Select(x => new { x.RoleId, x.PermissionId }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.RoleId, x.PermissionId)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var memberKeySet = (await db.Members.Where(x => userIds.Contains(x.UserId)).Select(x => new { x.UserId, x.OrgId }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.UserId, x.OrgId)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var reportKeySet = (await db.EventReports.Where(x => evtIds.Contains(x.EventId)).Select(x => x.EventId).ToListAsync(cancellationToken)).Select(x => CompositeKey(x)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var milestoneKeySet = (await db.Milestones.Where(x => evtIds.Contains(x.EventId)).Select(x => new { x.EventId, x.Title }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.EventId, x.Title)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < 6; i++)
        {
            var leaderRole = seedContext.Roles[i * 2];
            var memberRole = seedContext.Roles[i * 2 + 1];

            // Leader gets permissions 1,2,3
            for(int p=0; p<3; p++) {
                var rpKey = CompositeKey(leaderRole.Id, seedContext.Permissions[p].Id);
                if (!rolePermKeySet.Contains(rpKey)) db.RolePermissions.Add(new RolePermission { RoleId = leaderRole.Id, PermissionId = seedContext.Permissions[p].Id });
            }
            // Member gets permissions 4,5
            for(int p=3; p<5; p++) {
                var rpKey = CompositeKey(memberRole.Id, seedContext.Permissions[p].Id);
                if (!rolePermKeySet.Contains(rpKey)) db.RolePermissions.Add(new RolePermission { RoleId = memberRole.Id, PermissionId = seedContext.Permissions[p].Id });
            }
        }

        var expectedMemberKeys = new List<string>();
        var orgMemberCount = new Dictionary<Guid, int>();
        for (var i = 0; i < 40; i++)
        {
            var org = seedContext.Organizations[i % 6];
            if (!orgMemberCount.ContainsKey(org.Id)) orgMemberCount[org.Id] = 0;

            var orgDepts = seedContext.Departments.Where(d => d.OrgId == org.Id).ToList();
            int count = orgMemberCount[org.Id];
            
            string roleName = count == 0 ? "President" : (count == 1 ? "Manager" : "Member");
            var role = seedContext.Roles.First(r => r.OrgId == org.Id && r.RoleName == roleName);
            
            Department dept;
            if (count == 0)
                dept = orgDepts.First(d => d.DeptName == "Chủ nhiệm");
            else if (count == 1)
                dept = orgDepts.First(d => d.DeptName == "Quản lí");
            else
            {
                var remDepts = orgDepts.Where(d => d.DeptName != "Chủ nhiệm" && d.DeptName != "Quản lí").ToList();
                if (remDepts.Count == 0) remDepts = orgDepts;
                dept = remDepts[(count - 2) % remDepts.Count];
            }
            
            var memKey = CompositeKey(seedContext.Users[i].Id, org.Id);
            expectedMemberKeys.Add(memKey);
            if (!memberKeySet.Contains(memKey)) db.Members.Add(new Member { UserId = seedContext.Users[i].Id, OrgId = org.Id, DepartmentId = dept.Id, RoleId = role.Id, JoinDate = DateTime.UtcNow });
            
            orgMemberCount[org.Id]++;
        }

        for (var i = 0; i < 40; i++)
        {
            if (i < 20)
            {
                var rKey = CompositeKey(seedContext.Events[i].Id);
                if (!reportKeySet.Contains(rKey)) db.EventReports.Add(new EventReport { EventId = seedContext.Events[i].Id, ActualAttendance = 10, ActualBudget = 100, Summary = "Summary" });
            }
        }

        var expectedMilestoneKeys = new List<string>();
        for (var i = 0; i < 40; i++)
        {
            var evt = seedContext.Events[i];
            var titles = new[] { "Chuẩn bị", "Bắt đầu", "Kết thúc" };
            for (var j = 0; j < 3; j++)
            {
                var mKey = CompositeKey(evt.Id, titles[j]);
                expectedMilestoneKeys.Add(mKey);
                if (!milestoneKeySet.Contains(mKey)) db.Milestones.Add(new Milestone { EventId = evt.Id, Title = titles[j], OrderIndex = j + 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), Status = MilestoneStatus.InProgress });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        seedContext.Members = OrderByExpected(await db.Members.Where(x => userIds.Contains(x.UserId)).ToListAsync(cancellationToken), expectedMemberKeys, x => CompositeKey(x.UserId, x.OrgId));
        seedContext.Milestones = OrderByExpected(await db.Milestones.Where(x => evtIds.Contains(x.EventId)).ToListAsync(cancellationToken), expectedMilestoneKeys, x => CompositeKey(x.EventId, x.Title));
    }

    private static async Task SeedOperationalDataAsync(AppDbContext db, SeedContext seedContext, CancellationToken cancellationToken)
    {
        var evtIds = seedContext.Events.Select(x => x.Id).ToList();
        var msIds = seedContext.Milestones.Select(x => x.Id).ToList();
        
        var catKeySet = (await db.EventCategories.Where(x => msIds.Contains(x.MilestoneId)).Select(x => new { x.MilestoneId, x.CategoryName }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.MilestoneId, x.CategoryName)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        var expectedCatKeys = new List<string>();
        for (var i = 0; i < 30; i++)
        {
            var cKey = CompositeKey(seedContext.Milestones[i].Id, "Logistics");
            expectedCatKeys.Add(cKey);
            if (!catKeySet.Contains(cKey)) db.EventCategories.Add(new EventCategory { MilestoneId = seedContext.Milestones[i].Id, CategoryName = "Logistics", OrderIndex = 1 });
        }
        await db.SaveChangesAsync(cancellationToken);
        seedContext.EventCategories = OrderByExpected(await db.EventCategories.Where(x => msIds.Contains(x.MilestoneId)).ToListAsync(cancellationToken), expectedCatKeys, x => CompositeKey(x.MilestoneId, x.CategoryName));

        var catIds = seedContext.EventCategories.Select(x => x.Id).ToList();
        var taskKeySet = (await db.Tasks.Where(x => catIds.Contains(x.EventCategoryId)).Select(x => new { x.EventCategoryId, x.TaskName }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.EventCategoryId, x.TaskName)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ememKeySet = (await db.EventMembers.Where(x => evtIds.Contains(x.EventId)).Select(x => new { x.EventId, x.MemberId }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.EventId, x.MemberId)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var attKeySet = (await db.Attendees.Where(x => evtIds.Contains(x.EventId) && x.UserId != null).Select(x => new { x.EventId, x.UserId }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.EventId, x.UserId)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var assetKeySet = (await db.DigitalAssets.Where(x => evtIds.Contains(x.EventId)).Select(x => new { x.EventId, x.FileName }).ToListAsync(cancellationToken)).Select(x => CompositeKey(x.EventId, x.FileName)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        int taskIdx = 1;
        for (var i = 0; i < 30; i++)
        {
            int count = i < 15 ? 2 : 1;
            for (var j = 0; j < count; j++)
            {
                var tName = $"Task {taskIdx++}";
                var tKey = CompositeKey(seedContext.EventCategories[i].Id, tName);
                if (!taskKeySet.Contains(tKey)) db.Tasks.Add(new OrgTask { EventCategoryId = seedContext.EventCategories[i].Id, TaskName = tName, AssigneeId = seedContext.Members[0].Id, Priority = TaskPriority.Medium, Status = TaskStatus.Todo });
            }
        }

        for (var i = 0; i < 40; i++)
        {
            int count = 0;
            if (i < 15) count = 1;
            else if (i < 30) count = 2;

            for (var j = 0; j < count; j++)
            {
                var mem = seedContext.Members[(i + j) % seedContext.Members.Count];
                var emKey = CompositeKey(seedContext.Events[i].Id, mem.Id);
                if (!ememKeySet.Contains(emKey)) db.EventMembers.Add(new EventMember { EventId = seedContext.Events[i].Id, MemberId = mem.Id, EventRole = "Coordinator", AssignedAt = DateTime.UtcNow });
            }
        }

        for (var i = 0; i < 40; i++)
        {
            int count = i < 20 ? 1 : 2;
            for (var j = 0; j < count; j++)
            {
                var usr = seedContext.Users[(i + j) % seedContext.Users.Count];
                var aKey = CompositeKey(seedContext.Events[i].Id, usr.Id);
                if (!attKeySet.Contains(aKey)) db.Attendees.Add(new Attendee { EventId = seedContext.Events[i].Id, UserId = usr.Id, GuestName = usr.FullName, Email = usr.Email, Status = AttendeeStatus.Attended });
            }
        }

        for (var i = 0; i < 30; i++)
        {
            var asKey = CompositeKey(seedContext.Events[i].Id, "asset.pdf");
            if (!assetKeySet.Contains(asKey)) db.DigitalAssets.Add(new DigitalAsset { EventId = seedContext.Events[i].Id, FileName = "asset.pdf", FileUrl = "url", FileType = FileType.Document, UploadedBy = seedContext.Members[0].Id });
        }

        // Link 1 resource per org to event
        var resources = await db.Resources.Where(x => seedContext.Organizations.Select(o => o.Id).Contains(x.OrgId)).ToListAsync(cancellationToken);
        for(var i=0; i<6; i++) {
            var org = seedContext.Organizations[i];
            var firstRes = resources.FirstOrDefault(r => r.OrgId == org.Id);
            var firstEvt = seedContext.Events.FirstOrDefault(e => e.OrgId == org.Id);
            if (firstRes != null && firstEvt != null && firstRes.EventId != firstEvt.Id) {
                firstRes.EventId = firstEvt.Id;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static List<T> OrderByExpected<T>(IEnumerable<T> source, IEnumerable<string> expectedKeys, Func<T, string> selector)
    {
        var items = source.ToList();
        var ordered = new List<T>();
        foreach (var key in expectedKeys)
        {
            var match = items.FirstOrDefault(item => string.Equals(selector(item), key, StringComparison.OrdinalIgnoreCase));
            if (match == null) throw new InvalidOperationException($"Seed reference not found for key '{key}'.");
            ordered.Add(match);
        }
        return ordered;
    }

    private static string CompositeKey(params object?[] parts) => string.Join("::", parts.Select(x => x?.ToString() ?? string.Empty));

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
