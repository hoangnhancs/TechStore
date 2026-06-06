using Contract.Order;

namespace Contract.Product;

/// <summary>
/// Event: Stock reservation failed for an order
/// </summary>
public class StockReservationFailed
{
    public required string OrderId { get; set; }
    public required string Reason { get; set; }
    public required List<OrderItemEvent> Items { get; set; }
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
}
