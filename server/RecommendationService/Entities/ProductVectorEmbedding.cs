using Pgvector;
using Shared.Core.EF.Domain.Entities;

namespace RecommendationService.Entities
{
    public class ProductVectorEmbedding : BaseEntity<int>
    {
        public required string ProductId { get; set; }
        public required bool IsProductDeleted { get; set; }
        public Vector Embedding { get; set; } = null!;
    }
}
