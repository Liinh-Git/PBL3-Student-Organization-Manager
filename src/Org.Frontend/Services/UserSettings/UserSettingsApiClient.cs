using System.Net.Http.Json;
using System.Text.Json;
using Org.Frontend.Services.Auth;
using Org.Frontend.ViewModels;
using Org.Shared.Features.Users;

namespace Org.Frontend.Services.UserSettings;

public sealed class UserSettingsApiClient(IAuthenticatedBackendClient backendClient) : IUserSettingsService
{
    private const string DefaultAvatarUrl = "/images/mockimages/AvtUser/Avt1.jpg";

    private readonly IAuthenticatedBackendClient _backendClient = backendClient;

    public async Task<UserSettingsPageViewModel> GetSettingsAsync(CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetCurrentUserProfileResponse>("api/users/me", ct);
        var profile = payload?.Data ?? throw new InvalidOperationException("Empty profile payload from API.");

        return new UserSettingsPageViewModel
        {
            Profile = MapProfile(profile),
            Notifications = new NotificationPreferencesViewModel
            {
                EmailNotificationsEnabled = true,
                AppPushEnabled = true,
                SmsAlertsEnabled = false
            },
            Devices =
            [
                new UserDeviceViewModel
                {
                    DeviceName = "Trinh duyet hien tai",
                    LastActiveLabel = "Hoat dong hien tai",
                    IsCurrentSession = true
                }
            ]
        };
    }

    public async Task<UserSettingsOperationResult> SaveProfileAsync(UserProfileFormViewModel profile, CancellationToken ct = default)
    {
        var normalizedName = NormalizeName(profile.FullName);
        if (normalizedName is null)
            return UserSettingsOperationResult.Failure("Ho va ten phai co it nhat 2 ky tu.");

        if (profile.DateOfBirth is not null && profile.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Today))
            return UserSettingsOperationResult.Failure("Ngay sinh khong hop le.");

        var request = new UpdateCurrentUserProfileRequest(
            normalizedName,
            NormalizeOptional(profile.PhoneNumber),
            profile.DateOfBirth,
            NormalizeOptional(profile.Gender),
            NormalizeOptional(profile.Address),
            NormalizeOptional(profile.AvatarUrl),
            NormalizeOptional(profile.Bio),
            null,
            NormalizeProfileVisibility(profile.ProfileVisibility));

        try
        {
            using var putRequest = new HttpRequestMessage(HttpMethod.Put, "api/users/me")
            {
                Content = JsonContent.Create(request)
            };
            using var _ = await _backendClient.SendAsync(putRequest, ct);
            return UserSettingsOperationResult.Success("Da luu thong tin ca nhan thanh cong.");
        }
        catch (AuthApiException ex)
        {
            return UserSettingsOperationResult.Failure(ex.Message);
        }
    }

    public Task<UserSettingsOperationResult> ChangePasswordAsync(PasswordChangeFormViewModel request, CancellationToken ct = default)
    {
        return Task.FromResult(UserSettingsOperationResult.Unsupported(
            "API doi mat khau chua duoc backend ho tro. Vui long su dung mock mode de demo tinh nang nay."));
    }

    public Task<UserSettingsOperationResult> SaveNotificationsAsync(NotificationPreferencesViewModel preferences, CancellationToken ct = default)
    {
        return Task.FromResult(UserSettingsOperationResult.Unsupported(
            "API cai dat thong bao chua duoc backend ho tro. Vui long su dung mock mode de demo tinh nang nay."));
    }

    public Task<UserSettingsOperationResult> RevokeOtherSessionsAsync(CancellationToken ct = default)
    {
        return Task.FromResult(UserSettingsOperationResult.Unsupported(
            "API quan ly thiet bi dang nhap chua duoc backend ho tro."));
    }

    public Task<UserSettingsOperationResult> DeleteAccountAsync(CancellationToken ct = default)
    {
        return Task.FromResult(UserSettingsOperationResult.Unsupported(
            "API xoa tai khoan chua duoc backend ho tro. Hanh dong nay tam thoi chi co trong mock mode."));
    }

    private static UserProfileFormViewModel MapProfile(UserProfileDto dto)
    {
        return new UserProfileFormViewModel
        {
            UserId = dto.Id,
            ProfileCode = BuildProfileCode(dto.Id),
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address,
            AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? DefaultAvatarUrl : dto.AvatarUrl,
            Bio = dto.Bio,
            ProfileVisibility = NormalizeProfileVisibility(dto.ProfileVisibility)
        };
    }

    private static string BuildProfileCode(Guid id)
        => $"KORA-{Math.Abs(id.GetHashCode()) % 10000:0000}";

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeName(string? value)
    {
        var normalized = NormalizeOptional(value);
        return normalized is not null && normalized.Length >= 2 ? normalized : null;
    }

    private static string NormalizeProfileVisibility(string? value)
    {
        return value?.Trim().ToUpperInvariant() switch
        {
            "PRIVATE" => "Private",
            "ORGANIZATIONONLY" => "OrganizationOnly",
            _ => "Public"
        };
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var fallback = $"User settings API failed with status code {(int)response.StatusCode}.";
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(raw))
            return fallback;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("message", out var message)
                    && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString() ?? fallback;
                }

                if (doc.RootElement.TryGetProperty("reason", out var reason)
                    && reason.ValueKind == JsonValueKind.String)
                {
                    return reason.GetString() ?? fallback;
                }
            }
        }
        catch (JsonException)
        {
            // Ignore and fallback to raw text.
        }

        return raw;
    }
}
