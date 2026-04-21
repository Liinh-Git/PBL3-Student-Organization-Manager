namespace Org.Frontend.ViewModels;

public sealed class UserSettingsPageViewModel
{
    public UserProfileFormViewModel Profile { get; set; } = new();
    public NotificationPreferencesViewModel Notifications { get; set; } = new();
    public List<UserDeviceViewModel> Devices { get; set; } = [];

    public UserSettingsPageViewModel Clone()
    {
        return new UserSettingsPageViewModel
        {
            Profile = Profile.Clone(),
            Notifications = Notifications.Clone(),
            Devices = Devices.Select(x => x.Clone()).ToList()
        };
    }
}

public sealed class UserProfileFormViewModel
{
    public Guid UserId { get; set; }
    public string ProfileCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }

    public UserProfileFormViewModel Clone()
    {
        return new UserProfileFormViewModel
        {
            UserId = UserId,
            ProfileCode = ProfileCode,
            FullName = FullName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            DateOfBirth = DateOfBirth,
            Gender = Gender,
            Address = Address,
            AvatarUrl = AvatarUrl,
            Bio = Bio
        };
    }
}

public sealed class PasswordChangeFormViewModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class NotificationPreferencesViewModel
{
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool AppPushEnabled { get; set; } = true;
    public bool SmsAlertsEnabled { get; set; }

    public NotificationPreferencesViewModel Clone()
    {
        return new NotificationPreferencesViewModel
        {
            EmailNotificationsEnabled = EmailNotificationsEnabled,
            AppPushEnabled = AppPushEnabled,
            SmsAlertsEnabled = SmsAlertsEnabled
        };
    }
}

public sealed class UserDeviceViewModel
{
    public string DeviceName { get; set; } = string.Empty;
    public string LastActiveLabel { get; set; } = string.Empty;
    public bool IsCurrentSession { get; set; }

    public UserDeviceViewModel Clone()
    {
        return new UserDeviceViewModel
        {
            DeviceName = DeviceName,
            LastActiveLabel = LastActiveLabel,
            IsCurrentSession = IsCurrentSession
        };
    }
}

public sealed record UserSettingsOperationResult(bool Succeeded, string Message, bool IsUnsupported = false)
{
    public static UserSettingsOperationResult Success(string message) => new(true, message);

    public static UserSettingsOperationResult Failure(string message) => new(false, message);

    public static UserSettingsOperationResult Unsupported(string message) => new(false, message, true);
}
