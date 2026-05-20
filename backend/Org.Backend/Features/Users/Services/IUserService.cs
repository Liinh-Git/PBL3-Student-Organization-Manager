using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Services;

public interface IUserService
{
    // Read operations
    Task<UserProfileDto> GetMeAsync(Guid userId, CancellationToken ct = default);
    Task<List<MyOrganizationDto>> GetMyOrganizationsAsync(Guid userId, CancellationToken ct = default);
    Task<List<MyEventDto>> GetMyEventsAsync(Guid userId, CancellationToken ct = default);
    Task<List<MyTaskDto>> GetMyTasksAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default);
    Task<List<DiscoverOrganizationDto>> DiscoverOrganizationsAsync(Guid userId, CancellationToken ct = default);
    
    // Write operations
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken ct = default);
    Task<UserProfileDto> UploadAvatarAsync(Guid userId, Stream fileStream, string originalFileName, string contentType, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}
