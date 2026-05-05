from sentence_transformers import SentenceTransformer
import logging
from typing import Optional
from config import settings
from models import ProductData

logger = logging.getLogger(__name__)


class EmbeddingService:
    def __init__(self):
        self.model: Optional[SentenceTransformer] = None
        self.model_name = settings.model_name
        self.dimension = None

    def initialize(self):
        """Load the embedding model"""
        try:
            logger.info(f"Loading embedding model: {self.model_name}")
            self.model = SentenceTransformer(self.model_name)
            # Get embedding dimension
            test_embedding = self.model.encode("test")
            self.dimension = len(test_embedding)
            logger.info(f"Model loaded successfully. Dimension: {self.dimension}")
        except Exception as e:
            logger.error(f"Failed to load model: {e}")
            raise

    def encode(self, text: str) -> list[float]:
        """Generate embedding for text"""
        if not self.model:
            raise RuntimeError("Model not initialized")
        
        embedding = self.model.encode(text, convert_to_numpy=True)
        return embedding.tolist()

    def encode_batch(self, texts: list[str]) -> list[list[float]]:
        """Generate embeddings for multiple texts"""
        if not self.model:
            raise RuntimeError("Model not initialized")
        
        embeddings = self.model.encode(texts, convert_to_numpy=True, batch_size=32)
        return [emb.tolist() for emb in embeddings]

    @staticmethod
    def product_to_text(product: ProductData) -> str:
        """Convert product data to searchable text"""
        # Normalize tags
        tags_text = []
        for tag in product.tags:
            if tag.get('name') and tag.get('value'):
                tags_text.append(f"{tag['name']}: {tag['value']}")
        
        # Build comprehensive description
        parts = [
            f"Name: {product.name}",
        ]
        
        if product.description:
            # Clean description from array format if needed
            desc = product.description
            if isinstance(desc, list):
                desc = ". ".join(desc)
            elif isinstance(desc, str):
                # Remove curly braces and quotes if present
                desc = desc.strip("{}").replace('"', '')
            parts.append(f"Description: {desc}")
        
        parts.extend([
            f"Price: {product.old_price:,.0f} VND",
        ])
        
        if product.discount_percentage > 0:
            final_price = product.old_price * (1 - product.discount_percentage / 100)
            parts.append(f"Discount: {product.discount_percentage}% (Final: {final_price:,.0f} VND)")
        
        if product.category_name:
            parts.append(f"Category: {product.category_name}")
        
        if product.brand_name:
            parts.append(f"Brand: {product.brand_name}")
        
        if tags_text:
            parts.append(f"Specifications: {', '.join(tags_text)}")
        
        return ". ".join(parts)


# Global embedding service instance
embedding_service = EmbeddingService()
