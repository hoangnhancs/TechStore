# Start VectorService - Pure ML Mode (No DB, No RabbitMQ)
Write-Host "`n🚀 Starting VectorService`n" -ForegroundColor Green
Write-Host "✓ ML Model: ENABLED (all-MiniLM-L6-v2)" -ForegroundColor Green
Write-Host "✗ Database: NOT NEEDED (stateless)" -ForegroundColor Yellow
Write-Host "✗ RabbitMQ: NOT NEEDED (API-only)" -ForegroundColor Yellow
Write-Host ""
Write-Host "📌 Pure computation service - only generates vectors!" -ForegroundColor Cyan
Write-Host "   SuggestionService calls API and stores vectors." -ForegroundColor Gray
Write-Host ""

# Check if running from correct directory
if (-not (Test-Path "main.py")) {
    Write-Host "❌ Error: Run this script from VectorService directory!" -ForegroundColor Red
    Write-Host "   cd server\VectorService" -ForegroundColor Yellow
    exit 1
}

# Activate venv
.\venv\Scripts\Activate.ps1

Write-Host "📍 Working Directory: $(Get-Location)" -ForegroundColor Cyan
Write-Host "🐍 Python: $(Get-Command python | Select-Object -ExpandProperty Source)" -ForegroundColor Cyan
Write-Host "📡 Service URL: http://localhost:8000" -ForegroundColor Cyan
Write-Host "📖 API Docs: http://localhost:8000/docs" -ForegroundColor Cyan
Write-Host ""
Write-Host "🎯 Architecture: Stateless ML service" -ForegroundColor Magenta
Write-Host "   → SuggestionService calls POST /products/embed" -ForegroundColor Gray
Write-Host "   → Gets vector response" -ForegroundColor Gray  
Write-Host "   → Stores in its own DB" -ForegroundColor Gray
Write-Host ""
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host ""

# Run service
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
