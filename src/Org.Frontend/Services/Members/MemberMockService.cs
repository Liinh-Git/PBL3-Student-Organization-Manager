// ---- Mock service thành viên — CRUD trên in-memory MockDataStore ----
// Tự tạo MockUser nếu email chưa có khi thêm thành viên mới.
// Xóa thành viên: giải phóng các quan hệ FK (department, category, task, eventMember).
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Shared.Contracts;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using FeatureCreateMemberRequest = Org.Shared.Features.Members.CreateMemberRequest;

namespace Org.Frontend.Services.Members;

public sealed class MemberMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IMemberService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<List<MemberDto>> GetMembers(Guid orgId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureCanReadOrganization(data, currentUserId, orgId);

            return data.Members
                .Where(x => x.OrgId == orgId)
                .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(member => MapDto(member, data))
                .ToList();
        });
    }

    public async Task<MemberDto> CreateMember(Guid orgId, FeatureCreateMemberRequest req)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureOrganizationExists(data, orgId);
            EnsureCanManageMembers(data, currentUserId, orgId);

            if (req.DepartmentId.HasValue)
            {
                EnsureDepartmentBelongsToOrganization(data, req.DepartmentId.Value, orgId);
            }

            var existingUser = data.Users.FirstOrDefault(x =>
                string.Equals(x.Email, req.Email, StringComparison.OrdinalIgnoreCase));

            var user = existingUser ?? new MockUser
            {
                Id = Guid.NewGuid(),
                FullName = NormalizeName(req.FullName),
                Email = NormalizeEmail(req.Email)
            };

            if (existingUser is null)
            {
                data.Users.Add(user);
            }

            var alreadyJoined = data.Members.Any(x => x.OrgId == orgId && x.UserId == user.Id);
            if (alreadyJoined)
            {
                throw new InvalidOperationException("User is already a member of this organization in mock data.");
            }

            var dto = new MockMember
            {
                Id = Guid.NewGuid(),
                OrgId = orgId,
                UserId = user.Id,
                DisplayName = NormalizeName(req.FullName),
                DepartmentId = req.DepartmentId,
                RoleId = null,
                JoinDate = DateTime.UtcNow
            };

            data.Members.Add(dto);
            UpdateOrganizationMemberCount(data, orgId);
            return MapDto(dto, data);
        });
    }

    public async Task AssignRole(Guid memberId, Guid roleId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");
            EnsureCanManageMembers(data, currentUserId, current.OrgId);

            var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == roleId && x.OrgId == current.OrgId)
                ?? throw new InvalidOperationException("Role does not belong to the member organization.");

            current.RoleId = role.Id;
            return 0;
        });
    }

    public async Task AssignDepartment(Guid memberId, Guid departmentId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");
            EnsureCanManageMembers(data, currentUserId, current.OrgId);

            EnsureDepartmentBelongsToOrganization(data, departmentId, current.OrgId);
            current.DepartmentId = departmentId;
            return 0;
        });
    }

    public async Task DeleteMember(Guid memberId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");
            EnsureCanManageMembers(data, currentUserId, current.OrgId);

            var actorMember = ResolveMemberByUserId(data, current.OrgId, currentUserId);
            if (actorMember is not null && actorMember.Id == memberId)
            {
                throw new InvalidOperationException("Use leave organization flow instead of admin delete for self-removal.");
            }

            EnsureNotRemovingLastPresident(data, current);

            foreach (var department in data.Departments.Where(x => x.ManagerId == memberId))
            {
                department.ManagerId = null;
            }

            foreach (var category in data.EventCategories.Where(x => x.LeadMemberId == memberId))
            {
                category.LeadMemberId = null;
            }

            foreach (var task in data.Tasks.Where(x => x.AssigneeMemberId == memberId))
            {
                task.AssigneeMemberId = null;
            }

            data.EventMembers.RemoveAll(x => x.MemberId == memberId);
            data.Members.Remove(current);
            UpdateOrganizationMemberCount(data, current.OrgId);
            return 0;
        });
    }

    public async Task<bool> CanManageOrganizationMembersAsync(Guid orgId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
            HasAnyPermission(data, currentUserId, orgId, "org.members.manage"));
    }

    public async Task LeaveOrganizationAsync(Guid orgId)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var current = ResolveMemberByUserId(data, orgId, currentUserId)
                ?? throw new InvalidOperationException("Current user is not a member of this organization.");

            EnsureNotRemovingLastPresident(data, current);

            foreach (var department in data.Departments.Where(x => x.ManagerId == current.Id))
            {
                department.ManagerId = null;
            }

            foreach (var category in data.EventCategories.Where(x => x.LeadMemberId == current.Id))
            {
                category.LeadMemberId = null;
            }

            foreach (var task in data.Tasks.Where(x => x.AssigneeMemberId == current.Id))
            {
                task.AssigneeMemberId = null;
            }

            data.EventMembers.RemoveAll(x => x.MemberId == current.Id);
            data.Members.Remove(current);
            UpdateOrganizationMemberCount(data, orgId);
            return 0;
        });
    }

    private static MemberDto MapDto(MockMember source, MockDataSet data)
    {
        var email = data.Users.FirstOrDefault(x => x.Id == source.UserId)?.Email ?? string.Empty;

        return new MemberDto
        {
            Id = source.Id,
            OrgId = source.OrgId,
            UserId = source.UserId,
            DisplayName = source.DisplayName,
            Email = email,
            DepartmentId = source.DepartmentId,
            RoleId = source.RoleId,
            RoleName = data.OrganizationRoles.FirstOrDefault(x => x.Id == source.RoleId)?.RoleName,
            JoinDate = source.JoinDate
        };
    }

    private static void EnsureOrganizationExists(MockDataSet data, Guid organizationId)
    {
        if (!data.Organizations.Any(x => x.Id == organizationId))
        {
            throw new KeyNotFoundException($"Organization {organizationId} not found in mock data.");
        }
    }

    private static void EnsureDepartmentBelongsToOrganization(MockDataSet data, Guid departmentId, Guid organizationId)
    {
        var department = data.Departments.FirstOrDefault(x => x.Id == departmentId)
            ?? throw new KeyNotFoundException($"Department {departmentId} not found in mock data.");

        if (department.OrgId != organizationId)
        {
            throw new InvalidOperationException("Department must belong to the same organization as the member.");
        }
    }

    private static string NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Member full name is required.");
        }

        return value.Trim();
    }

    private static string NormalizeEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Member email is required.");
        }

        var email = value.Trim();
        if (!email.Contains('@'))
        {
            throw new InvalidOperationException("Member email is invalid.");
        }

        return email;
    }

    private static void EnsureCanReadOrganization(MockDataSet data, Guid? userId, Guid organizationId)
    {
        if (!HasAnyPermission(data, userId, organizationId, "org.workspace.access", "org.overview.read", "org.members.manage"))
        {
            throw new UnauthorizedAccessException("You do not have permission to read member data.");
        }
    }

    private static void EnsureCanManageMembers(MockDataSet data, Guid? userId, Guid organizationId)
    {
        if (!HasAnyPermission(data, userId, organizationId, "org.members.manage"))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage organization members.");
        }
    }

    private static bool HasAnyPermission(MockDataSet data, Guid? userId, Guid organizationId, params string[] expectedPermissions)
    {
        var currentMember = ResolveMemberByUserId(data, organizationId, userId);
        if (currentMember?.RoleId is null)
        {
            return false;
        }

        var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == currentMember.RoleId.Value && x.OrgId == organizationId);
        if (role is null)
        {
            return false;
        }

        var permissions = role.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return expectedPermissions.Any(permissions.Contains);
    }

    private static MockMember? ResolveMemberByUserId(MockDataSet data, Guid orgId, Guid? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        return data.Members.FirstOrDefault(x => x.OrgId == orgId && x.UserId == userId.Value);
    }

    private static void EnsureNotRemovingLastPresident(MockDataSet data, MockMember targetMember)
    {
        if (!targetMember.RoleId.HasValue)
        {
            return;
        }

        var targetRole = data.OrganizationRoles.FirstOrDefault(x => x.Id == targetMember.RoleId.Value);
        if (!string.Equals(targetRole?.RoleName, "President", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var presidentCount = data.Members
            .Where(x => x.OrgId == targetMember.OrgId && x.RoleId.HasValue)
            .Count(x =>
            {
                var role = data.OrganizationRoles.FirstOrDefault(r => r.Id == x.RoleId!.Value);
                return string.Equals(role?.RoleName, "President", StringComparison.OrdinalIgnoreCase);
            });

        if (presidentCount <= 1)
        {
            throw new InvalidOperationException("Cannot remove or leave as the last organization president.");
        }
    }

    private static void UpdateOrganizationMemberCount(MockDataSet data, Guid orgId)
    {
        var organization = data.Organizations.FirstOrDefault(x => x.Id == orgId);
        if (organization is not null)
        {
            organization.TotalMembers = data.Members.Count(x => x.OrgId == orgId);
            organization.LastActivityAtUtc = DateTime.UtcNow;
        }
    }

    private async Task<Guid?> TryGetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }
}
