// ---- Điểm khởi động frontend: đăng ký service, auth state và pipeline render ----
using Org.Frontend.Components;
using Org.Frontend.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// ---- Đọc URL backend từ cấu hình, fallback về localhost khi chưa cấu hình ----
var backendApiBaseUrl = builder.Configuration["BackendApi:BaseUrl"] ?? "http://localhost:5058";

// ---- Đăng ký Razor Components và bật chế độ interactive server ----
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ---- Tạo typed HttpClient cho module auth, mặc định nhận JSON ----
builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(backendApiBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ---- Đăng ký token storage + auth state provider dùng chung toàn app ----
builder.Services.AddScoped<ITokenStorage, LocalStorageTokenStorage>();
builder.Services.AddScoped<FrontendAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FrontendAuthStateProvider>());

// ---- Cấu hình cookie auth để route chưa đăng nhập chuyển về /login ----
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
    });

// ---- Bật authorization cho cả middleware và Blazor component ----
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationCore();

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

// ---- Bật xác thực/phân quyền trước khi map component ----
app.UseAuthentication();
app.UseAuthorization();

// ---- Bật antiforgery để giảm rủi ro CSRF cho request có state ----
app.UseAntiforgery();

// ---- Map static assets và root component của ứng dụng ----
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
