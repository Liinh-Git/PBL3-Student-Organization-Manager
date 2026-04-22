// ---- Mock service thành viên — CRUD trên in-memory MockDataStore ----
// Tự tạo MockUser nếu email chưa có khi thêm thành viên mới.
// Xóa thành viên: giải phóng các quan hệ FK (department, category, task, eventMember).
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Shared.Contracts;
using FeatureCreateMemberRequest = Org.Shared.Features.Members.CreateMemberRequest;

namespace Org.Frontend.Services.Members;

public sealed class MemberMockService(FrontendMockDataStore mockDataStore) : IMemberService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;

    public Task<List<MemberDto>> GetMembers(Guid orgId)
    {
        return _mockDataStore.UseAsync(data => data.Members
            .Where(x => x.OrgId == orgId)
            .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(member => MapDto(member, data))
            .ToList());
    }

    public Task<MemberDto> CreateMember(Guid orgId, FeatureCreateMemberRequest req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            EnsureOrganizationExists(data, orgId);

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
            return MapDto(dto, data);
        });
    }

    public async Task AssignRole(Guid memberId, Guid roleId)
    {
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");

            current.RoleId = roleId;
            return 0;
        });
    }

    public async Task AssignDepartment(Guid memberId, Guid departmentId)
    {
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");

            EnsureDepartmentBelongsToOrganization(data, departmentId, current.OrgId);
            current.DepartmentId = departmentId;
            return 0;
        });
    }

    public async Task DeleteMember(Guid memberId)
    {
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Members.FirstOrDefault(x => x.Id == memberId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in mock data.");

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
}
