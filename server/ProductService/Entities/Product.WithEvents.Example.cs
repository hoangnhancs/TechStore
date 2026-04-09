using Shared.Core.EF.Domain.Entities;
using Shared.Core.EF.Domain.Events;

namespace ProductService.Entities;

/// <summary>
/// Product Entity - Option 2: Extend AggregateRoot (with Domain Events)
/// Sử dụng khi cần publish domain events
/// </summary>
public class ProductWithEvents : AggregateRoot<string>
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required int QuantityInStock { get; set; }
    // ... other properties

    public ProductWithEvents() : base()
    {
        Id = Guid.NewGuid().ToString();
    }

    // Domain methods with events
    public void UpdatePrice(decimal newPrice)
    {
        var oldPrice = Price;
        Price = newPrice;
        
        // Raise domain event
        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
    }

    public void DecreaseStock(int quantity, string orderId)
    {
        if (QuantityInStock < quantity)
            throw new InvalidOperationException("Insufficient stock");
            
        QuantityInStock -= quantity;
        
        // Raise domain event for other services (SearchService, NotificationService)
        AddDomainEvent(new ProductStockChangedEvent(Id, QuantityInStock, orderId));
    }
}

// Domain Events
public record ProductPriceChangedEvent(string ProductId, decimal OldPrice, decimal NewPrice) 
    : DomainEventBase;

public record ProductStockChangedEvent(string ProductId, int CurrentStock, string OrderId) 
    : DomainEventBase;
