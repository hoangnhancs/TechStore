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
    /// Consumer for ReserveStock command from OrderSaga
    /// Atomically reserves stock for order items
    /// </summary>
    public class ReserveStockConsumer : IConsumer<ReserveStock>
    {
        private readonly ProductSvcDbContext _context;
        private readonly ILogger<ReserveStockConsumer> _logger;

        public ReserveStockConsumer(ProductSvcDbContext context, ILogger<ReserveStockConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing ReserveStock for OrderId: {OrderId}", message.OrderId);

            try
            {
                // Validate all products exist and have sufficient stock
                var productIds = message.Items.Select(i => i.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != productIds.Distinct().Count())
                {
                    var foundIds = products.Select(p => p.Id).ToList();
                    var missingIds = productIds.Where(id => !foundIds.Contains(id)).ToList();
                    
                    _logger.LogWarning("Products not found for OrderId {OrderId}: {MissingIds}", 
                        message.OrderId, string.Join(", ", missingIds));
                    
                    await context.Publish(new StockReservationFailed
                    {
                        OrderId = message.OrderId,
                        Reason = $"Products not found: {string.Join(", ", missingIds)}",
                        Items = message.Items
                    });
                    return;
                }

                // Check stock availability for each item
                foreach (var item in message.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);
                    if (product.QuantityInStock < item.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for ProductId {ProductId}. Requested: {Requested}, Available: {Available}",
                            item.ProductId, item.Quantity, product.QuantityInStock);
                        
                        await context.Publish(new StockReservationFailed
                        {
                            OrderId = message.OrderId,
                            Reason = $"Insufficient stock for product {product.Name}. Available: {product.QuantityInStock}, Requested: {item.Quantity}",
                            Items = message.Items
                        });
                        return;
                    }
                }

                // Precheck passed - try to reserve stock automatically with SQL update to prevent race conditions
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var item in message.Items)
                    {
                        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($"""
                            UPDATE "Products"
                            SET "ReservedQuantity" = "ReservedQuantity" + {item.Quantity}
                            WHERE "Id" = {item.ProductId}
                            AND "QuantityInStock" - "ReservedQuantity" >= {item.Quantity}
                        """);
                        if (rowsAffected == 0)
                        {
                            await transaction.RollbackAsync();
                            
                            var product = products.First(p => p.Id == item.ProductId);
                            await context.Publish(new StockReservationFailed
                            {
                                OrderId = message.OrderId,
                                Reason = $"Insufficient stock for product {product.Name} (concurrent reservation conflict)",
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
                        Reason = $"Error reserving stock for OrderId: {message.OrderId}",
                        Items = message.Items
                    });
                    _logger.LogError(ex, "Error reserving stock for OrderId: {OrderId}", message.OrderId);
                }


                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock reserved successfully for OrderId: {OrderId}", message.OrderId);
                
                // Publish success event
                await context.Publish(new StockReserved
                {
                    OrderId = message.OrderId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock for OrderId: {OrderId}", message.OrderId);
                
                await context.Publish(new StockReservationFailed
                {
                    OrderId = message.OrderId,
                    Reason = $"Internal error: {ex.Message}",
                    Items = message.Items
                });
            }
        }
    }
}
