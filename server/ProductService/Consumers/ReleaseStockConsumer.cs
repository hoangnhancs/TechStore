using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;

namespace ProductService.Consumers
{
    /// <summary>
    /// Consumer for ReleaseStock compensation command
    /// Releases previously reserved stock when order fails/cancels
    /// </summary>
    public class ReleaseStockConsumer : IConsumer<ReleaseStock>
    {
        private readonly ProductSvcDbContext _context;
        private readonly ILogger<ReleaseStockConsumer> _logger;

        public ReleaseStockConsumer(ProductSvcDbContext context, ILogger<ReleaseStockConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReleaseStock> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing ReleaseStock for OrderId: {OrderId}", message.OrderId);

            try
            {
                // Release stock for each item
                foreach (var item in message.Items)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        UPDATE ""Products""
                        SET ""QuantityInStock"" = ""QuantityInStock"" + {item.Quantity}
                        WHERE ""Id"" = {item.ProductId}
                    ");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock released successfully for OrderId: {OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stock for OrderId: {OrderId}", message.OrderId);
                // Don't publish failure event for compensation - just log
            }
        }
    }
}
