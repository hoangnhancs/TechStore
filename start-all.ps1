param(
    [switch]$OpenExternalWindow,
    [switch]$DryRun
)

$services = @(
    "ProductService",
    "IdentityService",
    "CartService",
    "OrderService",
    "PaymentService",
    "ReviewService",
    "CommentService",
    "SearchService",
    "RecommendationService",
    "NotificationService",
    "GatewayService"
)

foreach ($svc in $services) {
    $path = Join-Path $PSScriptRoot "server\$svc"

    if (-not (Test-Path $path)) {
        Write-Warning "Skipping $svc because path was not found: $path"
        continue
    }

    if ($OpenExternalWindow) {
        if ($DryRun) {
            Write-Host "[DryRun] Would start $svc in an external PowerShell window"
            continue
        }

        Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$path'; dotnet run"
        Write-Host "Started $svc in external PowerShell"
        continue
    }

    if ($DryRun) {
        Write-Host "[DryRun] Would start $svc in the current terminal from: $path"
        continue
    }

    $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory $path -NoNewWindow -PassThru
    Write-Host "Started $svc in current terminal (PID: $($process.Id))"
}
