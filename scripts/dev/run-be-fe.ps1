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

    Write-Host "Starting Backend and Frontend..."

    Start-Process -FilePath "powershell" -ArgumentList @(
        "-NoExit",
        "-Command",
        "Set-Location '$repoRoot'; dotnet run --project '$BackendProject'"
    ) | Out-Null

    Start-Process -FilePath "powershell" -ArgumentList @(
        "-NoExit",
        "-Command",
        "Set-Location '$repoRoot'; dotnet run --project '$FrontendProject'"
    ) | Out-Null

    Write-Host "Backend and Frontend started in separate PowerShell windows."
}
finally {
    Pop-Location
}
