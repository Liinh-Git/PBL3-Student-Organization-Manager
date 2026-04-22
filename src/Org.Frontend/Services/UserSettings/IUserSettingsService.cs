using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.UserSettings;

public interface IUserSettingsService
{
    Task<UserSettingsPageViewModel> GetSettingsAsync(CancellationToken ct = default);

    Task<UserSettingsOperationResult> SaveProfileAsync(UserProfileFormViewModel profile, CancellationToken ct = default);

    Task<UserSettingsOperationResult> ChangePasswordAsync(PasswordChangeFormViewModel request, CancellationToken ct = default);

    Task<UserSettingsOperationResult> SaveNotificationsAsync(NotificationPreferencesViewModel preferences, CancellationToken ct = default);

    Task<UserSettingsOperationResult> RevokeOtherSessionsAsync(CancellationToken ct = default);

    Task<UserSettingsOperationResult> DeleteAccountAsync(CancellationToken ct = default);
}
