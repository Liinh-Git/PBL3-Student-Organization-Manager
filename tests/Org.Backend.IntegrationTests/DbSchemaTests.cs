using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Infrastructure.Database;

namespace Org.Backend.IntegrationTests;

/// <summary>
/// Kiểm tra toàn bộ schema: insert dữ liệu test vào các bảng chính,
/// xác nhận FK, constraints, và soft-delete filter hoạt động đúng.
/// Mỗi test tự dọn dẹp dữ liệu sau khi chạy.
/// </summary>
public class DbSchemaTests : IDisposable
{
    private readonly AppDbContext _context;

    private const string ConnectionString =
        "Host=localhost;Port=5433;Database=StudentOrgDb;Username=org_admin;Password=SecretPassword123!";

    public DbSchemaTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _context = new AppDbContext(options);
    }

    /// <summary>
    /// Test 4: Insert Account → UserProfile, kiểm tra quan hệ 1:1.
    /// </summary>
    [Fact]
    public async Task CanInsertAccountAndUserProfile()
    {
        // Arrange — dùng timestamp để tránh trùng username/email
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var account = new Account
        {
            Username     = $"testuser_{stamp}",
            Email        = $"test_{stamp}@example.com",
            PasswordHash = "hashed_password_placeholder",
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            IsDeleted    = false,
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var profile = new UserProfile
        {
            AccountId  = account.Id,
            FullName   = "Nguyễn Văn Test",
            StudentId  = $"SV{stamp}",
            CreatedAt  = DateTime.UtcNow,
            IsDeleted  = false,
        };

        // Act
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Assert
        var savedProfile = await _context.UserProfiles
            .Include(u => u.Account)
            .FirstAsync(u => u.AccountId == account.Id);

        Assert.Equal("Nguyễn Văn Test", savedProfile.FullName);
        Assert.Equal(account.Username, savedProfile.Account.Username);

        // Cleanup
        _context.UserProfiles.Remove(profile);
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Test 5: Insert Organization → Department, kiểm tra FK cascade.
    /// </summary>
    [Fact]
    public async Task CanInsertOrganizationAndDepartment()
    {
        // Arrange
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var org = new Organization
        {
            Name        = $"CLB Test {stamp}",
            OrgType     = OrgType.Club,
            Description = "Tổ chức dùng để test",
            CreatedAt   = DateTime.UtcNow,
            IsDeleted   = false,
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        var dept = new Department
        {
            OrganizationId = org.Id,
            Name           = "Ban Kỹ Thuật",
            Description    = "Test department",
            CreatedAt      = DateTime.UtcNow,
            IsDeleted      = false,
        };

        // Act
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        // Assert
        var savedDept = await _context.Departments
            .Include(d => d.Organization)
            .FirstAsync(d => d.Id == dept.Id);

        Assert.Equal(org.Id, savedDept.OrganizationId);
        Assert.Equal($"CLB Test {stamp}", savedDept.Organization.Name);

        // Cleanup — xóa dept trước (FK), rồi org
        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        _context.Organizations.Remove(org);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Test 6: Kiểm tra global soft-delete filter — bản ghi IsDeleted=true không hiện trong query.
    /// </summary>
    [Fact]
    public async Task SoftDeleteFilterHidesDeletedRecords()
    {
        // Arrange
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var org = new Organization
        {
            Name      = $"Org SoftDelete {stamp}",
            OrgType   = OrgType.Club,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        // Đánh dấu soft-delete
        org.IsDeleted = true;
        await _context.SaveChangesAsync();

        // Act — query bình thường (global filter áp dụng)
        var found = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == org.Id);

        // Assert — bản ghi đã bị ẩn bởi soft-delete filter
        Assert.Null(found);

        // Cleanup — phải bypass filter để xóa thật
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Organizations\" WHERE \"Id\" = {0}", org.Id);
    }

    /// <summary>
    /// Test 7: Kiểm tra unique index trên Accounts(Email) — không cho phép email trùng.
    /// </summary>
    [Fact]
    public async Task UniqueEmailConstraintIsEnforced()
    {
        // Arrange
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var email = $"unique_test_{stamp}@example.com";

        var account1 = new Account
        {
            Username     = $"user1_{stamp}",
            Email        = email,
            PasswordHash = "hash1",
            CreatedAt    = DateTime.UtcNow,
            IsDeleted    = false,
        };

        var account2 = new Account
        {
            Username     = $"user2_{stamp}",
            Email        = email, // email trùng!
            PasswordHash = "hash2",
            CreatedAt    = DateTime.UtcNow,
            IsDeleted    = false,
        };

        _context.Accounts.Add(account1);
        await _context.SaveChangesAsync();

        // Act & Assert — phải ném exception
        _context.Accounts.Add(account2);
        await Assert.ThrowsAnyAsync<Exception>(() => _context.SaveChangesAsync());

        // Cleanup
        _context.ChangeTracker.Clear();
        var saved = await _context.Accounts.IgnoreQueryFilters().FirstAsync(a => a.Email == email);
        _context.Accounts.Remove(saved);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();
}
