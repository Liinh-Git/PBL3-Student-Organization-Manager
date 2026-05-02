using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.UserSettings;

public interface IUserProfileService
{
    Task<UserProfileViewModel> GetUserProfileAsync(Guid targetUserId, CancellationToken ct = default);
    Task<string> GetMyProfileVisibilityAsync(CancellationToken ct = default);
    Task UpdateMyProfileVisibilityAsync(string visibility, CancellationToken ct = default);
}
