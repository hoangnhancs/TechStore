from datetime import datetime
from typing import Optional
from pydantic import BaseModel, Field


class ProductVector(BaseModel):
    """Product vector embedding stored in database"""
    id: str = Field(..., description="Product ID (UUID)")
    embedding: list[float] = Field(..., description="Vector embedding")
    text: str = Field(..., description="Original text used for embedding")
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: datetime = Field(default_factory=datetime.utcnow)


class ProductData(BaseModel):
    """Product data from RabbitMQ event"""
    id: str
    name: str
    description: Optional[str] = None
    old_price: float
    discount_percentage: float = 0
    category_name: Optional[str] = None
    brand_name: Optional[str] = None
    tags: list[dict] = Field(default_factory=list)


class EmbeddingRequest(BaseModel):
    """Request to generate embedding for text"""
    text: str = Field(..., description="Text to embed")


class EmbeddingResponse(BaseModel):
    """Response containing generated embedding"""
    embedding: list[float]
    dimension: int
    model: str


class ProductEmbeddingResponse(BaseModel):
    """Response for product embedding generation (includes product context)"""
    product_id: str
    embedding: list[float]
    text: str = Field(..., description="Text representation used for embedding")
    dimension: int
    model: str


class SimilarProductsRequest(BaseModel):
    """Request to find similar products"""
    product_id: Optional[str] = None
    text: Optional[str] = None
    top_k: int = Field(default=10, ge=1, le=100)


class SimilarProduct(BaseModel):
    """Similar product result"""
    product_id: str
    similarity: float
    text: str
