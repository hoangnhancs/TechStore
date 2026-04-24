$ErrorActionPreference = "Stop"

# ── Helpers ──────────────────────────────────────────────────────────────────

function Get-WebApiProjects {
    param([string]$SlnxPath, [string]$RootDir)

    [xml]$slnx = Get-Content $SlnxPath
    $allProjects = $slnx.Solution.Folder.Project | Select-Object -ExpandProperty Path

    $webApiProjects = @()
    foreach ($relPath in $allProjects) {
        $csprojPath = Join-Path $RootDir $relPath.Replace("/", "\")
        if (-not (Test-Path $csprojPath)) { continue }

        $content = Get-Content $csprojPath -Raw
        # WebAPI dùng Microsoft.NET.Sdk.Web
        if ($content -match 'Sdk\s*=\s*"Microsoft\.NET\.Sdk\.Web"') {
            $webApiProjects += [PSCustomObject]@{
                Name   = [System.IO.Path]::GetFileNameWithoutExtension($csprojPath)
                Folder = Split-Path (Split-Path $csprojPath -Parent) -Leaf
                Path   = Split-Path $csprojPath -Parent
            }
        }
    }
    return $webApiProjects
}

function Compare-ScriptVsSolution {
    param(
        [string[]]$ScriptServices,
        [PSCustomObject[]]$SolutionWebApis
    )

    $solutionNames = $SolutionWebApis | Select-Object -ExpandProperty Name

    $inScriptNotSolution = $ScriptServices | Where-Object { $_ -notin $solutionNames }
    $inSolutionNotScript = $solutionNames  | Where-Object { $_ -notin $ScriptServices }

    if ($inScriptNotSolution.Count -gt 0) {
        Write-Host "`n[WARN] In script but NOT in solution:" -ForegroundColor Yellow
        $inScriptNotSolution | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    }
    if ($inSolutionNotScript.Count -gt 0) {
        Write-Host "`n[WARN] WebAPI in solution but MISSING from script:" -ForegroundColor Magenta
        $inSolutionNotScript | ForEach-Object { Write-Host "  - $_" -ForegroundColor Magenta }
    }
    if ($inScriptNotSolution.Count -eq 0 -and $inSolutionNotScript.Count -eq 0) {
        Write-Host "`n[OK] Script services match solution WebAPI projects." -ForegroundColor Green
    }
}

# ── Config ───────────────────────────────────────────────────────────────────

$services = @(
    "ProductService",
    "IdentityService",
    "CartService",
    "OrderService",
    "PaymentService",
    "ReviewService",
    "CommentService",
    "NotificationService"
)

$serverRoot  = Join-Path $PSScriptRoot "server"
$slnxFile    = Join-Path $PSScriptRoot "TechStore.slnx"
$failedServices = @()

# ── Pre-check: so sánh script vs solution ───────────────────────────────────

if (Test-Path $slnxFile) {
    Write-Host "=== Checking WebAPI projects in solution ===" -ForegroundColor Cyan
    $webApis = Get-WebApiProjects -SlnxPath $slnxFile -RootDir $PSScriptRoot
    Write-Host "Found $($webApis.Count) WebAPI project(s) in solution:"
    $webApis | ForEach-Object { Write-Host "  - $($_.Name)" }

    Compare-ScriptVsSolution -ScriptServices $services -SolutionWebApis $webApis
    Write-Host ""
}
else {
    Write-Host "[WARN] .slnx file not found, skipping solution check." -ForegroundColor Yellow
}

# ── Main: update databases ───────────────────────────────────────────────────

foreach ($service in $services) {
    $servicePath = Join-Path $serverRoot $service

    if (-not (Test-Path $servicePath)) {
        Write-Host "[SKIP] $service - folder not found: $servicePath" -ForegroundColor Yellow
        continue
    }

    Write-Host "`n=== Updating database for $service ===" -ForegroundColor Cyan

    Push-Location $servicePath
    try {
        dotnet ef database update
        if ($LASTEXITCODE -ne 0) { throw "exited with code $LASTEXITCODE" }
        Write-Host "[OK] $service" -ForegroundColor Green
    }
    catch {
        Write-Host "[WARN] First update failed, attempting migration..." -ForegroundColor Yellow

        $vnTime  = [System.TimeZoneInfo]::ConvertTimeBySystemTimeZoneId([DateTime]::UtcNow, "SE Asia Standard Time")
        $dateStr = $vnTime.ToString("yyyyMMdd_HHmmss")
        $migrationName = "Update${service}${dateStr}"

        try {
            Write-Host "[INFO] Adding migration: $migrationName" -ForegroundColor Cyan
            dotnet ef migrations add $migrationName
            if ($LASTEXITCODE -ne 0) { throw "migrations add exited with code $LASTEXITCODE" }

            dotnet ef database update
            if ($LASTEXITCODE -ne 0) { throw "database update after migration exited with code $LASTEXITCODE" }

            Write-Host "[OK] $service (after migration)" -ForegroundColor Green
        }
        catch {
            $failedServices += $service
            Write-Host "[FAILED] $service - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    finally {
        Pop-Location
    }
}

# ── Summary ──────────────────────────────────────────────────────────────────

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
if ($failedServices.Count -eq 0) {
    Write-Host "All database updates completed successfully." -ForegroundColor Green
    exit 0
}
Write-Host "Failed services: $($failedServices -join ', ')" -ForegroundColor Red
exit 1