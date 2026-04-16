using Org.Frontend.Components;
using Org.Frontend.Services.Auth;
using Org.Frontend.Services.Departments;
using Org.Frontend.Services.EventCategories;
using Org.Frontend.Services.Events;
using Org.Frontend.Services.Milestones;
using Org.Frontend.Services.Members;
using Org.Frontend.Services.Organizations;
using Org.Frontend.Services.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace Org.Frontend.Infrastructure.Startup;

public static class FrontendStartupExtensions
{
    public static IServiceCollection AddFrontendApp(this IServiceCollection services, IConfiguration configuration)
    {
        var backendApiBaseUrl = configuration["BackendApi:BaseUrl"] ?? "http://localhost:5058";
        var useMockServices = configuration.GetValue<bool?>("FrontendData:UseMockServices") ?? true;

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddHttpClient<AuthApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddScoped<ITokenStorage, LocalStorageTokenStorage>();
        services.AddScoped<IAccessTokenStore, AccessTokenStore>();
        services.AddScoped<FrontendAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FrontendAuthStateProvider>());
        services.AddAuthorizationCore();
        services.AddTransient<AuthHeaderDelegatingHandler>();

        services.AddHttpClient<DepartmentApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

        services.AddHttpClient<MemberApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

        services.AddHttpClient<OrganizationApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

        services.AddHttpClient<EventApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

        services.AddHttpClient<MilestoneApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

        services.AddHttpClient<EventCategoryApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

        services.AddHttpClient<TaskApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AuthHeaderDelegatingHandler>();

        services.AddScoped<DepartmentMockService>();
        services.AddScoped<MemberMockService>();
        services.AddScoped<EventMockService>();
        services.AddScoped<MilestoneMockService>();
        services.AddScoped<EventCategoryMockService>();
        services.AddScoped<TaskMockService>();
        services.AddScoped<MockOrganizationContext>();

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

        return services;
    }

    public static WebApplication UseFrontendApp(this WebApplication app)
    {
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
