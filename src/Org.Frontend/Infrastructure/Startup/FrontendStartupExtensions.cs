// ---- Extension startup cho Org.Frontend: đăng ký DI và cấu hình middleware ----
// AddFrontendApp: đăng ký Blazor, MudBlazor, Auth services và domain services.
// UseFrontendApp: warm-up mock (nếu dùng mock), cấu hình pipeline Blazor Server.
using Org.Frontend.Components;
using Org.Frontend.Services.Auth;
using Org.Frontend.Services.Dashboard;
using Org.Frontend.Services.Departments;
using Org.Frontend.Services.Events;
using Org.Frontend.Services.Members;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Organizations;
using Org.Frontend.Services.Tasks;
using Org.Frontend.Services.UserSettings;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using System.Net.Http.Headers;

namespace Org.Frontend.Infrastructure.Startup;

public static class FrontendStartupExtensions
{
    // ---- Helper tái sử dụng: cấu hình HttpClient chuẩn với BaseUrl và accept JSON ----
    private static void ConfigureApiClient(HttpClient client, string baseUrl)
    {
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // ---- Đăng ký toàn bộ service cần thiết cho ứng dụng FE ----
    public static IServiceCollection AddFrontendApp(this IServiceCollection services, IConfiguration configuration)
    {
        var backendApiBaseUrl = configuration["BackendApi:BaseUrl"] ?? "http://localhost:5058";
        var useMockServices = configuration.GetValue<bool?>("FrontendData:UseMockServices") ?? true;

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddMudServices();

        // ---- Đăng ký HttpClient có auth cho các API client (chỉ khi không dùng mock) ----
        if (!useMockServices)
        {
            services.AddHttpClient<AuthApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl));
        }

        // ---- Đăng ký các token/auth service (dùng cho cả mock lẫn real) ----
        services.AddScoped<ITokenStorage, LocalStorageTokenStorage>();
        services.AddScoped<IAccessTokenStore, AccessTokenStore>();
        services.AddScoped<FrontendAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FrontendAuthStateProvider>());
        services.AddAuthorizationCore();
        services.AddTransient<AuthHeaderDelegatingHandler>();

        // ---- Đăng ký HttpClient có Bearer token header cho các domain client ----
        if (!useMockServices)
        {
            services.AddHttpClient<DepartmentApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<MemberApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<OrganizationApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<EventApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<MilestoneApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<EventCategoryApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<TaskApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

                services.AddHttpClient<UserDashboardApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<UserSettingsApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();
        }

        // ---- Mock services (Singleton/Scoped) — luôn đăng ký để DI có thể resolve ----
        services.AddSingleton<FrontendMockDataStore>();
        services.AddSingleton<AuthMockService>();
        services.AddScoped<DepartmentMockService>();
        services.AddScoped<MemberMockService>();
        services.AddScoped<EventMockService>();
        services.AddScoped<MilestoneMockService>();
        services.AddScoped<EventCategoryMockService>();
        services.AddScoped<TaskMockService>();
        services.AddScoped<UserDashboardMockService>();
        services.AddScoped<UserSettingsMockService>();
        services.AddScoped<MockOrganizationContext>();

        // ---- Chọn implementation thực hoặc mock theo cấu hình FrontendData:UseMockServices ----
        services.AddScoped<IAuthService>(sp =>
            useMockServices ? sp.GetRequiredService<AuthMockService>() : sp.GetRequiredService<AuthApiClient>());

        services.AddScoped<IOrganizationContext>(sp =>
            useMockServices ? sp.GetRequiredService<MockOrganizationContext>() : sp.GetRequiredService<OrganizationApiClient>());

        services.AddScoped<IDepartmentService>(sp =>
            useMockServices ? sp.GetRequiredService<DepartmentMockService>() : sp.GetRequiredService<DepartmentApiClient>());

        services.AddScoped<IMemberService>(sp =>
            useMockServices ? sp.GetRequiredService<MemberMockService>() : sp.GetRequiredService<MemberApiClient>());

        services.AddScoped<IEventService>(sp =>
            useMockServices ? sp.GetRequiredService<EventMockService>() : sp.GetRequiredService<EventApiClient>());

        services.AddScoped<IMilestoneService>(sp =>
            useMockServices ? sp.GetRequiredService<MilestoneMockService>() : sp.GetRequiredService<MilestoneApiClient>());

        services.AddScoped<IEventCategoryService>(sp =>
            useMockServices ? sp.GetRequiredService<EventCategoryMockService>() : sp.GetRequiredService<EventCategoryApiClient>());

        services.AddScoped<ITaskService>(sp =>
            useMockServices ? sp.GetRequiredService<TaskMockService>() : sp.GetRequiredService<TaskApiClient>());

        services.AddScoped<IUserDashboardService>(sp =>
            useMockServices ? sp.GetRequiredService<UserDashboardMockService>() : sp.GetRequiredService<UserDashboardApiClient>());

        services.AddScoped<IUserSettingsService>(sp =>
            useMockServices ? sp.GetRequiredService<UserSettingsMockService>() : sp.GetRequiredService<UserSettingsApiClient>());

        return services;
    }

    // ---- Cấu hình middleware pipeline cho Blazor Server ----
    // Auth được xử lý ở tầng Blazor component (CascadingAuthenticationState + AuthorizeRouteView)
    // không cần UseAuthentication/UseAuthorization middleware.
    public static WebApplication UseFrontendApp(this WebApplication app)
    {
        var useMockServices = app.Configuration.GetValue<bool?>("FrontendData:UseMockServices") ?? true;
        if (useMockServices)
        {
            using var scope = app.Services.CreateScope();
            var mockStore = scope.ServiceProvider.GetRequiredService<FrontendMockDataStore>();
            mockStore.WarmupAsync().GetAwaiter().GetResult();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

        // ---- Blazor Server dùng CascadingAuthenticationState + AuthorizeRouteView ----
        // Auth được xử lý ở tầng Blazor component, không cần UseAuthentication/UseAuthorization middleware.
        // Các Razor page được bảo vệ qua AuthorizeRouteView trong Routes.razor (không dùng [Authorize]).
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }
}
