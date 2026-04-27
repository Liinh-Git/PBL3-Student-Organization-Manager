// ---- Chạy seed mode: migrate DB, seed dữ liệu mẫu, in summary rồi thoát ----
using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;

namespace Org.Backend.Infrastructure.Startup;

public static class SeedModeRunner
{
    // ---- Trả true nếu đã xử lý seed mode để Program.cs dừng startup web server ----
    public static async Task<bool> TryRunAsync(WebApplication app, bool isSeedMode, CancellationToken cancellationToken = default)
    {
        // ---- Web mode: không làm gì ở đây, để Program chạy pipeline API bình thường ----
        if (!isSeedMode)
        {
            return false;
        }

        // ---- Seed mode: tạo scope service và lấy AppDbContext ----
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // ---- Đồng bộ migration trước, sau đó seed để tránh lỗi thiếu bảng/cột ----
        await db.Database.MigrateAsync(cancellationToken);
        await DatabaseSeeder.SeedAsync(db, cancellationToken);
        
        // ---- Tự động export ra JSON cho FE sau khi seed ----
        var feMockDataPath = Path.Combine(app.Environment.ContentRootPath, "..", "Org.Frontend", "Services", "Mocks", "Data");
        await MockDataExporter.ExportToJsonAsync(db, feMockDataPath, cancellationToken);

        Console.WriteLine("Seeded and exported mock data successfully.");
        await PrintSummaryAsync(db, cancellationToken);
        await PrintSampleRowsAsync(db, cancellationToken);

        return true;
    }

    // ---- In thống kê nhanh sau seed để dev kiểm tra dữ liệu đã được tạo ----
    private static async Task PrintSummaryAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Users: {await db.Users.CountAsync(cancellationToken)}");
        Console.WriteLine($"Organizations: {await db.Organizations.CountAsync(cancellationToken)}");
        Console.WriteLine($"Departments: {await db.Departments.CountAsync(cancellationToken)}");
        Console.WriteLine($"Roles: {await db.Roles.CountAsync(cancellationToken)}");
        Console.WriteLine($"Permissions: {await db.Permissions.CountAsync(cancellationToken)}");
        Console.WriteLine($"RolePermissions: {await db.RolePermissions.CountAsync(cancellationToken)}");
        Console.WriteLine($"Members: {await db.Members.CountAsync(cancellationToken)}");
        Console.WriteLine($"Events: {await db.Events.CountAsync(cancellationToken)}");
        Console.WriteLine($"EventMembers: {await db.EventMembers.CountAsync(cancellationToken)}");
        Console.WriteLine($"EventReports: {await db.EventReports.CountAsync(cancellationToken)}");
        Console.WriteLine($"Milestones: {await db.Milestones.CountAsync(cancellationToken)}");
        Console.WriteLine($"EventCategories: {await db.EventCategories.CountAsync(cancellationToken)}");
        Console.WriteLine($"Tasks: {await db.Tasks.CountAsync(cancellationToken)}");
        Console.WriteLine($"Attendees: {await db.Attendees.CountAsync(cancellationToken)}");
        Console.WriteLine($"DigitalAssets: {await db.DigitalAssets.CountAsync(cancellationToken)}");
        Console.WriteLine($"Requests: {await db.Requests.CountAsync(cancellationToken)}");
        Console.WriteLine($"Resources: {await db.Resources.CountAsync(cancellationToken)}");
        Console.WriteLine($"ActivityHistories: {await db.ActivityHistories.CountAsync(cancellationToken)}");
    }

    // ---- In vài bản ghi mẫu để dev kiểm tra nhanh dummy data đã đúng cấu trúc ----
    private static async Task PrintSampleRowsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        Console.WriteLine("--- Dummy Data Preview ---");

        var orgs = await db.Organizations
            .OrderBy(x => x.OrgName)
            .Take(3)
            .Select(x => new { x.OrgName, x.Location, x.Status })
            .ToListAsync(cancellationToken);

        foreach (var org in orgs)
        {
            Console.WriteLine($"ORG: {org.OrgName} | {org.Location} | {org.Status}");
        }

        var events = await db.Events
            .OrderBy(x => x.EventName)
            .Take(3)
            .Select(x => new { x.EventName, x.Location, x.Status })
            .ToListAsync(cancellationToken);

        foreach (var ev in events)
        {
            Console.WriteLine($"EVENT: {ev.EventName} | {ev.Location} | {ev.Status}");
        }

        var categories = await db.EventCategories
            .OrderBy(x => x.MilestoneId)
            .ThenBy(x => x.CategoryName)
            .Take(3)
            .Select(x => new { x.CategoryName, x.OrderIndex })
            .ToListAsync(cancellationToken);

        foreach (var category in categories)
        {
            Console.WriteLine($"CATEGORY: {category.CategoryName} | Order {category.OrderIndex}");
        }

        var tasks = await db.Tasks
            .OrderBy(x => x.TaskName)
            .Take(3)
            .Select(x => new { x.TaskName, x.Priority, x.Status })
            .ToListAsync(cancellationToken);

        foreach (var task in tasks)
        {
            Console.WriteLine($"TASK: {task.TaskName} | {task.Priority} | {task.Status}");
        }
    }
}
