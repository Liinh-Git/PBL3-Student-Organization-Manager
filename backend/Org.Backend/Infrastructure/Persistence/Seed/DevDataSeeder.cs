using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent development data seeder.
/// Running multiple times will not duplicate data.
/// Checks existing records by stable keys.
/// </summary>
public class DevDataSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public DevDataSeeder(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Seeds development data. Idempotent - safe to run multiple times.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Seed in order of dependencies with SaveChanges between stages
        
        // Stage 1: Base entities (permissions, users, organization)
        await SeedPermissionsAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
        await SeedOrganizationAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Stage 2: Roles (depends on organization)
        await SeedRolesAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Stage 3: Role permissions, members, departments, events (depends on roles)
        await SeedRolePermissionsAsync(cancellationToken);
        await SeedMembersAsync(cancellationToken);
        await SeedDepartmentsAsync(cancellationToken);
        await SeedEventsAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Stage 4: Milestones (depends on events)
        await SeedMilestonesAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Stage 5: Event categories (depends on milestones)
        await SeedEventCategoriesAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Stage 6: Tasks, requests, notifications, friend requests (depends on categories)
        await SeedOrgTasksAsync(cancellationToken);
        await SeedRequestsAsync(cancellationToken);
        await SeedNotificationsAsync(cancellationToken);
        await SeedFriendRequestsAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Final validation
        await ValidateSeedDataAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        foreach (var permissionKey in SeedConstants.CanonicalPermissions)
        {
            var exists = await _context.Permissions
                .AnyAsync(p => p.PermissionKey == permissionKey, ct);
            
            if (!exists)
            {
                var permission = new Permission
                {
                    PermissionKey = permissionKey,
                    DisplayName = SeedConstants.PermissionDisplayNames.GetValueOrDefault(permissionKey, permissionKey),
                    ModuleGroup = SeedConstants.PermissionModuleGroups.GetValueOrDefault(permissionKey, "General"),
                    Description = $"Permission: {permissionKey}"
                };
                _context.Permissions.Add(permission);
            }
        }
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        var usersToCreate = new List<(string Email, string Password, string FullName)>
        {
            (SeedConstants.AdminEmail, SeedConstants.AdminPassword, "Admin User"),
            (SeedConstants.Member1Email, SeedConstants.Member1Password, "John Doe"),
            (SeedConstants.Member2Email, SeedConstants.Member2Password, "Jane Smith"),
            (SeedConstants.Member3Email, SeedConstants.Member3Password, "Bob Johnson"),
            (SeedConstants.Member4Email, SeedConstants.Member4Password, "Alice Williams"),
            (SeedConstants.Member5Email, SeedConstants.Member5Password, "Charlie Brown")
        };

        foreach (var (email, password, fullName) in usersToCreate)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, ct);
            
            if (existingUser == null)
            {
                var user = new User
                {
                    Email = email,
                    FullName = fullName,
                    PasswordHash = HashPassword(null!, password), // User will be set after
                    Status = UserStatus.Active,
                    EmailConfirmed = true,
                    ProfileVisibility = ProfileVisibility.Public
                };
                user.PasswordHash = _passwordHasher.HashPassword(user, password);
                _context.Users.Add(user);
            }
        }
    }

    private async Task SeedOrganizationAsync(CancellationToken ct)
    {
        var exists = await _context.Organizations
            .AnyAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (!exists)
        {
            var org = new Organization
            {
                OrgName = SeedConstants.DefaultOrgName,
                Description = "Default student organization for development and testing",
                Status = OrgStatus.Active,
                TotalMembers = 0 // Will be updated after members are added
            };
            _context.Organizations.Add(org);
        }
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        // Ensure organization exists first
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        var rolesToCreate = new List<(string Name, bool IsDefault, int? Level)>
        {
            (SeedConstants.PresidentRoleName, false, 1),
            (SeedConstants.ManagerRoleName, false, 2),
            (SeedConstants.MemberRoleName, true, 3)
        };

        foreach (var (name, isDefault, level) in rolesToCreate)
        {
            var exists = await _context.Roles
                .AnyAsync(r => r.OrgId == org.Id && r.RoleName == name, ct);
            
            if (!exists)
            {
                var role = new Role
                {
                    OrgId = org.Id,
                    RoleName = name,
                    Description = $"{name} role for {org.OrgName}",
                    IsDefault = isDefault,
                    Level = level
                };
                _context.Roles.Add(role);
            }
        }
    }

    private async Task SeedRolePermissionsAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        // President role - all permissions
        var presidentRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.OrgId == org.Id && r.RoleName == SeedConstants.PresidentRoleName, ct);
        
        if (presidentRole != null)
        {
            await AssignPermissionsToRoleAsync(presidentRole, SeedConstants.PresidentPermissions, ct);
        }

        // Manager role
        var managerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.OrgId == org.Id && r.RoleName == SeedConstants.ManagerRoleName, ct);
        
        if (managerRole != null)
        {
            await AssignPermissionsToRoleAsync(managerRole, SeedConstants.ManagerPermissions, ct);
        }

        // Member role
        var memberRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.OrgId == org.Id && r.RoleName == SeedConstants.MemberRoleName, ct);
        
        if (memberRole != null)
        {
            await AssignPermissionsToRoleAsync(memberRole, SeedConstants.MemberPermissions, ct);
        }
    }

    private async Task AssignPermissionsToRoleAsync(Role role, string[] permissionKeys, CancellationToken ct)
    {
        foreach (var permissionKey in permissionKeys)
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.PermissionKey == permissionKey, ct);
            
            if (permission != null)
            {
                var exists = await _context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, ct);
                
                if (!exists)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }
    }

    private async Task SeedMembersAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        var presidentRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.OrgId == org.Id && r.RoleName == SeedConstants.PresidentRoleName, ct);
        
        var memberRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.OrgId == org.Id && r.RoleName == SeedConstants.MemberRoleName, ct);

        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == SeedConstants.AdminEmail, ct);
        
        var member1 = await _context.Users.FirstOrDefaultAsync(u => u.Email == SeedConstants.Member1Email, ct);
        var member2 = await _context.Users.FirstOrDefaultAsync(u => u.Email == SeedConstants.Member2Email, ct);
        var member3 = await _context.Users.FirstOrDefaultAsync(u => u.Email == SeedConstants.Member3Email, ct);
        var member4 = await _context.Users.FirstOrDefaultAsync(u => u.Email == SeedConstants.Member4Email, ct);
        var member5 = await _context.Users.FirstOrDefaultAsync(u => u.Email == SeedConstants.Member5Email, ct);

        var membersToCreate = new List<(User? User, Role? Role)>
        {
            (adminUser, presidentRole),
            (member1, memberRole),
            (member2, memberRole),
            (member3, memberRole),
            (member4, memberRole),
            (member5, memberRole)
        };

        foreach (var (user, role) in membersToCreate)
        {
            if (user == null) continue;
            
            var exists = await _context.Members
                .AnyAsync(m => m.UserId == user.Id && m.OrgId == org.Id, ct);
            
            if (!exists)
            {
                var member = new Member
                {
                    UserId = user.Id,
                    OrgId = org.Id,
                    RoleId = role?.Id,
                    JoinDate = DateTime.UtcNow,
                    Status = MemberStatus.Active
                };
                _context.Members.Add(member);
            }
        }

        // Update total members count
        org.TotalMembers = membersToCreate.Count;
    }

    private async Task SeedDepartmentsAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        var departmentsToCreate = new List<(string Name, string Code, string Function)>
        {
            (SeedConstants.TechDeptName, "TECH", "Technical and development team"),
            (SeedConstants.EventsDeptName, "EVNT", "Event planning and coordination"),
            (SeedConstants.MarketingDeptName, "MKTG", "Marketing and communications")
        };

        foreach (var (name, code, function) in departmentsToCreate)
        {
            var exists = await _context.Departments
                .AnyAsync(d => d.OrgId == org.Id && d.DeptName == name, ct);
            
            if (!exists)
            {
                var department = new Department
                {
                    OrgId = org.Id,
                    DeptName = name,
                    Code = code,
                    Function = function,
                    Status = DepartmentStatus.Active
                };
                _context.Departments.Add(department);
            }
        }
    }

    private async Task SeedEventsAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        var exists = await _context.Events
            .AnyAsync(e => e.OrgId == org.Id && e.EventName == SeedConstants.DemoEventName, ct);
        
        if (!exists)
        {
            var eventEntity = new Event
            {
                OrgId = org.Id,
                EventName = SeedConstants.DemoEventName,
                Description = "Annual technology summit featuring workshops, talks, and networking opportunities",
                StartDate = DateTime.UtcNow.AddMonths(2),
                EndDate = DateTime.UtcNow.AddMonths(2).AddDays(2),
                Location = "University Main Hall",
                Status = EventStatus.Published,
                Visibility = EventVisibility.Public
            };
            _context.Events.Add(eventEntity);
        }
    }

    private async Task SeedMilestonesAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.OrgId == org.Id && e.EventName == SeedConstants.DemoEventName, ct);
        
        if (eventEntity == null) return;

        var milestonesToCreate = new List<(string Title, int OrderIndex)>
        {
            ("Planning Phase", 1),
            ("Execution Phase", 2),
            ("Wrap-up Phase", 3)
        };

        foreach (var (title, orderIndex) in milestonesToCreate)
        {
            var exists = await _context.Milestones
                .AnyAsync(m => m.EventId == eventEntity.Id && m.Title == title, ct);
            
            if (!exists)
            {
                var milestone = new Milestone
                {
                    EventId = eventEntity.Id,
                    Title = title,
                    Description = $"{title} for {eventEntity.EventName}",
                    OrderIndex = orderIndex,
                    Status = MilestoneStatus.Planned
                };
                _context.Milestones.Add(milestone);
            }
        }
    }

    private async Task SeedEventCategoriesAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.OrgId == org.Id && e.EventName == SeedConstants.DemoEventName, ct);
        
        if (eventEntity == null) return;

        var milestone = await _context.Milestones
            .FirstOrDefaultAsync(m => m.EventId == eventEntity.Id && m.Title == "Planning Phase", ct);
        
        if (milestone == null) return;

        var categoriesToCreate = new List<(string Name, int OrderIndex)>
        {
            ("Venue & Logistics", 1),
            ("Speaker Coordination", 2),
            ("Marketing & Promotion", 3)
        };

        foreach (var (name, orderIndex) in categoriesToCreate)
        {
            var exists = await _context.EventCategories
                .AnyAsync(c => c.MilestoneId == milestone.Id && c.CategoryName == name, ct);
            
            if (!exists)
            {
                var category = new EventCategory
                {
                    MilestoneId = milestone.Id,
                    CategoryName = name,
                    Description = $"{name} tasks",
                    OrderIndex = orderIndex
                };
                _context.EventCategories.Add(category);
            }
        }
    }

    private async Task SeedOrgTasksAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.OrgId == org.Id && e.EventName == SeedConstants.DemoEventName, ct);
        
        if (eventEntity == null) return;

        var category = await _context.EventCategories
            .FirstOrDefaultAsync(c => c.Milestone.EventId == eventEntity.Id && c.CategoryName == "Venue & Logistics", ct);
        
        if (category == null) return;

        var tasksToCreate = new List<(string Name, string Description, TaskPriority Priority)>
        {
            ("Book main hall", "Reserve the university main hall for the event dates", TaskPriority.High),
            ("Arrange seating", "Plan and arrange seating for 500 attendees", TaskPriority.Medium),
            ("Setup AV equipment", "Coordinate with IT for audio-visual setup", TaskPriority.High),
            ("Prepare name badges", "Design and print attendee name badges", TaskPriority.Low),
            ("Order refreshments", "Arrange catering for the event", TaskPriority.Medium)
        };

        foreach (var (name, description, priority) in tasksToCreate)
        {
            var exists = await _context.OrgTasks
                .AnyAsync(t => t.EventCategoryId == category.Id && t.TaskName == name, ct);
            
            if (!exists)
            {
                var task = new OrgTask
                {
                    EventCategoryId = category.Id,
                    TaskName = name,
                    Description = description,
                    Priority = priority,
                    Status = Domain.Enums.TaskStatus.Todo,
                    Deadline = eventEntity.StartDate.AddDays(-7)
                };
                _context.OrgTasks.Add(task);
            }
        }
    }

    private async Task SeedRequestsAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null) return;

        // Check if member5 is already a member - if so, don't create a join request
        var member5 = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == SeedConstants.Member5Email, ct);

        if (member5 != null)
        {
            var isMember = await _context.Members
                .AnyAsync(m => m.UserId == member5.Id && m.OrgId == org.Id, ct);
            
            // Only create join request if member5 is not already a member
            if (!isMember)
            {
                var exists = await _context.Requests
                    .AnyAsync(r => r.SenderId == member5.Id && r.OrgId == org.Id && r.RequestType == RequestType.JoinOrganization, ct);
                
                if (!exists)
                {
                    var request = new Request
                    {
                        SenderId = member5.Id,
                        OrgId = org.Id,
                        RequestType = RequestType.JoinOrganization,
                        Title = "Request to join organization",
                        Content = "I would like to become a member of this organization.",
                        Status = RequestStatus.Pending
                    };
                    _context.Requests.Add(request);
                }
            }
        }
    }

    private async Task SeedNotificationsAsync(CancellationToken ct)
    {
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == SeedConstants.AdminEmail, ct);
        
        if (adminUser == null) return;

        var notificationsToCreate = new List<(string Title, string Message, NotificationType Type)>
        {
            ("Welcome!", "Welcome to the Student Organization Management System.", NotificationType.System),
            ("New Event Created", "A new event 'Annual Tech Summit 2026' has been created.", NotificationType.EventCreated),
            ("Task Assigned", "You have been assigned to 'Book main hall' task.", NotificationType.TaskAssigned)
        };

        foreach (var (title, message, type) in notificationsToCreate)
        {
            var exists = await _context.Notifications
                .AnyAsync(n => n.ReceiverId == adminUser.Id && n.Title == title, ct);
            
            if (!exists)
            {
                var notification = new Notification
                {
                    ReceiverId = adminUser.Id,
                    Title = title,
                    Message = message,
                    Type = type,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }
        }
    }

    private async Task SeedFriendRequestsAsync(CancellationToken ct)
    {
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == SeedConstants.AdminEmail, ct);
        
        var member1 = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == SeedConstants.Member1Email, ct);
        
        var member2 = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == SeedConstants.Member2Email, ct);

        // Admin sends friend request to member1
        if (adminUser != null && member1 != null)
        {
            var exists = await _context.FriendRequests
                .AnyAsync(f => f.SenderId == adminUser.Id && f.ReceiverId == member1.Id, ct);
            
            if (!exists)
            {
                var friendRequest = new FriendRequest
                {
                    SenderId = adminUser.Id,
                    ReceiverId = member1.Id,
                    Status = FriendRequestStatus.Pending
                };
                _context.FriendRequests.Add(friendRequest);
            }
        }

        // Member2 sends friend request to admin (already accepted)
        if (member2 != null && adminUser != null)
        {
            var exists = await _context.FriendRequests
                .AnyAsync(f => f.SenderId == member2.Id && f.ReceiverId == adminUser.Id, ct);
            
            if (!exists)
            {
                var friendRequest = new FriendRequest
                {
                    SenderId = member2.Id,
                    ReceiverId = adminUser.Id,
                    Status = FriendRequestStatus.Accepted,
                    RespondedAt = DateTime.UtcNow.AddDays(-1)
                };
                _context.FriendRequests.Add(friendRequest);
            }
        }
    }

    private string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    /// <summary>
    /// Validates that all seed data was created successfully.
    /// Throws InvalidOperationException if validation fails.
    /// </summary>
    private async Task ValidateSeedDataAsync(CancellationToken ct)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrgName == SeedConstants.DefaultOrgName, ct);
        
        if (org == null)
            throw new InvalidOperationException("Organization not found after seeding");

        // Count entities
        var rolesCount = await _context.Roles.CountAsync(r => r.OrgId == org.Id, ct);
        var rolePermissionsCount = await _context.RolePermissions
            .CountAsync(rp => _context.Roles.Any(r => r.Id == rp.RoleId && r.OrgId == org.Id), ct);
        var membersCount = await _context.Members.CountAsync(m => m.OrgId == org.Id, ct);
        var departmentsCount = await _context.Departments.CountAsync(d => d.OrgId == org.Id, ct);
        var eventsCount = await _context.Events.CountAsync(e => e.OrgId == org.Id, ct);
        var milestonesCount = await _context.Milestones
            .CountAsync(m => _context.Events.Any(e => e.Id == m.EventId && e.OrgId == org.Id), ct);
        var eventCategoriesCount = await _context.EventCategories
            .CountAsync(ec => _context.Milestones.Any(m => m.Id == ec.MilestoneId && 
                _context.Events.Any(e => e.Id == m.EventId && e.OrgId == org.Id)), ct);
        var orgTasksCount = await _context.OrgTasks
            .CountAsync(t => _context.EventCategories.Any(ec => ec.Id == t.EventCategoryId && 
                _context.Milestones.Any(m => m.Id == ec.MilestoneId && 
                    _context.Events.Any(e => e.Id == m.EventId && e.OrgId == org.Id))), ct);

        // Validate counts
        if (rolesCount < 3)
            throw new InvalidOperationException($"Expected at least 3 roles, found {rolesCount}");
        
        if (rolePermissionsCount < 28)
            throw new InvalidOperationException($"Expected at least 28 role permissions, found {rolePermissionsCount}");
        
        if (membersCount < 6)
            throw new InvalidOperationException($"Expected at least 6 members, found {membersCount}");
        
        if (departmentsCount < 3)
            throw new InvalidOperationException($"Expected at least 3 departments, found {departmentsCount}");
        
        if (eventsCount < 1)
            throw new InvalidOperationException($"Expected at least 1 event, found {eventsCount}");
        
        if (milestonesCount < 3)
            throw new InvalidOperationException($"Expected at least 3 milestones, found {milestonesCount}");
        
        if (eventCategoriesCount < 3)
            throw new InvalidOperationException($"Expected at least 3 event categories, found {eventCategoriesCount}");
        
        if (orgTasksCount < 5)
            throw new InvalidOperationException($"Expected at least 5 org tasks, found {orgTasksCount}");

        // Validate admin is President
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == SeedConstants.AdminEmail, ct);
        
        if (adminUser == null)
            throw new InvalidOperationException("Admin user not found");

        var adminMember = await _context.Members
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.UserId == adminUser.Id && m.OrgId == org.Id, ct);
        
        if (adminMember?.Role?.RoleName != SeedConstants.PresidentRoleName)
            throw new InvalidOperationException($"Admin user is not President. Current role: {adminMember?.Role?.RoleName ?? "null"}");

        // Validate no member has null RoleId
        var membersWithNullRole = await _context.Members
            .CountAsync(m => m.OrgId == org.Id && m.RoleId == null, ct);
        
        if (membersWithNullRole > 0)
            throw new InvalidOperationException($"Found {membersWithNullRole} members with null RoleId");

        // Validate role permission counts
        var presidentRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.OrgId == org.Id && r.RoleName == SeedConstants.PresidentRoleName, ct);
        var managerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.OrgId == org.Id && r.RoleName == SeedConstants.ManagerRoleName, ct);
        var memberRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.OrgId == org.Id && r.RoleName == SeedConstants.MemberRoleName, ct);

        if (presidentRole != null)
        {
            var presidentPermCount = await _context.RolePermissions.CountAsync(rp => rp.RoleId == presidentRole.Id, ct);
            if (presidentPermCount != 15)
                throw new InvalidOperationException($"President role should have 15 permissions, found {presidentPermCount}");
        }

        if (managerRole != null)
        {
            var managerPermCount = await _context.RolePermissions.CountAsync(rp => rp.RoleId == managerRole.Id, ct);
            if (managerPermCount != 9)
                throw new InvalidOperationException($"Manager role should have 9 permissions, found {managerPermCount}");
        }

        if (memberRole != null)
        {
            var memberPermCount = await _context.RolePermissions.CountAsync(rp => rp.RoleId == memberRole.Id, ct);
            if (memberPermCount != 4)
                throw new InvalidOperationException($"Member role should have 4 permissions, found {memberPermCount}");
        }
    }
}
