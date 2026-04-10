$services = @(
    "ProductService",
    "IdentityService",
    "CartService",
    "OrderService",
    "PaymentService",
    "ReviewService",
    "CommentService",
    "SearchService",
    "NotificationService",
    "GatewayService"
)

foreach ($svc in $services) {
    $path = Join-Path $PSScriptRoot "server\$svc"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$path'; dotnet run"
    Write-Host "Started $svc"
}
