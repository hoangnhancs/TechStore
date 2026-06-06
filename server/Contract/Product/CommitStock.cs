using Contract.Order;

namespace Contract.Product;

/// <summary>
/// Command: Commit reserved stock after order is confirmed
/// Sent by OrderSaga to ProductService to actually deduct QuantityInStock
/// </summary>
public class CommitStock
{
    public required string OrderId { get; set; }
    public required List<OrderItemEvent> Items { get; set; }
}
