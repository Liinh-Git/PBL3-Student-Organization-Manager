# ---- Script đồng bộ database local: migrate + seed dữ liệu mẫu ----
param(
    # Đường dẫn project backend (cho phép override khi cần).
    [string]$ProjectPath = "src/Org.Backend"
)

# Dừng ngay khi gặp lỗi để tránh trạng thái nửa chừng.
$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    # Luôn chạy từ root repo để đường dẫn tương đối ổn định.
    Set-Location "../.."

    Write-Host "[1/3] Checking dotnet-ef..."
    # Kiểm tra dotnet-ef trước để báo lỗi sớm, dễ xử lý.
    dotnet ef --version | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet-ef is not available. Install it with: dotnet tool install --global dotnet-ef"
    }

    Set-Location $ProjectPath

    Write-Host "[2/3] Applying migrations..."
    # Đồng bộ schema DB theo migration mới nhất.
    dotnet ef database update
    if ($LASTEXITCODE -ne 0) {
        throw "Migration failed. Please verify connection string and migration state."
    }

    Write-Host "[3/3] Seeding sample data..."
    # Seed mode: tạo dữ liệu mẫu rồi thoát, không khởi động web server.
    dotnet run -- --seed
    if ($LASTEXITCODE -ne 0) {
        throw "Seed mode failed. Please inspect the output above."
    }

    Write-Host "Database sync completed successfully."
}
finally {
    Pop-Location
}
