# 🤖 VectorService

**Pure ML API - Stateless embedding generation for TechStore**

## 🎯 What It Does

Converts text/products into 384-dimensional vectors using ML model `all-MiniLM-L6-v2`.

**That's it.** No database, no events, no storage.

## 🚀 Quick Start

```powershell
# 1. Install dependencies
pip install -r requirements.txt

# 2. Start service
.\start.ps1

# 3. Test (new terminal)
.\test.ps1
```

Service: http://localhost:8000  
Docs: http://localhost:8000/docs

## 📡 API

### Generate Product Embedding
```http
POST /products/embed
Content-Type: application/json

{
  "id": "prod-001",
  "name": "Smart TV Samsung",
  "old_price": 15000000,
  ...
}
```

Response:
```json
{
  "product_id": "prod-001",
  "embedding": [0.002, 0.065, ...],  // 384 floats
  "text": "Name: Smart TV Samsung...",
  "dimension": 384
}
```

**SuggestionService** calls this API and stores the vector in its own DB.

## 🏗️ Architecture

```
SuggestionService → POST /products/embed → VectorService
                                              ↓
                                        Returns vector
        ↓
Stores in own DB
```

## 📦 Files

- `main.py` - FastAPI app
- `config.py` - Settings
- `models.py` - Data models
- `embedding_service.py` - ML model wrapper
- `requirements.txt` - Dependencies
- `.env` - Configuration

## ⚙️ Configuration

`.env`:
```env
MODEL_NAME=all-MiniLM-L6-v2
LOG_LEVEL=INFO
```

## 🔧 Integration Example

```csharp
// In SuggestionService (C#)
var response = await _http.PostAsJsonAsync(
    "http://vectorservice:8000/products/embed",
    productData
);

var embedding = await response.Content
    .ReadFromJsonAsync<ProductEmbeddingResponse>();

// Store in SuggestionService's database
await _db.ProductVectors.AddAsync(new ProductVector {
    ProductId = embedding.ProductId,
    Embedding = embedding.Embedding
});
```

## ✅ Why Stateless?

- **Simple**: No DB/RabbitMQ dependencies
- **Fast**: Startup in ~3 seconds
- **Scalable**: Run multiple instances easily
- **Clean**: Single responsibility (ML only)

---

**Need help?** Check VectorService.http for API examples.
