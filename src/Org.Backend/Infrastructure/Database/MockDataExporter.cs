using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Enums;
using TaskStatus = Org.Shared.TaskStatus;

namespace Org.Backend.Infrastructure.Database;

public static class MockDataExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task ExportToJsonAsync(AppDbContext db, string outputFolder, CancellationToken ct = default)
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        Console.WriteLine($"Exporting database to JSON mocks in: {outputFolder}");

        var usersDb = await db.Users.AsNoTracking().ToListAsync(ct);
        var orgsDb = await db.Organizations.AsNoTracking().ToListAsync(ct);
        var deptsDb = await db.Departments.AsNoTracking().ToListAsync(ct);
        var membersDb = await db.Members.AsNoTracking().ToListAsync(ct);
        var eventsDb = await db.Events.AsNoTracking().ToListAsync(ct);
        var eventMembersDb = await db.EventMembers.AsNoTracking().ToListAsync(ct);
        var attendeesDb = await db.Attendees.AsNoTracking().ToListAsync(ct);
        var milestonesDb = await db.Milestones.AsNoTracking().ToListAsync(ct);
        var categoriesDb = await db.EventCategories.AsNoTracking().ToListAsync(ct);
        var tasksDb = await db.Tasks.AsNoTracking().ToListAsync(ct);
        var requestsDb = await db.Requests.AsNoTracking().ToListAsync(ct);
        var assetsDb = await db.DigitalAssets.AsNoTracking().ToListAsync(ct);

        var userDisplayNames = usersDb.ToDictionary(x => x.Id, x => x.FullName);

        // Users
        var users = usersDb.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.PasswordHash,
            u.AvatarUrl,
            u.PhoneNumber,
            DateOfBirth = u.Dob.HasValue ? DateOnly.FromDateTime(u.Dob.Value) : (DateOnly?)null,
            u.Gender,
            u.Address,
            u.Bio,
            Status = u.Status.ToString(),
            EmailNotificationsEnabled = true,
            AppPushEnabled = true,
            SmsAlertsEnabled = false,
            UpdatedAt = u.UpdatedAt
        }).ToList();
        await WriteFileAsync(outputFolder, "users.mock.json", users, ct);

        // Organizations
        var orgs = orgsDb.Select(o => new
        {
            o.Id,
            Code = o.OrgName.Replace(" ", "").ToUpper(),
            o.OrgName,
            o.Description,
            o.AvatarUrl,
            o.CoverUrl,
            o.Location,
            o.TotalMembers,
            Status = (int)o.Status
        }).ToList();
        await WriteFileAsync(outputFolder, "organizations.mock.json", orgs, ct);

        // Departments
        var depts = deptsDb.Select(d => new
        {
            d.Id,
            d.OrgId,
            d.DeptName,
            d.ManagerId,
            d.Function
        }).ToList();
        await WriteFileAsync(outputFolder, "departments.mock.json", depts, ct);

        // Members
        var members = membersDb.Select(m => new
        {
            m.Id,
            m.OrgId,
            m.UserId,
            DisplayName = userDisplayNames.TryGetValue(m.UserId, out var displayName) ? displayName : "Unknown",
            m.DepartmentId,
            m.RoleId,
            m.JoinDate
        }).ToList();
        await WriteFileAsync(outputFolder, "members.mock.json", members, ct);

        // Events
        var events = eventsDb.Select(e => new
        {
            e.Id,
            e.OrgId,
            Name = e.EventName,
            e.Description,
            StartDate = DateOnly.FromDateTime(e.StartDate),
            EndDate = DateOnly.FromDateTime(e.EndDate),
            StatusLabel = e.Status.ToString().ToUpper(),
            e.Location,
            TotalSlots = e.TargetParticipants,
            ImageUrl = $"/images/mockimages/event-{(Math.Abs(e.Id.GetHashCode()) % 3) + 1}.jpg",
            CompletionLabel = string.Empty,
            BudgetLabel = e.Budget.ToString("N0"),
            RiskLevel = "LOW",
            TotalFiles = assetsDb.Count(a => a.EventId == e.Id),
            ActualSpending = 0m
        }).ToList();
        await WriteFileAsync(outputFolder, "events.mock.json", events, ct);

        // EventMembers
        var eventMembers = eventMembersDb.Select(em => new
        {
            em.EventId,
            em.MemberId,
            em.EventRole
        }).ToList();
        await WriteFileAsync(outputFolder, "event-members.mock.json", eventMembers, ct);

        // Attendees
        var attendees = attendeesDb.Select(a => new
        {
            a.Id,
            a.EventId,
            a.UserId,
            a.GuestName,
            a.Email,
            a.TicketType,
            a.CheckInTime,
            Status = a.Status.ToString().ToUpper(),
            a.CreatedAt
        }).ToList();
        await WriteFileAsync(outputFolder, "attendees.mock.json", attendees, ct);

        // Milestones
        var milestones = milestonesDb.Select(m => new
        {
            m.Id,
            m.EventId,
            Name = m.Title,
            m.OrderIndex,
            m.StartDate,
            m.EndDate
        }).ToList();
        await WriteFileAsync(outputFolder, "milestones.mock.json", milestones, ct);

        // EventCategories
        var categories = categoriesDb.Select(c => new
        {
            c.Id,
            c.MilestoneId,
            Name = c.CategoryName,
            LeadMemberId = (Guid?)null,
            c.OrderIndex,
            c.Description,
            c.OwnerDepartmentId,
            IsUrgent = false,
            Guidelines = Array.Empty<string>()
        }).ToList();
        await WriteFileAsync(outputFolder, "event-categories.mock.json", categories, ct);

        // Tasks
        var tasks = tasksDb.Select(t => new
        {
            t.Id,
            CategoryId = t.EventCategoryId,
            Title = t.TaskName,
            Status = t.Status switch
            {
                TaskStatus.InProgress => "IN_PROGRESS",
                TaskStatus.Done => "DONE",
                _ => "TODO"
            },
            AssigneeMemberId = t.AssigneeId,
            DueDate = t.Deadline,
            Note = t.Note
        }).ToList();
        await WriteFileAsync(outputFolder, "tasks.mock.json", tasks, ct);

        // Requests
        var requests = requestsDb.Select(r => new
        {
            r.Id,
            r.OrgId,
            UserId = r.SenderId,
            RequestType = r.RequestType == RequestType.JoinClub ? "JOIN" : "OTHER",
            Status = r.Status.ToString().ToUpper(),
            CreatedAt = r.RequestDate,
            Title = $"Request from User {r.SenderId}",
            Message = r.Content
        }).ToList();
        await WriteFileAsync(outputFolder, "requests.mock.json", requests, ct);

        // DigitalAssets
        var assets = assetsDb.Select(a => new
        {
            a.Id,
            a.EventId,
            a.FileName,
            a.FileUrl,
            FileType = a.FileType.ToString().ToUpper(),
            a.UploadedBy,
            a.CreatedAt
        }).ToList();
        await WriteFileAsync(outputFolder, "digital-assets.mock.json", assets, ct);

        Console.WriteLine("Export completed successfully.");
    }

    private static async Task WriteFileAsync(string folder, string fileName, object data, CancellationToken ct)
    {
        var path = Path.Combine(folder, fileName);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(path, json, ct);
        Console.WriteLine($" - Wrote {fileName}");
    }
}