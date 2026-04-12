$ErrorActionPreference = "Stop"

$services = @(
    "ProductService",
    "IdentityService",
    "CartService",
    "OrderService",
    "PaymentService",
    "ReviewService",
    "CommentService"
)

$serverRoot = Join-Path $PSScriptRoot "server"
$failedServices = @()

foreach ($service in $services) {
    $servicePath = Join-Path $serverRoot $service

    if (-not (Test-Path $servicePath)) {
        Write-Host "[SKIP] $service - service folder not found: $servicePath" -ForegroundColor Yellow
        continue
    }

    Write-Host "`n=== Updating database for $service ===" -ForegroundColor Cyan

    Push-Location $servicePath
    try {
        dotnet ef database update
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet ef exited with code $LASTEXITCODE"
        }

        Write-Host "[OK] $service" -ForegroundColor Green
    }
    catch {
        $failedServices += $service
        Write-Host "[FAILED] $service - $($_.Exception.Message)" -ForegroundColor Red
    }
    finally {
        Pop-Location
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
if ($failedServices.Count -eq 0) {
    Write-Host "All database updates completed successfully." -ForegroundColor Green
    exit 0
}

Write-Host "Failed services: $($failedServices -join ', ')" -ForegroundColor Red
exit 1
