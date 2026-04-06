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
    public class CommitStockConsumer : IConsumer<CommitStock>
    {
        private readonly ProductSvcDbContext _context;
        private readonly ILogger<CommitStockConsumer> _logger;
        public CommitStockConsumer(ProductSvcDbContext context, ILogger<CommitStockConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<CommitStock> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing CommitStock for OrderId: {OrderId}", message.OrderId);

            try
            {
                var productIds = message.Items.Select(i => i.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();
                // In a real implementation, you might mark the reserved stock as "committed" or perform other actions.
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var item in message.Items)
                    {
                        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                            $"""
                                UPDATE "Products"
                                SET "ReservedQuantity" = "ReservedQuantity" - {item.Quantity},
                                    "QuantityInStock"  = "QuantityInStock"  - {item.Quantity}
                                WHERE "Id" = {item.ProductId}
                                AND "ReservedQuantity" >= {item.Quantity}
                            """);
                        if (rowsAffected == 0)
                        {
                            await transaction.RollbackAsync();
                            
                            var product = products.First(p => p.Id == item.ProductId);
                            await context.Publish(new StockReservationFailed
                            {
                                OrderId = message.OrderId,
                                Reason = $"Commit stock for product {product.Name} (fail)",
                                Items = message.Items
                            });
                            return;
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch(Exception ex)
                {
                    await transaction.RollbackAsync();
                    await context.Publish(new StockReservationFailed
                    {
                        OrderId = message.OrderId,
                        Reason = $"Error committing stock for OrderId: {message.OrderId}",
                        Items = message.Items
                    });
                    _logger.LogError(ex, "Error committing stock for OrderId: {OrderId}", message.OrderId);
                }
                
                _logger.LogInformation("Stock committed successfully for OrderId: {OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing stock for OrderId: {OrderId}", message.OrderId);
                // Don't publish failure event for compensation - just log
            }
        }
    }
}