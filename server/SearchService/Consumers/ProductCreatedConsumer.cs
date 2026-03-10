using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contract;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class ProductCreatedConsumer : IConsumer<ProductCreated>
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ProductCreatedConsumer> _logger;
        public ProductCreatedConsumer(IMapper mapper, ILogger<ProductCreatedConsumer> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<ProductCreated> context)
        {
            _logger.LogInformation("Received ProductCreated event for ProductId: {ProductId}", context.Message.Id);
            
            var item = _mapper.Map<ProductItem>(context.Message);

            await item.SaveAsync(cancellation: context.CancellationToken);
        }
    }
}