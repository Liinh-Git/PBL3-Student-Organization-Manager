using Org.Frontend.Components;
using Org.Frontend.Services.Departments;
using Org.Frontend.Services.Members;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// ---- Đọc URL backend từ cấu hình, fallback về localhost khi chưa cấu hình ----
var backendApiBaseUrl = builder.Configuration["BackendApi:BaseUrl"] ?? "http://localhost:5058";
var useMockServices = builder.Configuration.GetValue<bool?>("FrontendData:UseMockServices") ?? true;

// ---- Đăng ký Razor Components và bật chế độ interactive server ----
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ---- Tạo typed HttpClient cho module departments/members khi chạy API mode ----
builder.Services.AddHttpClient<DepartmentApiClient>(client =>
{
    client.BaseAddress = new Uri(backendApiBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<MemberApiClient>(client =>
{
    client.BaseAddress = new Uri(backendApiBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ---- FE1 data mode: mock trước, có thể bật API bằng cấu hình ----
builder.Services.AddScoped<DepartmentMockService>();
builder.Services.AddScoped<MemberMockService>();

builder.Services.AddScoped<IDepartmentService>(sp =>
    useMockServices ? sp.GetRequiredService<DepartmentMockService>() : sp.GetRequiredService<DepartmentApiClient>());

builder.Services.AddScoped<IMemberService>(sp =>
    useMockServices ? sp.GetRequiredService<MemberMockService>() : sp.GetRequiredService<MemberApiClient>());

var app = builder.Build();

// ---- Pipeline production: xử lý lỗi tập trung + HSTS + HTTPS ----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // ---- Mặc định HSTS 30 ngày, có thể tăng theo chính sách bảo mật khi release ----
    app.UseHsts();
    app.UseHttpsRedirection();
}

// ---- Nếu route không tồn tại thì chuyển sang trang not-found ----
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// ---- Bật antiforgery để giảm rủi ro CSRF cho request có state ----
app.UseAntiforgery();

// ---- Map static assets và root component của ứng dụng ----
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
