// ---- Điểm khởi động frontend: đăng ký service và pipeline render ----
using Org.Frontend.Infrastructure.Startup;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFrontendApp(builder.Configuration);

var app = builder.Build();
app.UseFrontendApp();

app.Run();
