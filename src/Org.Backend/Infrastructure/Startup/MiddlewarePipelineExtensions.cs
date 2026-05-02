// ---- Gom middleware pipeline API để Program.cs ngắn gọn và dễ đọc ----
using FastEndpoints;
using Org.Backend.Hubs;
using Scalar.AspNetCore;
using FastEndpoints.Swagger;

namespace Org.Backend.Infrastructure.Startup;

public static class MiddlewarePipelineExtensions
{
    // ---- Cấu hình middleware cho web mode (HTTPS, CORS, AuthN/AuthZ, endpoints) ----
    public static WebApplication UseAppApiPipeline(this WebApplication app)
    {
        // ---- Chỉ bật OpenAPI trong môi trường phát triển ----
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.UseSwaggerGen();
        }

        // ---- Thứ tự middleware quan trọng để auth và CORS hoạt động đúng ----
        app.UseHttpsRedirection();
        app.UseCors("FrontendPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints();

        // ---- Map SignalR hub endpoint ----
        app.MapHub<NotificationHub>("/hubs/notifications");

        return app;
    }
}
