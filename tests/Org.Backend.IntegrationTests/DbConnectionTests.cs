using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;

namespace Org.Backend.IntegrationTests;

/// <summary>
/// Kiểm tra kết nối cơ bản tới PostgreSQL database.
/// Yêu cầu: database StudentOrgDb đã được tạo và migration đã chạy.
/// </summary>
public class DbConnectionTests : IDisposable
{
    private readonly AppDbContext _context;

    // Ưu tiên dùng biến môi trường khi chạy CI; fallback theo appsettings.Development.
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=StudentOrgDb;Username=org_admin;Password=CHANGE_ME";

    public DbConnectionTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _context = new AppDbContext(options);
    }

    /// <summary>
    /// Test 1: Kiểm tra kết nối TCP tới PostgreSQL server.
    /// Không cần database tồn tại, chỉ cần server chạy.
    /// </summary>
    [Fact]
    public async Task CanConnectToPostgresServer()
    {
        // Act
        var canConnect = await _context.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect, "Không thể kết nối tới PostgreSQL. Kiểm tra lại server đang chạy và connection string.");
    }

    /// <summary>
    /// Test 2: Kiểm tra tất cả migrations đã được apply (schema đúng).
    /// Nếu test này fail, chạy: dotnet ef database update
    /// </summary>
    [Fact]
    public async Task AllMigrationsHaveBeenApplied()
    {
        // Act
        var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

        // Assert
        Assert.Empty(pendingMigrations);
    }

    /// <summary>
    /// Test 3: Kiểm tra các bảng cốt lõi tồn tại bằng cách count (không có data cũng OK).
    /// </summary>
    [Fact]
    public async Task CoreTablesExistAndAreQueryable()
    {
        // Assert — chỉ cần không ném exception là bảng tồn tại
        var userCount       = await _context.Users.CountAsync();
        var orgCount        = await _context.Organizations.CountAsync();
        var memberCount     = await _context.Members.CountAsync();
        var eventCount      = await _context.Events.CountAsync();

        // Kết quả >= 0 là hợp lệ (database mới thì == 0)
        Assert.True(userCount >= 0);
        Assert.True(orgCount >= 0);
        Assert.True(memberCount >= 0);
        Assert.True(eventCount >= 0);
    }

    public void Dispose() => _context.Dispose();
}
