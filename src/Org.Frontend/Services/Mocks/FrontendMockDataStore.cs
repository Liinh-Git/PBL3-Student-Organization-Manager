// ---- Kho dữ liệu mock cho FE — đọc file JSON và giữ trong memory khi chạy ----
// Singleton: dùng chung toàn bộ ứng dụng, tải một lần khi khởi động (WarmupAsync).
// Thread-safe: dùng SemaphoreSlim để tránh load nhiều lần đồng thời.
// Truy xuất qua UseAsync<T> để đảm bảo dữ liệu đã được load và lock đúng cách.
using System.Text.Json;
using Org.Frontend.Services.Mocks.Models;

namespace Org.Frontend.Services.Mocks;

public sealed class FrontendMockDataStore(IWebHostEnvironment env, ILogger<FrontendMockDataStore> logger)
{
    private readonly IWebHostEnvironment _env = env;
    private readonly ILogger<FrontendMockDataStore> _logger = logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly object _syncRoot = new();
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    private bool _isLoaded;
    private MockDataSet _data = new();

    public async Task<TResult> UseAsync<TResult>(Func<MockDataSet, TResult> action, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        await EnsureLoadedAsync(ct);

        lock (_syncRoot)
        {
            return action(_data);
        }
    }

    public Task WarmupAsync(CancellationToken ct = default) => EnsureLoadedAsync(ct);

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_isLoaded)
        {
            return;
        }

        await _loadLock.WaitAsync(ct);
        try
        {
            if (_isLoaded)
            {
                return;
            }

            _data = await LoadDataSetAsync(ct);
            MockDataValidator.Validate(_data);
            _isLoaded = true;

            _logger.LogInformation(
                "Loaded FE mock dataset: {Users} users, {Organizations} organizations, {Members} members, {Events} events, {Attendees} attendees.",
                _data.Users.Count,
                _data.Organizations.Count,
                _data.Members.Count,
                _data.Events.Count,
                _data.Attendees.Count);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private async Task<MockDataSet> LoadDataSetAsync(CancellationToken ct)
    {
        return new MockDataSet
        {
            Users = await ReadDomainAsync<MockUser>("users.mock.json", ct),
            Organizations = await ReadDomainAsync<MockOrganization>("organizations.mock.json", ct),
            Departments = await ReadDomainAsync<MockDepartment>("departments.mock.json", ct),
            Members = await ReadDomainAsync<MockMember>("members.mock.json", ct),
            Events = await ReadDomainAsync<MockEvent>("events.mock.json", ct),
            EventMembers = await ReadDomainAsync<MockEventMember>("event-members.mock.json", ct),
            Attendees = await ReadDomainAsync<MockAttendee>("attendees.mock.json", ct),
            Milestones = await ReadDomainAsync<MockMilestone>("milestones.mock.json", ct),
            EventCategories = await ReadDomainAsync<MockEventCategory>("event-categories.mock.json", ct),
            Tasks = await ReadDomainAsync<MockTask>("tasks.mock.json", ct)
        };
    }

    private async Task<List<T>> ReadDomainAsync<T>(string fileName, CancellationToken ct)
    {
        var path = Path.Combine(_env.ContentRootPath, "Services", "Mocks", "Data", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Mock data file not found: {path}");
        }

        await using var stream = File.OpenRead(path);
        var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions, ct);
        return data ?? [];
    }
}
