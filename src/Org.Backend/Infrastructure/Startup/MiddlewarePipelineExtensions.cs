// ---- Gom middleware pipeline API để Program.cs ngắn gọn và dễ đọc ----
using FastEndpoints;

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
        }

        // ---- Thứ tự middleware quan trọng để auth và CORS hoạt động đúng ----
        app.UseHttpsRedirection();
        app.UseCors("FrontendPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints();

        return app;
    }
}
