using System.Net;
using System.Net.Http.Json;
using Org.Frontend.ViewModels;
using Org.Shared.Features.Users;

namespace Org.Frontend.Services.UserSettings;

public sealed class UserProfileApiClient(HttpClient httpClient) : IUserProfileService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<UserProfileViewModel> GetUserProfileAsync(Guid targetUserId, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"api/users/{targetUserId:D}", ct);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            var me = await _httpClient.GetFromJsonAsync<GetCurrentUserProfileResponse>("api/users/me", ct)
                ?? throw new InvalidOperationException("Cannot resolve current user profile.");

            if (me.Data.Id == targetUserId)
            {
                return MapToViewModel(me.Data, isOwner: true, isVisible: true, canViewFull: true);
            }

            return new UserProfileViewModel
            {
                UserId = targetUserId,
                FullName = "Private Profile",
                ProfileVisibility = "Private",
                IsOwnerView = false,
                CanViewFullProfile = false,
                IsProfileVisibleToViewer = false,
                HiddenReason = "This profile is private."
            };
        }

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GetUserByIdResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("User profile payload is empty.");
        var current = await _httpClient.GetFromJsonAsync<GetCurrentUserProfileResponse>("api/users/me", ct)
            ?? throw new InvalidOperationException("Cannot resolve current user profile.");

        var isOwner = current.Data.Id == payload.Data.Id;
        return MapToViewModel(payload.Data, isOwner, isVisible: true, canViewFull: true);
    }

    public async Task<string> GetMyProfileVisibilityAsync(CancellationToken ct = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetCurrentUserProfileResponse>("api/users/me", ct)
            ?? throw new InvalidOperationException("User profile payload is empty.");
        return NormalizeVisibility(payload.Data.ProfileVisibility);
    }

    public async Task UpdateMyProfileVisibilityAsync(string visibility, CancellationToken ct = default)
    {
        var current = await _httpClient.GetFromJsonAsync<GetCurrentUserProfileResponse>("api/users/me", ct)
            ?? throw new InvalidOperationException("User profile payload is empty.");
        var me = current.Data;

        var request = new UpdateCurrentUserProfileRequest(
            me.FullName,
            me.PhoneNumber,
            me.DateOfBirth,
            me.Gender,
            me.Address,
            me.AvatarUrl,
            me.Bio,
            me.SocialLinksJson,
            NormalizeVisibility(visibility));

        using var response = await _httpClient.PutAsJsonAsync("api/users/me", request, ct);
        response.EnsureSuccessStatusCode();
    }

    private static UserProfileViewModel MapToViewModel(UserProfileDto dto, bool isOwner, bool isVisible, bool canViewFull)
    {
        return new UserProfileViewModel
        {
            UserId = dto.Id,
            FullName = dto.FullName,
            AvatarUrl = dto.AvatarUrl,
            Bio = dto.Bio,
            Email = dto.Email,
            PhoneNumber = isOwner ? dto.PhoneNumber : null,
            Gender = dto.Gender,
            DateOfBirth = isOwner ? dto.DateOfBirth : null,
            Address = isOwner ? dto.Address : null,
            ProfileVisibility = NormalizeVisibility(dto.ProfileVisibility),
            IsOwnerView = isOwner,
            CanViewFullProfile = canViewFull,
            IsProfileVisibleToViewer = isVisible,
            HiddenReason = null,
            Organizations = []
        };
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
}
