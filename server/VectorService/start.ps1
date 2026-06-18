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

# Check Python
try {
    $pythonVersion = python --version
    Write-Host "🐍 $pythonVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ Python not found in PATH" -ForegroundColor Red
    exit 1
}

# Create venv if not exists
if (-not (Test-Path ".\venv")) {
    Write-Host ""
    Write-Host "📦 Creating virtual environment..." -ForegroundColor Yellow

    python -m venv venv

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to create virtual environment" -ForegroundColor Red
        exit 1
    }

    Write-Host "✓ Virtual environment created" -ForegroundColor Green
}

# Activate venv
Write-Host "🔄 Activating virtual environment..." -ForegroundColor Yellow
& ".\venv\Scripts\Activate.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to activate virtual environment" -ForegroundColor Red
    exit 1
}

# Upgrade pip
Write-Host "⬆️ Upgrading pip..." -ForegroundColor Yellow
python -m pip install --upgrade pip

# Install dependencies
if (Test-Path "requirements.txt") {
    Write-Host "📥 Installing dependencies from requirements.txt..." -ForegroundColor Yellow
    pip install -r requirements.txt
}
else {
    Write-Host "⚠ requirements.txt not found" -ForegroundColor Yellow
    Write-Host "📥 Installing minimum packages..." -ForegroundColor Yellow

    pip install `
        fastapi `
        uvicorn `
        sentence-transformers `
        torch `
        transformers `
        numpy `
        pydantic
}

# Verify packages
Write-Host ""
Write-Host "🔍 Verifying packages..." -ForegroundColor Yellow

python -c "
import fastapi
import uvicorn
import sentence_transformers
print('✓ FastAPI OK')
print('✓ Uvicorn OK')
print('✓ SentenceTransformers OK')
"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Missing required packages" -ForegroundColor Red
    exit 1
}

Write-Host ""
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

Write-Host "🚀 Starting FastAPI server..." -ForegroundColor Green
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host ""

# Run service
python -m uvicorn main:app --host 0.0.0.0 --port 8000 --reload