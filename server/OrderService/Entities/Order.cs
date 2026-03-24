using System;
using Shared.Core.EF.Domain.Entities;

namespace OrderService.Entities;

public class Order : BaseEntity<string>
{
    public required string UserId { get; set; }

    public List<OrderItem> Items { get; set; } = [];

    public OrderStatus Status { get; private set; } = OrderStatus.Created;

    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }

    public long SubToTal { get; set; }
    public long ShippingCost { get; set; }
    public long Discount { get; set; } = 0;
    public long Total { get; set; }

    public Order() : base(Guid.NewGuid().ToString())
    {
    }

    /// <summary>
    /// Factory method to create a new order
    /// </summary>
    public static Order CreateOrder(
        string userId, 
        List<OrderItem> items, 
        string shippingAddress, 
        string? billingAddress, 
        long shippingCost, 
        long discount)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Order must have at least one item");

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required");

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address is required");

        var order = new Order
        {
            UserId = userId,
            Items = items,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            ShippingCost = shippingCost,
            Discount = discount
        };

        // Calculate totals
        order.CalculateTotals();

        return order;
    }

    private void CalculateTotals()
    {
        SubToTal = Items.Sum(x => x.UnitPrice * x.Quantity);
        Total = SubToTal + ShippingCost - Discount;
    }

    public void UpdateOrder(List<OrderItem> items, string shippingAddress, string? billingAddress, long shippingCost, long discount)
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException("Cannot update order after processing");

        Items = items;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        ShippingCost = shippingCost;
        Discount = discount;

        CalculateTotals();
        UpdatedAt = DateTime.UtcNow;
    }
    public void Process()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException($"Cannot process order with status {Status}");

        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot ship order with status {Status}");

        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver order with status {Status}");

        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
            throw new InvalidOperationException($"Cannot complete order with status {Status}");

        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed order");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        switch (newStatus)
        {
            case OrderStatus.Processing:
                Process();
                break;
            case OrderStatus.Shipped:
                Ship();
                break;
            case OrderStatus.Delivered:
                Deliver();
                break;
            case OrderStatus.Completed:
                Complete();
                break;
            case OrderStatus.Cancelled:
                Cancel();
                break;
            default:
                throw new InvalidOperationException("Invalid order status");
        }
    }
    public enum OrderStatus
    {
        Created,
        Processing,
        Shipped,
        Delivered,
        Completed,
        Cancelled
    }
}