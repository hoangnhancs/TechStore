using AutoMapper;
using Contract.Product;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services;

namespace SearchService.Consumers
{
    public class ProductUpdatedConsumer : IConsumer<ProductUpdated>
    {
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private readonly ILogger<ProductUpdatedConsumer> _logger;

        public ProductUpdatedConsumer(IMapper mapper, ICacheService cache, ILogger<ProductUpdatedConsumer> logger)
        {
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductUpdated> context)
        {
            _logger.LogInformation("Received ProductUpdated event for ProductId: {ProductId}", context.Message.Id);

            var item = _mapper.Map<ProductItem>(context.Message);
            await item.SaveAsync(cancellation: context.CancellationToken);

            await Task.WhenAll(
                _cache.RemoveAsync("search:top10", context.CancellationToken),
                _cache.RemoveByPrefixAsync($"search:category:{item.CategoryId}", context.CancellationToken)
            );
        }
    }
}
