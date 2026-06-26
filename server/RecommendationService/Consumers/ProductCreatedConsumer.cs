using Contract;
using MassTransit;
using RecommendationService.DTOs;
using RecommendationService.Entities;
using RecommendationService.Persistence;
using RecommendationService.Services;

namespace RecommendationService.Consumers
{
    public class ProductCreatedConsumer : IConsumer<ProductCreated>
    {
        private readonly IRecommendationUnitOfWork _unitOfWork;
        private readonly VectorServiceClient _vectorServiceClient;
        private readonly ILogger<ProductCreatedConsumer> _logger;

        public ProductCreatedConsumer(
            IRecommendationUnitOfWork unitOfWork,
            VectorServiceClient vectorServiceClient,
            ILogger<ProductCreatedConsumer> logger)
        {
            _unitOfWork = unitOfWork;
            _vectorServiceClient = vectorServiceClient;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductCreated> context)
        {
            var product = context.Message;
            _logger.LogInformation("Received ProductCreated event for product: {ProductId}", product.Id);

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
                _logger.LogWarning("Failed to generate embedding for new product: {ProductId}", product.Id);
                return;
            }

            await _unitOfWork.ProductVectorEmbeddingRepository.AddAsync(new ProductVectorEmbedding
            {
                ProductId = product.Id,
                IsProductDeleted = false,
                Embedding = embeddingResponse.Embedding
            });

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Successfully added vector embedding for product: {ProductId}", product.Id);
        }
    }
}
