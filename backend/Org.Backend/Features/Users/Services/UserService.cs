using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Users.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserProfileDto> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return user.ToUserProfileDto();
    }

    public async Task<List<MyOrganizationDto>> GetMyOrganizationsAsync(Guid userId, CancellationToken ct = default)
    {
        var members = await _context.Members
            .Include(m => m.Organization)
            .Include(m => m.Role)
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .ToListAsync(ct);

        return members.Select(m => m.ToMyOrganizationDto()).ToList();
    }

    public async Task<List<MyEventDto>> GetMyEventsAsync(Guid userId, CancellationToken ct = default)
    {
        // Get events from organizations where user is a member
        var memberOrgIds = await _context.Members
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .Select(m => m.OrgId)
            .ToListAsync(ct);

        var events = await _context.Events
            .Include(e => e.Organization)
            .Where(e => memberOrgIds.Contains(e.OrgId))
            .OrderBy(e => e.StartDate)
            .ToListAsync(ct);

        return events.Select(e => e.ToMyEventDto()).ToList();
    }

    public async Task<List<DiscoverOrganizationDto>> DiscoverOrganizationsAsync(Guid userId, CancellationToken ct = default)
    {
        // Get organizations where user is NOT currently an active member.
        // Users who previously left (inactive membership) should still discover again.
        var memberOrgIds = await _context.Members
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .Select(m => m.OrgId)
            .ToListAsync(ct);

        var organizations = await _context.Organizations
            .Where(o => o.Status == OrgStatus.Active && !memberOrgIds.Contains(o.Id))
            .OrderByDescending(o => o.TotalMembers)
            .ToListAsync(ct);

        return organizations.Select(o => o.ToDiscoverOrganizationDto()).ToList();
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Update user profile fields
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.Dob = request.Dob;
        user.Gender = request.Gender?.Trim();
        user.Address = request.Address?.Trim();
        user.AvatarUrl = request.AvatarUrl?.Trim();
        user.Bio = request.Bio?.Trim();
        user.SocialLinks = request.SocialLinks?.Trim();
        
        // Parse and set profile visibility
        if (!string.IsNullOrWhiteSpace(request.ProfileVisibility))
        {
            if (Enum.TryParse<ProfileVisibility>(request.ProfileVisibility, out var visibility))
            {
                user.ProfileVisibility = visibility;
            }
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return user.ToUserProfileDto();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Verify current password
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        // Hash new password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }
}
