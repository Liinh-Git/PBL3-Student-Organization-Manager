namespace Org.Backend.Infrastructure.Persistence.Seed;

/// <summary>
/// Constants for development seeding.
/// These values are used to create consistent demo data.
/// </summary>
public static class SeedConstants
{
    // Default dev credentials
    public const string AdminEmail = "admin@example.com";
    public const string AdminPassword = "Admin@123456";
    
    public const string Member1Email = "member1@example.com";
    public const string Member1Password = "User@123456";
    
    public const string Member2Email = "member2@example.com";
    public const string Member2Password = "User@123456";
    
    public const string Member3Email = "member3@example.com";
    public const string Member3Password = "User@123456";
    
    public const string Member4Email = "member4@example.com";
    public const string Member4Password = "User@123456";
    
    public const string Member5Email = "member5@example.com";
    public const string Member5Password = "User@123456";
    
    // Default organization
    public const string DefaultOrgName = "Student Organization";
    
    // Role names
    public const string PresidentRoleName = "President";
    public const string ManagerRoleName = "Manager";
    public const string MemberRoleName = "Member";
    
    // Department names
    public const string TechDeptName = "Technology";
    public const string EventsDeptName = "Events";
    public const string MarketingDeptName = "Marketing";
    
    // Demo event
    public const string DemoEventName = "Annual Tech Summit 2026";
    
    /// <summary>
    /// Canonical permission keys as specified in DOMAIN_ENTITY_LOCK_V1.md
    /// </summary>
    public static readonly string[] CanonicalPermissions = new[]
    {
        "org.overview.read",
        "org.overview.write",
        "org.delete",
        "org.workspace.access",
        "org.members.manage",
        "org.roles.view",
        "org.roles.create",
        "org.roles.update",
        "org.roles.delete",
        "org.roles.assign",
        "org.events.create",
        "org.events.manage",
        "org.departments.manage",
        "org.requests.view",
        "org.requests.review",
        "org.requests.approve"
    };
    
    /// <summary>
    /// Permission display names mapped to keys
    /// </summary>
    public static readonly Dictionary<string, string> PermissionDisplayNames = new()
    {
        { "org.overview.read", "View Organization Overview" },
        { "org.overview.write", "Edit Organization Overview" },
        { "org.delete", "Delete Organization" },
        { "org.workspace.access", "Access Organization Workspace" },
        { "org.members.manage", "Manage Members" },
        { "org.roles.view", "View Roles" },
        { "org.roles.create", "Create Roles" },
        { "org.roles.update", "Update Roles" },
        { "org.roles.delete", "Delete Roles" },
        { "org.roles.assign", "Assign Roles" },
        { "org.events.create", "Create Events" },
        { "org.events.manage", "Manage Events" },
        { "org.departments.manage", "Manage Departments" },
        { "org.requests.view", "View Requests" },
        { "org.requests.review", "Review Requests" },
        { "org.requests.approve", "Approve Requests" }
    };
    
    /// <summary>
    /// Permission module groups
    /// </summary>
    public static readonly Dictionary<string, string> PermissionModuleGroups = new()
    {
        { "org.overview.read", "Overview" },
        { "org.overview.write", "Overview" },
        { "org.delete", "Overview" },
        { "org.workspace.access", "Workspace" },
        { "org.members.manage", "Members" },
        { "org.roles.view", "Roles" },
        { "org.roles.create", "Roles" },
        { "org.roles.update", "Roles" },
        { "org.roles.delete", "Roles" },
        { "org.roles.assign", "Roles" },
        { "org.events.create", "Events" },
        { "org.events.manage", "Events" },
        { "org.departments.manage", "Departments" },
        { "org.requests.view", "Requests" },
        { "org.requests.review", "Requests" },
        { "org.requests.approve", "Requests" }
    };
    
    /// <summary>
    /// President/Admin permissions (all permissions)
    /// </summary>
    public static readonly string[] PresidentPermissions = CanonicalPermissions;
    
    /// <summary>
    /// Manager permissions (reasonable management permissions)
    /// </summary>
    public static readonly string[] ManagerPermissions = new[]
    {
        "org.overview.read",
        "org.workspace.access",
        "org.members.manage",
        "org.roles.view",
        "org.events.create",
        "org.events.manage",
        "org.departments.manage",
        "org.requests.view",
        "org.requests.review"
    };
    
    /// <summary>
    /// Member permissions (workspace/read permissions)
    /// </summary>
    public static readonly string[] MemberPermissions = new[]
    {
        "org.overview.read",
        "org.workspace.access",
        "org.roles.view",
        "org.requests.view"
    };
}
