using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.UserSettings;

public sealed class UserSettingsMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IUserSettingsService
{
    private const string DefaultAvatarUrl = "/images/mockimages/FB_IMG_1744903311334.jpg";

    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<UserSettingsPageViewModel> GetSettingsAsync(CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = ParseUserId(user);

        return await _mockDataStore.UseAsync(data =>
        {
            var target = EnsureMockUser(data, userId, user);
            EnsureDefaults(target, user);
            return MapToPageModel(target);
        }, ct);
    }

    public async Task<UserSettingsOperationResult> SaveProfileAsync(UserProfileFormViewModel profile, CancellationToken ct = default)
    {
        var normalizedName = NormalizeName(profile.FullName);
        if (normalizedName is null)
            return UserSettingsOperationResult.Failure("Ho va ten phai co it nhat 2 ky tu.");

        if (profile.DateOfBirth is not null && profile.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Today))
            return UserSettingsOperationResult.Failure("Ngay sinh khong hop le.");

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = ParseUserId(user);

        await _mockDataStore.UseAsync(data =>
        {
            var target = EnsureMockUser(data, userId, user);
            target.FullName = normalizedName;
            target.PhoneNumber = NormalizeOptional(profile.PhoneNumber);
            target.DateOfBirth = profile.DateOfBirth;
            target.Gender = NormalizeOptional(profile.Gender);
            target.Address = NormalizeOptional(profile.Address);
            target.AvatarUrl = NormalizeOptional(profile.AvatarUrl) ?? target.AvatarUrl;
            target.Bio = NormalizeOptional(profile.Bio);
            target.UpdatedAt = DateTime.UtcNow;
            return 0;
        }, ct);

        return UserSettingsOperationResult.Success("Da luu thong tin ca nhan thanh cong.");
    }

    public async Task<UserSettingsOperationResult> ChangePasswordAsync(PasswordChangeFormViewModel request, CancellationToken ct = default)
    {
        var validation = ValidatePasswordRequest(request);
        if (!validation.Succeeded)
            return validation;

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = ParseUserId(user);

        return await _mockDataStore.UseAsync(data =>
        {
            var target = EnsureMockUser(data, userId, user);

            if (!string.IsNullOrWhiteSpace(target.PasswordHash)
                && !string.Equals(target.PasswordHash, request.CurrentPassword, StringComparison.Ordinal))
            {
                return UserSettingsOperationResult.Failure("Mat khau hien tai chua dung.");
            }

            if (string.Equals(request.CurrentPassword, request.NewPassword, StringComparison.Ordinal))
            {
                return UserSettingsOperationResult.Failure("Mat khau moi phai khac mat khau hien tai.");
            }

            target.PasswordHash = request.NewPassword;
            target.UpdatedAt = DateTime.UtcNow;
            return UserSettingsOperationResult.Success("Da cap nhat bao mat thanh cong.");
        }, ct);
    }

    public async Task<UserSettingsOperationResult> SaveNotificationsAsync(NotificationPreferencesViewModel preferences, CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = ParseUserId(user);

        await _mockDataStore.UseAsync(data =>
        {
            var target = EnsureMockUser(data, userId, user);
            target.EmailNotificationsEnabled = preferences.EmailNotificationsEnabled;
            target.AppPushEnabled = preferences.AppPushEnabled;
            target.SmsAlertsEnabled = preferences.SmsAlertsEnabled;
            target.UpdatedAt = DateTime.UtcNow;
            return 0;
        }, ct);

        return UserSettingsOperationResult.Success("Da luu cai dat thong bao.");
    }

    public async Task<UserSettingsOperationResult> RevokeOtherSessionsAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return UserSettingsOperationResult.Success("Da thu hoi cac phien dang nhap khac trong mock mode.");
    }

    public async Task<UserSettingsOperationResult> DeleteAccountAsync(CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = ParseUserId(user);

        await _mockDataStore.UseAsync(data =>
        {
            var target = EnsureMockUser(data, userId, user);
            target.Status = "Inactive";
            target.UpdatedAt = DateTime.UtcNow;
            return 0;
        }, ct);

        return UserSettingsOperationResult.Success("Tai khoan da duoc chuyen sang trang thai tam ngung trong mock mode.");
    }

    private static UserSettingsOperationResult ValidatePasswordRequest(PasswordChangeFormViewModel request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return UserSettingsOperationResult.Failure("Vui long nhap mat khau hien tai.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return UserSettingsOperationResult.Failure("Vui long nhap mat khau moi.");

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            return UserSettingsOperationResult.Failure("Xac nhan mat khau chua khop.");

        var hasUpper = request.NewPassword.Any(char.IsUpper);
        var hasLower = request.NewPassword.Any(char.IsLower);
        var hasDigit = request.NewPassword.Any(char.IsDigit);

        if (request.NewPassword.Length < 8 || !hasUpper || !hasLower || !hasDigit)
        {
            return UserSettingsOperationResult.Failure(
                "Mat khau moi can it nhat 8 ky tu, bao gom chu hoa, chu thuong va chu so.");
        }

        return UserSettingsOperationResult.Success("OK");
    }

    private static Guid ParseUserId(ClaimsPrincipal user)
    {
        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId))
            throw new InvalidOperationException("Invalid user context.");

        return userId;
    }

    private static MockUser EnsureMockUser(MockDataSet data, Guid userId, ClaimsPrincipal principal)
    {
        var target = data.Users.FirstOrDefault(x => x.Id == userId);
        if (target is not null)
            return target;

        target = new MockUser
        {
            Id = userId,
            FullName = principal.Identity?.Name?.Trim() ?? "Nguoi dung",
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? $"user.{userId:N}@kora.local"
        };

        data.Users.Add(target);
        return target;
    }

    private static void EnsureDefaults(MockUser user, ClaimsPrincipal principal)
    {
        user.PhoneNumber ??= "+84 901 234 567";
        user.DateOfBirth ??= new DateOnly(2002, 10, 15);
        user.Gender ??= "Nam";
        user.Address ??= "Quan 1, TP. Ho Chi Minh, Viet Nam";
        user.Bio ??= "Thanh vien nang dong trong cong dong Kora.";
        user.AvatarUrl ??= DefaultAvatarUrl;
        user.Status ??= "Active";
        user.EmailNotificationsEnabled ??= true;
        user.AppPushEnabled ??= true;
        user.SmsAlertsEnabled ??= false;

        if (string.IsNullOrWhiteSpace(user.FullName))
            user.FullName = principal.Identity?.Name?.Trim() ?? "Nguoi dung";

        if (string.IsNullOrWhiteSpace(user.Email))
            user.Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    private static UserSettingsPageViewModel MapToPageModel(MockUser user)
    {
        return new UserSettingsPageViewModel
        {
            Profile = new UserProfileFormViewModel
            {
                UserId = user.Id,
                ProfileCode = BuildProfileCode(user.Id),
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio
            },
            Notifications = new NotificationPreferencesViewModel
            {
                EmailNotificationsEnabled = user.EmailNotificationsEnabled ?? true,
                AppPushEnabled = user.AppPushEnabled ?? true,
                SmsAlertsEnabled = user.SmsAlertsEnabled ?? false
            },
            Devices =
            [
                new UserDeviceViewModel
                {
                    DeviceName = "Chrome tren Windows",
                    LastActiveLabel = "Hoat dong hien tai",
                    IsCurrentSession = true
                },
                new UserDeviceViewModel
                {
                    DeviceName = "Mobile App - Android",
                    LastActiveLabel = "15 phut truoc",
                    IsCurrentSession = false
                }
            ]
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
}
