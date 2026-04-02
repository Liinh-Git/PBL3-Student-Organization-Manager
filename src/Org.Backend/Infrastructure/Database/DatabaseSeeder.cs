using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using TaskStatus = Org.Backend.Domain.Enums.TaskStatus;

namespace Org.Backend.Infrastructure.Database;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var organizations = Enumerable.Range(1, 10)
            .Select(i => new Organization
            {
                OrgName = $"Organization {i}",
                Description = $"Description for organization {i}",
                AvatarUrl = $"https://example.com/org-{i}.png",
                CoverUrl = $"https://example.com/org-cover-{i}.png",
                FoundingDate = DateTime.UtcNow.Date.AddYears(-i),
                Location = $"Campus {i}",
                TotalMembers = 10,
                Status = i % 2 == 0 ? OrgStatus.Inactive : OrgStatus.Active
            })
            .ToList();

        foreach (var org in organizations)
        {
            if (!await db.Organizations.AnyAsync(x => x.OrgName == org.OrgName, cancellationToken))
            {
                db.Organizations.Add(org);
            }
        }

        var permissions = Enumerable.Range(1, 10)
            .Select(i => new Permission
            {
                PermissionKey = $"module.permission.{i}",
                DisplayName = $"Permission {i}",
                ModuleGroup = i % 2 == 0 ? "Events" : "Members"
            })
            .ToList();

        foreach (var permission in permissions)
        {
            if (!await db.Permissions.AnyAsync(x => x.PermissionKey == permission.PermissionKey, cancellationToken))
            {
                db.Permissions.Add(permission);
            }
        }

        var users = Enumerable.Range(1, 10)
            .Select(i => new User
            {
                FullName = $"User {i}",
                Email = $"user{i}@example.com",
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
            })
            .ToList();

        foreach (var user in users)
        {
            if (!await db.Users.AnyAsync(x => x.Email == user.Email, cancellationToken))
            {
                db.Users.Add(user);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var orgList = await db.Organizations.OrderBy(x => x.OrgName).Take(10).ToListAsync(cancellationToken);
        var userList = await db.Users.OrderBy(x => x.Email).Take(10).ToListAsync(cancellationToken);
        var permissionList = await db.Permissions.OrderBy(x => x.PermissionKey).Take(10).ToListAsync(cancellationToken);

        for (var i = 0; i < 10; i++)
        {
            var roleName = $"Role {i + 1}";
            if (!await db.Roles.AnyAsync(x => x.OrgId == orgList[i].Id && x.RoleName == roleName, cancellationToken))
            {
                db.Roles.Add(new Role
                {
                    OrgId = orgList[i].Id,
                    RoleName = roleName,
                    Description = $"Role description {i + 1}",
                    IsDefault = i == 0
                });
            }

            var deptName = $"Department {i + 1}";
            if (!await db.Departments.AnyAsync(x => x.OrgId == orgList[i].Id && x.DeptName == deptName, cancellationToken))
            {
                db.Departments.Add(new Department
                {
                    OrgId = orgList[i].Id,
                    DeptName = deptName,
                    Function = $"Function {i + 1}"
                });
            }

            var eventName = $"Event {i + 1}";
            if (!await db.Events.AnyAsync(x => x.OrgId == orgList[i].Id && x.EventName == eventName, cancellationToken))
            {
                db.Events.Add(new Event
                {
                    OrgId = orgList[i].Id,
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
            if (!await db.Requests.AnyAsync(x => x.SenderId == userList[i].Id && x.Content == requestContent, cancellationToken))
            {
                db.Requests.Add(new Request
                {
                    SenderId = userList[i].Id,
                    OrgId = orgList[i].Id,
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
            if (!await db.Resources.AnyAsync(x => x.OrgId == orgList[i].Id && x.ResourceName == resourceName, cancellationToken))
            {
                db.Resources.Add(new Resource
                {
                    OrgId = orgList[i].Id,
                    ResourceName = resourceName,
                    Type = i % 2 == 0 ? "Equipment" : "Room",
                    Quantity = 5 + i,
                    Status = ResourceStatus.Available
                });
            }

            var title = $"Activity {i + 1}";
            if (!await db.ActivityHistories.AnyAsync(x => x.OrgId == orgList[i].Id && x.Title == title, cancellationToken))
            {
                db.ActivityHistories.Add(new ActivityHistory
                {
                    OrgId = orgList[i].Id,
                    Title = title,
                    Content = $"Activity content {i + 1}",
                    CoverUrl = $"https://example.com/activity-{i + 1}.png",
                    ActivityDate = DateTime.UtcNow.AddDays(-i),
                    Type = ActivityType.Other,
                    IsPublic = true
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var roleList = await db.Roles.OrderBy(x => x.RoleName).Take(10).ToListAsync(cancellationToken);
        var deptList = await db.Departments.OrderBy(x => x.DeptName).Take(10).ToListAsync(cancellationToken);
        var eventList = await db.Events.OrderBy(x => x.EventName).Take(10).ToListAsync(cancellationToken);

        for (var i = 0; i < 10; i++)
        {
            if (!await db.Members.AnyAsync(x => x.UserId == userList[i].Id && x.OrgId == orgList[i].Id, cancellationToken))
            {
                db.Members.Add(new Member
                {
                    UserId = userList[i].Id,
                    OrgId = orgList[i].Id,
                    DepartmentId = deptList[i].Id,
                    RoleId = roleList[i].Id,
                    JoinDate = DateTime.UtcNow.Date.AddDays(-30 + i)
                });
            }

            if (!await db.RolePermissions.AnyAsync(x => x.RoleId == roleList[i].Id && x.PermissionId == permissionList[i].Id, cancellationToken))
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleList[i].Id,
                    PermissionId = permissionList[i].Id
                });
            }

            if (!await db.EventReports.AnyAsync(x => x.EventId == eventList[i].Id, cancellationToken))
            {
                db.EventReports.Add(new EventReport
                {
                    EventId = eventList[i].Id,
                    ActualAttendance = 40 + i,
                    ActualBudget = 800 + i * 50,
                    RatingAverage = 4.0f,
                    Summary = $"Report summary {i + 1}"
                });
            }

            if (!await db.Milestones.AnyAsync(x => x.EventId == eventList[i].Id && x.Title == $"Milestone {i + 1}", cancellationToken))
            {
                db.Milestones.Add(new Milestone
                {
                    EventId = eventList[i].Id,
                    Title = $"Milestone {i + 1}",
                    OrderIndex = i + 1,
                    DueDate = DateTime.UtcNow.Date.AddDays(i + 7),
                    Status = MilestoneStatus.InProgress
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var memberList = await db.Members.OrderBy(x => x.JoinDate).Take(10).ToListAsync(cancellationToken);
        var milestoneList = await db.Milestones.OrderBy(x => x.OrderIndex).Take(10).ToListAsync(cancellationToken);

        for (var i = 0; i < 10; i++)
        {
            if (deptList[i].ManagerId != memberList[i].Id)
            {
                deptList[i].ManagerId = memberList[i].Id;
            }

            if (!await db.EventMembers.AnyAsync(x => x.EventId == eventList[i].Id && x.MemberId == memberList[i].Id, cancellationToken))
            {
                db.EventMembers.Add(new EventMember
                {
                    EventId = eventList[i].Id,
                    MemberId = memberList[i].Id,
                    EventRole = i % 2 == 0 ? "Logistics" : "MC",
                    AssignedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            if (!await db.Attendees.AnyAsync(x => x.EventId == eventList[i].Id && x.UserId == userList[i].Id, cancellationToken))
            {
                db.Attendees.Add(new Attendee
                {
                    EventId = eventList[i].Id,
                    UserId = userList[i].Id,
                    GuestName = userList[i].FullName,
                    Email = userList[i].Email,
                    TicketType = "Standard",
                    CheckInTime = DateTime.UtcNow.AddHours(-i),
                    Status = AttendeeStatus.Attended
                });
            }

            if (!await db.Tasks.AnyAsync(x => x.MilestoneId == milestoneList[i].Id && x.TaskName == $"Task {i + 1}", cancellationToken))
            {
                db.Tasks.Add(new OrgTask
                {
                    MilestoneId = milestoneList[i].Id,
                    TaskName = $"Task {i + 1}",
                    AssigneeId = memberList[i].Id,
                    DeptId = deptList[i].Id,
                    Priority = i % 2 == 0 ? TaskPriority.Medium : TaskPriority.High,
                    Deadline = DateTime.UtcNow.Date.AddDays(i + 5),
                    Status = i % 2 == 0 ? TaskStatus.Todo : TaskStatus.InProgress,
                    Note = $"Task note {i + 1}"
                });
            }

            if (!await db.DigitalAssets.AnyAsync(x => x.EventId == eventList[i].Id && x.FileName == $"asset-{i + 1}.pdf", cancellationToken))
            {
                db.DigitalAssets.Add(new DigitalAsset
                {
                    EventId = eventList[i].Id,
                    FileName = $"asset-{i + 1}.pdf",
                    FileUrl = $"https://example.com/assets/{i + 1}.pdf",
                    FileType = FileType.Document,
                    UploadedBy = memberList[i].Id
                });
            }

            var linkedResource = await db.Resources.FirstOrDefaultAsync(x => x.OrgId == orgList[i].Id && x.ResourceName == $"Resource {i + 1}", cancellationToken);
            if (linkedResource is not null && linkedResource.EventId != eventList[i].Id)
            {
                linkedResource.EventId = eventList[i].Id;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
