using System;
using Shared.Core.EF.Domain.Entities;

namespace OrderService.Entities;

public class Order : BaseEntity<string>
{
    public required string UserId { get; set; }
    public required string UserName { get; set; }
    public string? UserEmail { get; set; }
    public required string UserPhone { get; set; }
    public List<OrderItem> Items { get; set; } = [];

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }

    public long SubTotal { get; set; }
    public long ShippingCost { get; set; }
    public long Discount { get; set; } = 0;
    public long Total { get; set; }
    public Shipment? Shipment { get; set; }
    public List<OrderStatusHistory> StatusHistories { get; set; } = [];

    public Order() : base(Guid.NewGuid().ToString())
    {
    }

    /// <summary>
    /// Factory method to create a new order
    /// </summary>
    public static Order CreateOrder(
        string userId, 
        string userName,
        string? userEmail,
        string userPhone,
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
            UserName = userName,
            UserEmail = userEmail,
            UserPhone = userPhone,
            Items = items,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            ShippingCost = shippingCost,
            Discount = discount,
        };
        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = OrderStatus.Pending,
            ToStatus = OrderStatus.Pending,
            ChangedBy = "system",
            ChangedAt = DateTime.UtcNow
        });
        // Calculate totals
        order.CalculateTotals();

        return order;
    }

    private void CalculateTotals()
    {
        SubTotal = Items.Sum(x => x.UnitPrice * x.Quantity);
        Total = SubTotal + ShippingCost - Discount;
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
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot process order with status {Status}");

        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
        StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = OrderStatus.Pending,
            ToStatus = OrderStatus.Processing,
            ChangedBy = "system",
            ChangedAt = DateTime.UtcNow
        });
    }

    public void UpdateFromShipment(ShipmentStatus shipmentStatus)
    {
        // switch (shipmentStatus)
        // {
        //     case ShipmentStatus.Preparing:
        //         if (Status == OrderStatus.Created)
        //             Process();
        //         break;

        //     case ShipmentStatus.Picked:
        //     case ShipmentStatus.InTransit:
        //         if (Status == OrderStatus.Processing)
        //         {
        //             Status = OrderStatus.HandedOverToCarrier;
        //             UpdatedAt = DateTime.UtcNow;
        //         }
        //         break;

        //     case ShipmentStatus.Delivered:
        //         if (Status == OrderStatus.HandedOverToCarrier)
        //         {
        //             Status = OrderStatus.Delivered;
        //             UpdatedAt = DateTime.UtcNow;
        //         }
        //         break;

        //     case ShipmentStatus.Failed:
        //         // tùy business
        //         // có thể rollback về Processing
        //         break;
        // }
        var newStatus = shipmentStatus switch
        {
            ShipmentStatus.Preparing => OrderStatus.Processing,
            ShipmentStatus.Picked or ShipmentStatus.InTransit => OrderStatus.HandedOverToCarrier,
            ShipmentStatus.Delivered => OrderStatus.Delivered,
            _ => Status
        };

        if (Status == newStatus) return;

        StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = Status,
            ToStatus = newStatus,
            ChangedAt = DateTime.UtcNow
        });

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        if (Shipment != null)
        {
            Shipment.UpdateStatus(shipmentStatus);
        }
        else
        {
            // Log warning: shipment is null when updating from shipment status
        }
    }

    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
            throw new InvalidOperationException($"Cannot complete order with status {Status}");

        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed order");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = Status,
            ToStatus = OrderStatus.Cancelled,
            Note = reason,           // lý do từ Saga (hết stock, payment fail...)
            ChangedBy = "system",
            ChangedAt = DateTime.UtcNow
        });
    }

    // public void UpdateStatus(OrderStatus newStatus)
    // {
    //     switch (newStatus)
    //     {
    //         case OrderStatus.Processing:
    //             Process();
    //             break;
    //         case OrderStatus.HandedOverToCarrier:
    //             Ship();
    //             break;
    //         case OrderStatus.Delivered:
    //             Deliver();
    //             break;
    //         case OrderStatus.Completed:
    //             Complete();
    //             break;
    //         case OrderStatus.Cancelled:
    //             Cancel();
    //             break;
    //         default:
    //             throw new InvalidOperationException("Invalid order status");
    //     }
    // }
    public enum OrderStatus
    {
        Pending,
        Created,
        Processing,
        WaitingForPayment,
        HandedOverToCarrier,
        Delivered,
        Completed,
        Cancelled
    }
}