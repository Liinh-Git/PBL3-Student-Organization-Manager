# ---- Cleanup local build artifacts to keep workspace lightweight ----
param(
    [string]$RootPath = "."
)

$ErrorActionPreference = "Stop"

function Get-DirectorySizeBytes {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return 0
    }

    return (Get-ChildItem -Path $Path -Recurse -File -Force -ErrorAction SilentlyContinue |
        Measure-Object -Property Length -Sum).Sum
}

function Remove-MatchedDirectories {
    param(
        [string]$BasePath,
        [string[]]$DirectoryNames
    )

    $removed = @()
    $freedBytes = 0

    foreach ($name in $DirectoryNames) {
        $dirs = Get-ChildItem -Path $BasePath -Directory -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -ieq $name }

        foreach ($dir in $dirs) {
            $size = Get-DirectorySizeBytes -Path $dir.FullName
            Remove-Item -Path $dir.FullName -Recurse -Force -ErrorAction SilentlyContinue
            $removed += $dir.FullName
            $freedBytes += $size
        }
    }

    return [pscustomobject]@{
        Removed = $removed
        FreedBytes = $freedBytes
    }
}

Push-Location $RootPath
try {
    $absRoot = (Get-Location).Path
    Write-Host "Cleaning local artifacts under: $absRoot"

    $summary = @()

    $buildOutput = Remove-MatchedDirectories -BasePath $absRoot -DirectoryNames @("bin", "obj")
    $summary += [pscustomobject]@{ Group = "bin/obj"; Count = $buildOutput.Removed.Count; FreedBytes = $buildOutput.FreedBytes }

    $vsPath = Join-Path $absRoot ".vs"
    $vsFreed = Get-DirectorySizeBytes -Path $vsPath
    if (Test-Path $vsPath) {
        Remove-Item -Path $vsPath -Recurse -Force -ErrorAction SilentlyContinue
        $summary += [pscustomobject]@{ Group = ".vs"; Count = 1; FreedBytes = $vsFreed }
    }
    else {
        $summary += [pscustomobject]@{ Group = ".vs"; Count = 0; FreedBytes = 0 }
    }

    $testOutput = Remove-MatchedDirectories -BasePath $absRoot -DirectoryNames @("TestResults")
    $summary += [pscustomobject]@{ Group = "TestResults"; Count = $testOutput.Removed.Count; FreedBytes = $testOutput.FreedBytes }

    $totalFreed = ($summary | Measure-Object -Property FreedBytes -Sum).Sum

    Write-Host ""
    Write-Host "Cleanup summary:"
    $summary | ForEach-Object {
        $mb = [Math]::Round($_.FreedBytes / 1MB, 2)
        Write-Host ("- {0}: removed {1} folder(s), freed {2} MB" -f $_.Group, $_.Count, $mb)
    }

    Write-Host ""
    Write-Host ("Total freed: {0} MB" -f [Math]::Round($totalFreed / 1MB, 2))
    Write-Host "Done."
}
finally {
    Pop-Location
}
