using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.UserSettings;

public sealed class UserProfileMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IUserProfileService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<UserProfileViewModel> GetUserProfileAsync(Guid targetUserId, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        return await _mockDataStore.UseAsync(data =>
        {
            var target = data.Users.FirstOrDefault(x => x.Id == targetUserId)
                ?? throw new KeyNotFoundException($"User {targetUserId} not found.");

            var isOwner = currentUserId.Value == targetUserId;
            var visibility = NormalizeVisibility(target.ProfileVisibility);
            var shareOrganization = ShareOrganization(data, currentUserId.Value, targetUserId);
            var canViewFull = isOwner || visibility == "Public" || (visibility == "OrganizationOnly" && shareOrganization);
            var isVisible = canViewFull || isOwner;

            var memberships = data.Members
                .Where(x => x.UserId == targetUserId)
                .Select(x => new UserProfileOrganizationSummaryViewModel
                {
                    OrganizationId = x.OrgId,
                    OrganizationName = data.Organizations.FirstOrDefault(o => o.Id == x.OrgId)?.OrgName ?? "Organization",
                    RoleName = ResolveRoleName(data, x.RoleId)
                })
                .OrderBy(x => x.OrganizationName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!isVisible)
            {
                return new UserProfileViewModel
                {
                    UserId = target.Id,
                    FullName = target.FullName,
                    AvatarUrl = target.AvatarUrl,
                    ProfileVisibility = visibility,
                    IsOwnerView = false,
                    CanViewFullProfile = false,
                    IsProfileVisibleToViewer = false,
                    HiddenReason = "This profile is private.",
                    Organizations = []
                };
            }

            var model = new UserProfileViewModel
            {
                UserId = target.Id,
                FullName = target.FullName,
                AvatarUrl = target.AvatarUrl,
                Bio = target.Bio,
                ProfileVisibility = visibility,
                IsOwnerView = isOwner,
                CanViewFullProfile = canViewFull,
                IsProfileVisibleToViewer = true,
                Organizations = memberships
            };

            if (isOwner)
            {
                model.Email = target.Email;
                model.PhoneNumber = target.PhoneNumber;
                model.Gender = target.Gender;
                model.DateOfBirth = target.DateOfBirth;
                model.Address = target.Address;
                return model;
            }

            if (canViewFull)
            {
                model.Email = visibility == "Public" ? target.Email : null;
                model.PhoneNumber = null;
                model.Gender = target.Gender;
                model.DateOfBirth = null;
                model.Address = null;
            }

            return model;
        }, ct);
    }

    public async Task<string> GetMyProfileVisibilityAsync(CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        return await _mockDataStore.UseAsync(data =>
        {
            var user = data.Users.FirstOrDefault(x => x.Id == currentUserId.Value)
                ?? throw new KeyNotFoundException("Current user not found.");
            return NormalizeVisibility(user.ProfileVisibility);
        }, ct);
    }

    public async Task UpdateMyProfileVisibilityAsync(string visibility, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync();
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        await _mockDataStore.UseAsync(data =>
        {
            var user = data.Users.FirstOrDefault(x => x.Id == currentUserId.Value)
                ?? throw new KeyNotFoundException("Current user not found.");
            user.ProfileVisibility = NormalizeVisibility(visibility);
            user.UpdatedAt = DateTime.UtcNow;
            return 0;
        }, ct);
    }

    private static bool ShareOrganization(MockDataSet data, Guid firstUserId, Guid secondUserId)
    {
        var firstOrgIds = data.Members.Where(x => x.UserId == firstUserId).Select(x => x.OrgId).ToHashSet();
        return data.Members.Any(x => x.UserId == secondUserId && firstOrgIds.Contains(x.OrgId));
    }

    private static string ResolveRoleName(MockDataSet data, Guid? roleId)
    {
        if (!roleId.HasValue)
        {
            return "Member";
        }

        return data.OrganizationRoles.FirstOrDefault(x => x.Id == roleId.Value)?.RoleName ?? "Member";
    }

    private static string NormalizeVisibility(string? value)
    {
        return value?.Trim().ToUpperInvariant() switch
        {
            "PRIVATE" => "Private",
            "ORGANIZATIONONLY" => "OrganizationOnly",
            _ => "Public"
        };
    }

    private async Task<Guid?> GetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }
}
