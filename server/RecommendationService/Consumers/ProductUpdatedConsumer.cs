using Contract;
using Contract.Product;
using MassTransit;
using RecommendationService.DTOs;
using RecommendationService.Entities;
using RecommendationService.Persistence;
using RecommendationService.Services;
using System.Text.Json;

namespace RecommendationService.Consumers
{
    public class ProductUpdatedConsumer : IConsumer<ProductUpdated>
    {
        private readonly IRecommendationUnitOfWork _unitOfWork;
        private readonly VectorServiceClient _vectorServiceClient;
        private readonly ILogger<ProductUpdatedConsumer> _logger;

        public ProductUpdatedConsumer(
            IRecommendationUnitOfWork unitOfWork,
            VectorServiceClient vectorServiceClient,
            ILogger<ProductUpdatedConsumer> logger)
        {
            _unitOfWork = unitOfWork;
            _vectorServiceClient = vectorServiceClient;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductUpdated> context)
        {
            var product = context.Message;
            _logger.LogInformation("Received ProductUpdated event for product: {ProductId}", product.Id);

            var request = new ProductEmbeddingRequest
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                OldPrice = product.OldPrice,
                DiscountPercentage = product.DiscountPercentage,
                CategoryName = product.CategoryName,
                BrandName = product.BrandName,
                Tags = product.Attributes.Select(a => new ProductTag
                {
                    Name = a.Name,
                    Value = a.Value
                }).ToList()
            };

            var embeddingResponse = await _vectorServiceClient.GenerateProductEmbeddingAsync(request, context.CancellationToken);

            if (embeddingResponse == null || embeddingResponse.Embedding.Count == 0)
            {
                _logger.LogWarning("Failed to generate embedding for updated product: {ProductId}", product.Id);
                return;
            }

            var existing = await _unitOfWork.ProductVectorEmbeddingRepository
                .GetProductVectorEmbeddingByProductId(product.Id, context.CancellationToken);

            if (existing != null)
            {
                var embeddingJson = JsonSerializer.Serialize(embeddingResponse.Embedding);
                await _unitOfWork.ProductVectorEmbeddingRepository
                    .UpdateProductVectorEmbedding(product.Id, embeddingJson);
            }
            else
            {
                // Product chưa có embedding (ví dụ bị miss lúc seed), tạo mới
                await _unitOfWork.ProductVectorEmbeddingRepository.AddAsync(new ProductVectorEmbedding
                {
                    ProductId = product.Id,
                    IsProductDeleted = false,
                    Embedding = embeddingResponse.Embedding
                });
            }

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Successfully updated vector embedding for product: {ProductId}", product.Id);
        }
    }
}
