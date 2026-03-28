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

    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=StudentOrgDb;Username=org_admin;Password=SecretPassword123!";

    public DbSchemaTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _context = new AppDbContext(options);
    }

    /// <summary>
    /// Test 4: Insert User + Organization + Member, kiểm tra FK và navigation.
    /// </summary>
    [Fact]
    public async Task CanInsertUserOrganizationAndMember()
    {
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var user = new User
        {
            FullName = "Nguyễn Văn Test",
            Email = $"test_{stamp}@example.com",
            PasswordHash = "hashed_password_placeholder",
            Status = UserStatus.Active,
        };

        var org = new Organization
        {
            OrgName = $"CLB Test {stamp}",
            Status = OrgStatus.Active,
            Description = "Tổ chức dùng để test",
        };

        _context.Users.Add(user);
        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            UserId = user.Id,
            OrgId = org.Id,
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var savedMember = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Organization)
            .FirstAsync(m => m.Id == member.Id);

        Assert.Equal(user.Id, savedMember.UserId);
        Assert.Equal(org.Id, savedMember.OrgId);
        Assert.Equal(user.Email, savedMember.User.Email);
        Assert.Equal(org.OrgName, savedMember.Organization.OrgName);

        _context.Members.Remove(member);
        _context.Organizations.Remove(org);
        _context.Users.Remove(user);
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
            OrgName      = $"CLB Test {stamp}",
            Status       = OrgStatus.Active,
            Description = "Tổ chức dùng để test",
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        var dept = new Department
        {
            OrgId = org.Id,
            DeptName = "Ban Kỹ Thuật",
            Function = "Test department",
        };

        // Act
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        // Assert
        var savedDept = await _context.Departments
            .Include(d => d.Organization)
            .FirstAsync(d => d.Id == dept.Id);

        Assert.Equal(org.Id, savedDept.OrgId);
        Assert.Equal($"CLB Test {stamp}", savedDept.Organization.OrgName);

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
            OrgName = $"Org SoftDelete {stamp}",
            Status = OrgStatus.Active,
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        // Đánh dấu soft-delete
        org.IsDeleted = true;
        await _context.SaveChangesAsync();

        // Act — query bình thường (global filter áp dụng)
        var found = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == org.Id);

        // Assert — bản ghi đã bị ẩn bởi global soft-delete filter
        Assert.Null(found);

        // Cleanup — phải bypass filter để xóa thật
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Organizations\" WHERE \"Id\" = {0}", org.Id);
    }

    /// <summary>
    /// Test 7: Kiểm tra unique index trên Users(Email) — không cho phép email trùng.
    /// </summary>
    [Fact]
    public async Task UniqueEmailConstraintIsEnforced()
    {
        // Arrange
        var stamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var email = $"unique_test_{stamp}@example.com";

        var user1 = new User
        {
            FullName = "User One",
            Email = email,
            PasswordHash = "hash1",
            Status = UserStatus.Active,
        };

        var user2 = new User
        {
            FullName = "User Two",
            Email = email, // email trùng!
            PasswordHash = "hash2",
            Status = UserStatus.Active,
        };

        _context.Users.Add(user1);
        await _context.SaveChangesAsync();

        // Act & Assert — phải ném exception
        _context.Users.Add(user2);
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => _context.SaveChangesAsync());

        // Cleanup
        _context.ChangeTracker.Clear();
        var saved = await _context.Users.IgnoreQueryFilters().FirstAsync(u => u.Email == email);
        _context.Users.Remove(saved);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();
}
