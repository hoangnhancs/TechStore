# 🧪 Test VectorService API

Write-Host "`n🧪 Testing VectorService`n" -ForegroundColor Cyan

$baseUrl = "http://localhost:8000"

# 1. Health Check
Write-Host "1️⃣  Health Check" -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/" -Method Get
    Write-Host "✅ Mode: $($health.mode)" -ForegroundColor Green
    Write-Host "   Model: $($health.model)" -ForegroundColor Cyan
    Write-Host "   Dimension: $($health.dimension)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Service not running. Start with: .\start.ps1" -ForegroundColor Red
    exit 1
}

# 2. Generate Product Embedding
Write-Host "`n2️⃣  Generate Product Embedding" -ForegroundColor Yellow
$productRequest = @{
    id = "test-tv-001"
    name = "Smart Tivi LG LED 4K 43 inch"
    description = "Bo xu ly AI, 4K, HDR10+"
    old_price = 11990000
    discount_percentage = 31
    category_name = "Tivi"
    brand_name = "LG"
    tags = @(
        @{name = "Kich co"; value = "43 inch"},
        @{name = "Do phan giai"; value = "4K"}
    )
}

try {
    $result = Invoke-RestMethod -Uri "$baseUrl/products/embed" -Method Post -Body ($productRequest | ConvertTo-Json -Depth 10) -ContentType "application/json; charset=utf-8"
    Write-Host "✅ Generated embedding for: $($result.product_id)" -ForegroundColor Green
    Write-Host "   Dimension: $($result.dimension)" -ForegroundColor Cyan
    Write-Host "   Text: $($result.text.Substring(0, [Math]::Min(60, $result.text.Length)))..." -ForegroundColor Cyan
    Write-Host "   First 5 vector values: $($result.embedding[0..4] -join ', ')" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   💡 SuggestionService would now store this in its DB" -ForegroundColor Magenta
} catch {
    Write-Host "❌ Failed: $_" -ForegroundColor Red
}

# 3. Simple Text Embedding
Write-Host "`n3️⃣  Generate Text Embedding" -ForegroundColor Yellow
$textRequest = @{text = "laptop gaming gia re"}

try {
    $textResult = Invoke-RestMethod -Uri "$baseUrl/embed" -Method Post -Body ($textRequest | ConvertTo-Json) -ContentType "application/json; charset=utf-8"
    Write-Host "✅ Generated text embedding" -ForegroundColor Green
    Write-Host "   Dimension: $($textResult.dimension)" -ForegroundColor Cyan
    Write-Host "   First 5 values: $($textResult.embedding[0..4] -join ', ')" -ForegroundColor Yellow
} catch {
    Write-Host "❌ Failed: $_" -ForegroundColor Red
}

Write-Host "`n✅ Test completed!`n" -ForegroundColor Green
Write-Host "📌 Note: This service ONLY generates vectors." -ForegroundColor Cyan
Write-Host "   Storage is the responsibility of SuggestionService." -ForegroundColor Gray
Write-Host ""
