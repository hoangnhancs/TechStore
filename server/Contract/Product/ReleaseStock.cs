using Contract.Order;

namespace Contract.Product;

/// <summary>
/// Command: Release/compensate reserved stock
/// Used for Saga compensation when order fails
/// </summary>
public class ReleaseStock
{
    public required string OrderId { get; set; }
    public required List<OrderItemEvent> Items { get; set; }
}
