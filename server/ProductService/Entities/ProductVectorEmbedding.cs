using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace ProductService.Entities
{
    public class ProductVectorEmbedding : BaseEntity<string>
    {
        public string ProductId { get; set; } = string.Empty;
        public Product? Product { get; set; }
        [NotMapped]
        public List<float> Embedding { get; set; } = new();

        // Property thực tế được lưu trong DB
        public string EmbeddingJson
        {
            get => JsonSerializer.Serialize(Embedding);
            set => Embedding = string.IsNullOrWhiteSpace(value)
                ? new List<float>()
                : JsonSerializer.Deserialize<List<float>>(value) ?? new List<float>();
        }
        public ProductVectorEmbedding() : base(Guid.NewGuid().ToString()) { }
    }
}