using Contract.Order;

namespace Contract.Product;

/// <summary>
/// Command: Reserve stock for a list of products
/// Sent by OrderSaga to ProductService
/// </summary>
public class ReserveStock
{
    public required string OrderId { get; set; }
    public required List<OrderItemEvent> Items { get; set; }
}
