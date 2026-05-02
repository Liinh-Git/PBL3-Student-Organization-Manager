using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;

namespace Org.Frontend.Services.Organizations;

public sealed class OrganizationMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IOrganizationService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<OrganizationOverviewViewModel> GetOrganizationOverviewAsync(Guid organizationId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();

        return await _mockDataStore.UseAsync(data =>
        {
            var organization = data.Organizations.FirstOrDefault(x => x.Id == organizationId)
                ?? throw new KeyNotFoundException($"Organization {organizationId} not found.");

            var permission = ResolvePermission(currentUserId, organizationId, data);
            var eventIds = data.Events
                .Where(x => x.OrgId == organizationId)
                .Select(x => x.Id)
                .ToHashSet();
            var milestoneIds = data.Milestones
                .Where(x => eventIds.Contains(x.EventId))
                .Select(x => x.Id)
                .ToHashSet();
            var categoryIds = data.EventCategories
                .Where(x => milestoneIds.Contains(x.MilestoneId))
                .Select(x => x.Id)
                .ToHashSet();

            var today = DateOnly.FromDateTime(DateTime.Today);

            var timeline = data.Milestones
                .Where(x => eventIds.Contains(x.EventId))
                .OrderByDescending(x => x.StartDate)
                .Take(8)
                .Select(x =>
                {
                    var eventItem = data.Events.FirstOrDefault(e => e.Id == x.EventId);
                    return new OrganizationTimelineItemViewModel
                    {
                        PeriodLabel = $"{x.StartDate:MM/yyyy}",
                        Title = x.Name,
                        Description = eventItem is null ? "Milestone update" : $"{eventItem.Name} - {x.Name}"
                    };
                })
                .ToList();

            var highlights = data.Events
                .Where(x => x.OrgId == organizationId && x.StartDate >= today)
                .OrderBy(x => x.StartDate)
                .Take(6)
                .Select(x => new OrganizationEventHighlightViewModel
                {
                    EventId = x.Id,
                    Name = x.Name,
                    StartDate = x.StartDate,
                    Location = x.Location,
                    ImageUrl = ResolveEventImage(x, organization)
                })
                .ToList();

            var leaders = data.Members
                .Where(x => x.OrgId == organizationId)
                .Where(x => IsLeadershipRole(ResolveRoleName(x.RoleId, data)))
                .OrderBy(x => ResolveRoleRank(ResolveRoleName(x.RoleId, data)))
                .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .Select(x =>
                {
                    var user = data.Users.FirstOrDefault(u => u.Id == x.UserId);
                    return new OrganizationAdminViewModel
                    {
                        UserId = x.UserId,
                        Name = x.DisplayName,
                        Role = ResolveRoleName(x.RoleId, data),
                        Avatar = user?.AvatarUrl ?? "/images/mockimages/AvtUser/Avt1.jpg"
                    };
                })
                .ToList();

            var departments = data.Departments
                .Where(x => x.OrgId == organizationId)
                .OrderBy(x => x.DeptName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new OrganizationDepartmentSummaryViewModel
                {
                    DepartmentId = x.Id,
                    Name = x.DeptName,
                    MemberCount = data.Members.Count(m => m.OrgId == organizationId && m.DepartmentId == x.Id)
                })
                .ToList();

            return new OrganizationOverviewViewModel
            {
                OrganizationId = organization.Id,
                Name = organization.OrgName,
                ShortDescription = organization.ShortDescription,
                Description = organization.Description,
                Mission = organization.Mission,
                Vision = organization.Vision,
                Tags = organization.Tags,
                AvatarUrl = organization.AvatarUrl ?? "/images/mockimages/Org1/Avt.jpg",
                CoverUrl = organization.CoverUrl ?? "/images/mockimages/Org1/Bia.jpg",
                Location = organization.Location,
                ContactEmail = organization.ContactEmail,
                ContactPhone = organization.ContactPhone,
                WebsiteUrl = organization.WebsiteUrl,
                FacebookUrl = organization.FacebookUrl,
                FoundedDate = organization.FoundedDate,
                MemberCount = data.Members.Count(x => x.OrgId == organizationId),
                EventCount = eventIds.Count,
                UpcomingEventCount = data.Events.Count(x => x.OrgId == organizationId && x.StartDate >= today),
                ActiveTaskCount = data.Tasks.Count(x => categoryIds.Contains(x.CategoryId) && !string.Equals(x.Status, "DONE", StringComparison.OrdinalIgnoreCase)),
                MilestoneCount = milestoneIds.Count,
                LastActivityAtUtc = organization.LastActivityAtUtc,
                ViewerPermission = permission,
                Departments = departments,
                Timeline = timeline,
                HighlightEvents = highlights,
                Leadership = leaders
            };
        }, ct);
    }

    public async Task<OrganizationOverviewViewModel> UpdateOrganizationOverviewAsync(
        Guid organizationId,
        UpdateOrganizationOverviewRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var currentUserId = await TryGetCurrentUserIdAsync();

        await _mockDataStore.UseAsync(data =>
        {
            var permission = ResolvePermission(currentUserId, organizationId, data);
            if (!permission.CanEditOverview)
                throw new UnauthorizedAccessException("You do not have permission to edit this organization overview.");

            var organization = data.Organizations.FirstOrDefault(x => x.Id == organizationId)
                ?? throw new KeyNotFoundException($"Organization {organizationId} not found.");

            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 2)
                throw new InvalidOperationException("Organization name must be at least 2 characters.");

            organization.OrgName = request.Name.Trim();
            organization.ShortDescription = NormalizeOptional(request.ShortDescription);
            organization.Description = NormalizeOptional(request.Description);
            organization.Mission = NormalizeOptional(request.Mission);
            organization.Vision = NormalizeOptional(request.Vision);
            organization.Tags = request.Tags
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();
            organization.Location = NormalizeOptional(request.Location);
            organization.ContactEmail = NormalizeOptional(request.ContactEmail);
            organization.ContactPhone = NormalizeOptional(request.ContactPhone);
            organization.WebsiteUrl = NormalizeOptional(request.WebsiteUrl);
            organization.FacebookUrl = NormalizeOptional(request.FacebookUrl);
            organization.AvatarUrl = NormalizeOptional(request.AvatarUrl) ?? organization.AvatarUrl;
            organization.CoverUrl = NormalizeOptional(request.CoverUrl) ?? organization.CoverUrl;
            organization.LastActivityAtUtc = DateTime.UtcNow;

            return 0;
        }, ct);

        return await GetOrganizationOverviewAsync(organizationId, ct);
    }

    public async Task<MyOrganizationsViewModel> GetMyOrganizationsAsync(CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            return new MyOrganizationsViewModel();

        return await _mockDataStore.UseAsync(data =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var cards = data.Members
                .Where(x => x.UserId == currentUserId.Value)
                .Select(member =>
                {
                    var organization = data.Organizations.FirstOrDefault(o => o.Id == member.OrgId);
                    if (organization is null)
                        return null;

                    var roleName = ResolveRoleName(member.RoleId, data);
                    var eventIds = data.Events
                        .Where(e => e.OrgId == organization.Id)
                        .Select(e => e.Id)
                        .ToHashSet();
                    var milestoneIds = data.Milestones
                        .Where(m => eventIds.Contains(m.EventId))
                        .Select(m => m.Id)
                        .ToHashSet();
                    var categoryIds = data.EventCategories
                        .Where(c => milestoneIds.Contains(c.MilestoneId))
                        .Select(c => c.Id)
                        .ToHashSet();

                    return new MyOrganizationCardViewModel
                    {
                        OrganizationId = organization.Id,
                        Name = organization.OrgName,
                        ShortDescription = organization.ShortDescription ?? organization.Description,
                        AvatarUrl = organization.AvatarUrl,
                        MembershipRole = roleName,
                        MembershipStatus = "ACTIVE",
                        MemberCount = data.Members.Count(x => x.OrgId == organization.Id),
                        UpcomingEventCount = data.Events.Count(x => x.OrgId == organization.Id && x.StartDate >= today),
                        ActiveTaskCount = data.Tasks.Count(x =>
                            categoryIds.Contains(x.CategoryId)
                            && !string.Equals(x.Status, "DONE", StringComparison.OrdinalIgnoreCase)),
                        JoinedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(member.JoinDate, DateTimeKind.Utc)),
                        LastActivityAtUtc = organization.LastActivityAtUtc,
                        Tags = organization.Tags,
                        CanAccessWorkspace = true,
                        CanManage = HasOverviewWritePermission(roleName)
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .OrderByDescending(x => x.LastActivityAtUtc ?? DateTime.MinValue)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new MyOrganizationsViewModel
            {
                LeadingOrganizations = cards.Where(x => x.CanManage).ToList(),
                ParticipatingOrganizations = cards.Where(x => !x.CanManage).ToList()
            };
        }, ct);
    }

    public async Task<OrganizationViewerPermissionViewModel> GetOrganizationViewerPermissionAsync(Guid organizationId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data => ResolvePermission(currentUserId, organizationId, data), ct);
    }

    public async Task<OrganizationDetailViewModel> CreateOrganizationAsync(CreateOrganizationViewModel model, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException("User not logged in.");

        return await _mockDataStore.UseAsync(data =>
        {
            if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Trim().Length < 2)
                throw new InvalidOperationException("Organization name must be at least 2 characters.");

            var normalizedName = model.Name.Trim();
            var org = new MockOrganization
            {
                Id = Guid.NewGuid(),
                Code = BuildOrganizationCode(normalizedName, data),
                OrgName = normalizedName,
                ShortDescription = "Organization created by user workspace.",
                Description = NormalizeOptional(model.Description),
                Mission = null,
                Vision = null,
                Tags = [],
                AvatarUrl = NormalizeOptional(model.AvatarUrl) ?? "/images/mockimages/Org1/Avt.jpg",
                CoverUrl = NormalizeOptional(model.CoverUrl) ?? "/images/mockimages/Org1/Bia.jpg",
                Location = NormalizeOptional(model.Location),
                ContactEmail = null,
                ContactPhone = null,
                WebsiteUrl = null,
                FacebookUrl = null,
                FoundedDate = DateOnly.FromDateTime(DateTime.Today),
                LastActivityAtUtc = DateTime.UtcNow,
                TotalMembers = 1,
                Status = 0
            };
            data.Organizations.Add(org);

            var presidentRole = new MockOrganizationRole
            {
                Id = Guid.NewGuid(),
                OrgId = org.Id,
                RoleName = "President",
                Permissions = ["org.overview.read", "org.overview.write", "org.workspace.access", "org.members.manage", "org.events.manage", "org.tasks.manage"]
            };
            var memberRole = new MockOrganizationRole
            {
                Id = Guid.NewGuid(),
                OrgId = org.Id,
                RoleName = "Member",
                Permissions = ["org.overview.read", "org.workspace.access"]
            };
            data.OrganizationRoles.Add(presidentRole);
            data.OrganizationRoles.Add(memberRole);

            var user = data.Users.FirstOrDefault(u => u.Id == currentUserId.Value);
            data.Members.Add(new MockMember
            {
                Id = Guid.NewGuid(),
                OrgId = org.Id,
                UserId = currentUserId.Value,
                DisplayName = user?.FullName ?? "Owner",
                RoleId = presidentRole.Id,
                JoinDate = DateTime.UtcNow
            });

            return new OrganizationDetailViewModel
            {
                Id = org.Id,
                Name = org.OrgName,
                Description = org.Description ?? string.Empty,
                AvatarUrl = org.AvatarUrl,
                CoverUrl = org.CoverUrl,
                Location = org.Location,
                TotalMembers = 1,
                IsActive = true,
                FoundedDate = DateTime.Today
            };
        }, ct);
    }

    private async Task<Guid?> TryGetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }

    private static OrganizationViewerPermissionViewModel ResolvePermission(Guid? currentUserId, Guid organizationId, MockDataSet data)
    {
        if (!currentUserId.HasValue)
        {
            return new OrganizationViewerPermissionViewModel
            {
                IsAuthenticated = false,
                IsMember = false,
                CanAccessWorkspace = false,
                CanEditOverview = false,
                ViewerMode = "EXTERNAL",
                MemberRole = null
            };
        }

        var membership = data.Members.FirstOrDefault(x => x.OrgId == organizationId && x.UserId == currentUserId.Value);
        if (membership is null)
        {
            return new OrganizationViewerPermissionViewModel
            {
                IsAuthenticated = true,
                IsMember = false,
                CanAccessWorkspace = false,
                CanEditOverview = false,
                ViewerMode = "EXTERNAL",
                MemberRole = null
            };
        }

        var role = ResolveRoleName(membership.RoleId, data);
        var canEdit = HasOverviewWritePermission(role);

        return new OrganizationViewerPermissionViewModel
        {
            IsAuthenticated = true,
            IsMember = true,
            CanAccessWorkspace = true,
            CanEditOverview = canEdit,
            ViewerMode = canEdit ? "INTERNAL_WRITE" : "INTERNAL_READ",
            MemberRole = role
        };
    }

    private static bool HasOverviewWritePermission(string? roleName)
    {
        return roleName?.Trim().ToUpperInvariant() switch
        {
            "PRESIDENT" => true,
            "VICEPRESIDENT" => true,
            "MANAGER" => true,
            _ => false
        };
    }

    private static bool IsLeadershipRole(string? roleName)
    {
        return roleName?.Trim().ToUpperInvariant() switch
        {
            "PRESIDENT" => true,
            "VICEPRESIDENT" => true,
            "MANAGER" => true,
            _ => false
        };
    }

    private static int ResolveRoleRank(string? roleName)
    {
        return roleName?.Trim().ToUpperInvariant() switch
        {
            "PRESIDENT" => 0,
            "VICEPRESIDENT" => 1,
            "MANAGER" => 2,
            _ => 9
        };
    }

    private static string ResolveRoleName(Guid? roleId, MockDataSet data)
    {
        if (!roleId.HasValue)
            return "Member";

        var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == roleId.Value);
        return string.IsNullOrWhiteSpace(role?.RoleName) ? "Member" : role.RoleName;
    }

    private static string ResolveEventImage(MockEvent eventItem, MockOrganization organization)
    {
        if (!string.IsNullOrWhiteSpace(eventItem.ImageUrl))
            return eventItem.ImageUrl;
        if (!string.IsNullOrWhiteSpace(organization.CoverUrl))
            return organization.CoverUrl;
        if (!string.IsNullOrWhiteSpace(organization.AvatarUrl))
            return organization.AvatarUrl;
        return "/images/mockimages/Org1/Card1.jpg";
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildOrganizationCode(string name, MockDataSet data)
    {
        var letters = new string(name
            .Where(char.IsLetterOrDigit)
            .Take(8)
            .ToArray())
            .ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(letters))
            letters = "ORG";

        var candidate = letters;
        var suffix = 1;
        var existingCodes = data.Organizations
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        while (existingCodes.Contains(candidate))
        {
            candidate = $"{letters}{suffix:00}";
            suffix++;
        }

        return candidate;
    }
}
