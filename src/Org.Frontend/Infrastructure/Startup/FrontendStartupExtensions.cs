// ---- Extension startup cho Org.Frontend: đăng ký DI và cấu hình middleware ----
// AddFrontendApp: đăng ký Blazor, MudBlazor, Auth services và domain services.
// UseFrontendApp: warm-up mock (nếu dùng mock), cấu hình pipeline Blazor Server.
using Org.Frontend.Components;
using Org.Frontend.Services.Auth;
using Org.Frontend.Services.Dashboard;
using Org.Frontend.Services.Discover;
using Org.Frontend.Services.Departments;
using Org.Frontend.Services.Events;
using Org.Frontend.Services.Members;
using Org.Frontend.Services.Messages;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Notifications;
using Org.Frontend.Services.Overview;
using Org.Frontend.Services.Organizations;
using Org.Frontend.Services.Posts;
using Org.Frontend.Services.Requests;
using Org.Frontend.Services.SignalR;
using Org.Frontend.Services.Tasks;
using Org.Frontend.Services.UserSettings;
using Org.Frontend.Services.Friends;
using Org.Frontend.Infrastructure.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
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

        // ---- Đăng ký các token/auth service (dùng cho cả mock lẫn real) ----
        services.AddScoped<ITokenStorage, LocalStorageTokenStorage>();
        services.AddScoped<IAccessTokenStore, AccessTokenStore>();
        services.AddScoped<FrontendAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FrontendAuthStateProvider>());
        services.AddAuthorizationCore();
        services.AddSingleton<CircuitServicesAccessor>();
        services.AddScoped<CircuitHandler, CircuitServicesAccessorHandler>();
        services.AddTransient<AuthHeaderDelegatingHandler>();
        services.AddHttpClient("BackendApi", c => ConfigureApiClient(c, backendApiBaseUrl));

        // ---- Đăng ký HttpClient cho các API client (chỉ khi không dùng mock) ----
        if (!useMockServices)
        {
            // AuthApiClient khong dung AuthHeaderDelegatingHandler vi no la noi lay token
            services.AddHttpClient<AuthApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl));

            services.AddScoped<IAuthenticatedBackendClient, AuthenticatedBackendClient>();
            services.AddScoped<DepartmentApiClient>();
            services.AddScoped<MemberApiClient>();
            services.AddScoped<OrganizationApiClient>();
            services.AddScoped<OrganizationServiceApiClient>();
            services.AddScoped<EventApiClient>();
            services.AddScoped<MilestoneApiClient>();
            services.AddScoped<EventCategoryApiClient>();
            services.AddScoped<TaskApiClient>();
            services.AddScoped<UserDashboardApiClient>();
            services.AddScoped<UserSettingsApiClient>();
            services.AddScoped<NotificationService>();
            services.AddScoped<RequestApiClient>();
            services.AddScoped<OrganizationRoleApiClient>();

            // Non-core clients can keep temporary handler path until migrated.
            services.AddHttpClient<MessageApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<DiscoverApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<UserProfileApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<OverviewApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
                    .AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

            services.AddHttpClient<FriendApiClient>(c => ConfigureApiClient(c, backendApiBaseUrl))
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
        services.AddScoped<UserProfileMockService>();
        services.AddScoped<MockOrganizationContext>();
        services.AddScoped<RequestMockService>();
        services.AddScoped<OrganizationMockService>();
        services.AddScoped<OrganizationRoleMockService>();
        services.AddScoped<NotificationMockService>();
        services.AddScoped<FriendMockService>();
        services.AddScoped<DiscoverMockService>();
        services.AddScoped<OverviewMockService>();
        services.AddScoped<PostMockService>();
        services.AddScoped<MessageMockService>();
        services.AddScoped<MessageStateBridge>();

        // ---- Chọn implementation thực hoặc mock theo cấu hình FrontendData:UseMockServices ----
        services.AddScoped<IAuthService>(sp =>
            useMockServices ? sp.GetRequiredService<AuthMockService>() : sp.GetRequiredService<AuthApiClient>());

        services.AddScoped<IOrganizationService>(sp =>
            useMockServices ? sp.GetRequiredService<OrganizationMockService>() : sp.GetRequiredService<OrganizationServiceApiClient>());

        services.AddScoped<IOrganizationContext>(sp =>
            useMockServices ? sp.GetRequiredService<MockOrganizationContext>() : sp.GetRequiredService<OrganizationApiClient>());

        services.AddScoped<IOrganizationRouteAccessGate, OrganizationRouteAccessGate>();

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

        services.AddScoped<INotificationService>(sp =>
            useMockServices ? sp.GetRequiredService<NotificationMockService>() : sp.GetRequiredService<NotificationService>());

        services.AddScoped<ISignalRService, SignalRService>();

        services.AddScoped<IRequestService>(sp =>
            useMockServices ? sp.GetRequiredService<RequestMockService>() : sp.GetRequiredService<RequestApiClient>());

        services.AddScoped<IOrganizationRoleService>(sp =>
            useMockServices ? sp.GetRequiredService<OrganizationRoleMockService>() : sp.GetRequiredService<OrganizationRoleApiClient>());

        services.AddScoped<IUserProfileService>(sp =>
            useMockServices ? sp.GetRequiredService<UserProfileMockService>() : sp.GetRequiredService<UserProfileApiClient>());

        services.AddScoped<IFriendService>(sp =>
            useMockServices ? sp.GetRequiredService<FriendMockService>() : sp.GetRequiredService<FriendApiClient>());

        services.AddScoped<IDiscoverService>(sp =>
            useMockServices ? sp.GetRequiredService<DiscoverMockService>() : sp.GetRequiredService<DiscoverApiClient>());

        services.AddScoped<IOverviewService>(sp =>
            useMockServices ? sp.GetRequiredService<OverviewMockService>() : sp.GetRequiredService<OverviewApiClient>());

        services.AddScoped<IPostService>(sp =>
            sp.GetRequiredService<PostMockService>());

        services.AddScoped<IMessageService>(sp =>
            useMockServices ? sp.GetRequiredService<MessageMockService>() : sp.GetRequiredService<MessageApiClient>());

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

