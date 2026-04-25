param(
    [string]$BackendProject = "src/Org.Backend",
    [string]$FrontendProject = "src/Org.Frontend"
)

$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    # Always run from repo root so relative paths remain stable.
    Set-Location "../.."
    $repoRoot = (Get-Location).Path

    Write-Host "Building solution once to avoid concurrent build locks..."
    dotnet build "StudentOrgManager.slnx"
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed. Please inspect the output above."
    }

    Write-Host "Starting Backend and Frontend..."

    Start-Process -FilePath "powershell" -ArgumentList @(
        "-NoExit",
        "-Command",
        "Set-Location '$repoRoot'; dotnet run --no-build --project '$BackendProject'"
    ) | Out-Null

    Start-Process -FilePath "powershell" -ArgumentList @(
        "-NoExit",
        "-Command",
        "Set-Location '$repoRoot'; dotnet run --no-build --project '$FrontendProject'"
    ) | Out-Null

    Write-Host "Backend and Frontend started in separate PowerShell windows."
}
finally {
    Pop-Location
}
