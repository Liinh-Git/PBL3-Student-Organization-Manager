// ---- Entry point: chỉ orchestration, logic chi tiết nằm trong Infrastructure/Startup ----
using Org.Backend.Infrastructure.Startup;

// ---- Nạp biến môi trường từ file .env (nếu có) trước khi tạo builder ----
DotEnvLoader.LoadIfExists();

// ---- Bước 1: tạo builder và xác định có chạy seed mode hay không ----
var builder = WebApplication.CreateBuilder(args);
var isSeedMode = args.Contains("--seed", StringComparer.OrdinalIgnoreCase);

// ---- Bước 2: đăng ký service lõi + runtime API (runtime sẽ bỏ qua khi seed mode) ----
builder.Services
    .AddAppCoreServices(builder)
    .AddAppApiRuntime(isSeedMode);

// ---- Bước 3: build ứng dụng ----
var app = builder.Build();

// ---- Bước 4: nếu chạy seed mode thì migrate + seed + thoát ngay ----
if (await SeedModeRunner.TryRunAsync(app, isSeedMode))
{
    return;
}

// ---- Bước 5: web mode bình thường, nạp middleware pipeline và chạy server ----
app.UseAppApiPipeline();
app.Run();
