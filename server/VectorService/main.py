"""
VectorService - Pure ML Computation Service
Only handles vector embedding generation, NO database, NO RabbitMQ
Stateless and horizontally scalable
"""
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
import logging
import sys

from config import settings
from models import EmbeddingRequest, EmbeddingResponse, ProductData, ProductEmbeddingResponse
from embedding_service import embedding_service

# Configure logging
logging.basicConfig(
    level=getattr(logging, settings.log_level),
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[logging.StreamHandler(sys.stdout)]
)
logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Initialize ML model only"""
    logger.info(f"🚀 Starting {settings.service_name} (Pure ML Mode)...")
    
    # Only initialize ML model - no DB, no RabbitMQ
    embedding_service.initialize()
    
    logger.info("✅ VectorService ready (stateless computation service)")
    logger.info(f"   Model: {embedding_service.model_name}")
    logger.info(f"   Dimension: {embedding_service.dimension}")
    
    yield
    
    logger.info("👋 Shutting down...")


app = FastAPI(
    title="VectorService - Pure ML API",
    description="Stateless embedding generation service. Only computes vectors, doesn't store anything.",
    version="2.0.0",
    lifespan=lifespan
)

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/")
async def root():
    """Health check"""
    return {
        "service": settings.service_name,
        "status": "healthy",
        "mode": "pure_ml",
        "model": embedding_service.model_name,
        "dimension": embedding_service.dimension,
        "description": "Stateless vector computation service"
    }


@app.post("/embed", response_model=EmbeddingResponse)
async def create_embedding(request: EmbeddingRequest):
    """
    Generate embedding vector from text
    
    This is a pure computation endpoint - no side effects, no storage.
    Call this from SuggestionService/SearchService to get vectors.
    """
    try:
        embedding = embedding_service.encode(request.text)
        return EmbeddingResponse(
            embedding=embedding,
            dimension=embedding_service.dimension,
            model=embedding_service.model_name
        )
    except Exception as e:
        logger.error(f"Error generating embedding: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/products/embed", response_model=ProductEmbeddingResponse)
async def embed_product(product: ProductData):
    """
    Generate embedding for a product
    
    Converts product data to searchable text and returns the embedding.
    The caller (SuggestionService) is responsible for storing this vector.
    
    Returns:
    - embedding: 384-dim vector
    - text: the text representation used for embedding
    - product_id: for reference
    """
    try:
        # Convert product to searchable text
        text = embedding_service.product_to_text(product)
        
        # Generate embedding
        embedding = embedding_service.encode(text)
        
        return ProductEmbeddingResponse(
            product_id=product.id,
            embedding=embedding,
            text=text,
            dimension=embedding_service.dimension,
            model=embedding_service.model_name
        )
    except Exception as e:
        logger.error(f"Error processing product: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/batch/embed")
async def embed_batch(texts: list[str]):
    """
    Generate embeddings for multiple texts in one call (more efficient)
    
    Use this for bulk operations from SuggestionService.
    """
    try:
        embeddings = embedding_service.encode_batch(texts)
        return {
            "count": len(embeddings),
            "embeddings": embeddings,
            "dimension": embedding_service.dimension,
            "model": embedding_service.model_name
        }
    except Exception as e:
        logger.error(f"Error in batch embedding: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/health")
async def health_check():
    """Detailed health check for load balancers"""
    return {
        "status": "healthy",
        "model_loaded": embedding_service.model is not None,
        "model_name": embedding_service.model_name,
        "dimension": embedding_service.dimension
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8000,
        reload=True
    )
