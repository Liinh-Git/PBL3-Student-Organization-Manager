using Org.Frontend.Components;
using Org.Frontend.Services.Auth;
using Org.Frontend.Services.Departments;
using Org.Frontend.Services.Members;
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
        services.AddScoped<FrontendAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FrontendAuthStateProvider>());
        services.AddAuthorizationCore();

        services.AddHttpClient<DepartmentApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddHttpClient<MemberApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddScoped<DepartmentMockService>();
        services.AddScoped<MemberMockService>();

        services.AddScoped<IDepartmentService>(sp =>
            useMockServices ? sp.GetRequiredService<DepartmentMockService>() : sp.GetRequiredService<DepartmentApiClient>());

        services.AddScoped<IMemberService>(sp =>
            useMockServices ? sp.GetRequiredService<MemberMockService>() : sp.GetRequiredService<MemberApiClient>());

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
        // Các Razor page được bảo vệ quà AuthorizeRouteView trong Routes.razor (không dùng [Authorize]).
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }
}
