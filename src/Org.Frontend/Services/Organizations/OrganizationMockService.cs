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

    public async Task<OrganizationDetailViewModel> GetOrganizationDetailAsync(Guid id, CancellationToken ct = default)
    {
        return await _mockDataStore.UseAsync(data =>
        {
            var org = data.Organizations.FirstOrDefault(x => x.Id == id);
            if (org == null)
            {
                throw new KeyNotFoundException($"Organization {id} not found.");
            }

            // Fetch Admins/Owners
            var adminMembers = data.Members
                .Where(m => m.OrgId == id && IsAdminRole(m.RoleId))
                .Take(4)
                .Select(m => new OrganizationAdminViewModel
                {
                    Name = m.DisplayName ?? "Anonymous",
                    Role = ResolveRoleName(m.RoleId),
                    Avatar = ResolveUserAvatar(m.UserId, data)
                })
                .ToList();

            // Mock Timeline (In real BE this would be a separate table/collection)
            var timeline = new List<OrganizationTimelineViewModel>
            {
                new() { Month = "Tháng 05/2021", Title = $"Thành lập {org.OrgName}", Description = "Khởi xướng dự án với định hướng phát triển cộng đồng sinh viên năng động." },
                new() { Month = "Tháng 12/2022", Title = "Mở rộng quy mô", Description = "Đạt cột mốc quan trọng về số lượng thành viên và chất lượng chuyên môn." },
                new() { Month = "Tháng 01/2024", Title = "Giải thưởng Cống hiến", Description = "Được vinh danh là tổ chức hoạt động xuất sắc nhất năm." }
            };

            return new OrganizationDetailViewModel
            {
                Id = org.Id,
                Name = org.OrgName,
                Description = org.Description,
                AvatarUrl = org.AvatarUrl ?? "/images/icons/icon1.png",
                CoverUrl = org.CoverUrl ?? "/images/banners/org-banner.jpg",
                Location = org.Location ?? "Vietnam",
                TotalMembers = data.Members.Count(m => m.OrgId == id),
                IsActive = org.Status == 0,
                FoundedDate = new DateTime(2021, 5, 15), // Mock
                Admins = adminMembers,
                Timeline = timeline
            };
        });
    }

    public async Task<OrganizationDetailViewModel> CreateOrganizationAsync(CreateOrganizationViewModel model, CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdStr = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) throw new UnauthorizedAccessException("User not logged in.");
        var userId = Guid.Parse(userIdStr);

        return await _mockDataStore.UseAsync(data =>
        {
            var newOrg = new MockOrganization
            {
                Id = Guid.NewGuid(),
                OrgName = model.Name,
                Description = model.Description,
                Location = model.Location ?? "Vietnam",
                AvatarUrl = model.AvatarUrl ?? "/images/icons/icon1.png",
                CoverUrl = model.CoverUrl ?? "/images/banners/org-banner.jpg",
                Status = 0
            };

            data.Organizations.Add(newOrg);

            // Add creator as Owner
            var user = data.Users.FirstOrDefault(u => u.Id == userId);
            data.Members.Add(new MockMember
            {
                Id = Guid.NewGuid(),
                OrgId = newOrg.Id,
                UserId = userId,
                DisplayName = user?.FullName ?? "Owner",
                RoleId = Guid.Parse("7e1e6b6d-9b5d-4f1e-9b5d-4f1e9b5d4f1e"), // Hardcoded Owner ID for mock
                JoinDate = DateTime.UtcNow
            });

            return new OrganizationDetailViewModel
            {
                Id = newOrg.Id,
                Name = newOrg.OrgName,
                Description = newOrg.Description,
                AvatarUrl = newOrg.AvatarUrl,
                CoverUrl = newOrg.CoverUrl,
                Location = newOrg.Location,
                TotalMembers = 1,
                IsActive = true,
                FoundedDate = DateTime.Today
            };
        });
    }

    private static bool IsAdminRole(Guid? roleId)
    {
        if (!roleId.HasValue) return false;
        var s = roleId.Value.ToString().ToLower();
        return s.StartsWith("7e") || s.StartsWith("f8"); // Owner or Admin
    }

    private static string ResolveRoleName(Guid? roleId)
    {
        if (!roleId.HasValue) return "Member";
        var s = roleId.Value.ToString().ToLower();
        if (s.StartsWith("7e")) return "Chủ sở hữu";
        if (s.StartsWith("f8")) return "Quản trị viên";
        return "Thành viên";
    }

    private static string? ResolveUserAvatar(Guid userId, MockDataSet data)
    {
        var user = data.Users.FirstOrDefault(u => u.Id == userId);
        return user?.AvatarUrl ?? "/images/mockimages/AvtUser/Avt1.jpg";
    }
}
