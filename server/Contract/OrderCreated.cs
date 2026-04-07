namespace Contract;

/// <summary>
/// Event published when a new order is created
/// </summary>
public class OrderCreated
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
    public required List<OrderItemEvent> Items { get; set; }
    public required string ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public long SubTotal { get; set; }
    public long ShippingCost { get; set; }
    public long Discount { get; set; }
    public long Total { get; set; }
    public string Status { get; set; } = "Created";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required string Currency { get; set; } 
    public required string PaymentMethod { get; set; } 
}
