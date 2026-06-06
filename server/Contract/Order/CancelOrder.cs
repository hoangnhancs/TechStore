namespace Contract.Order;

/// <summary>
/// Command: Cancel order (compensating transaction)
/// </summary>
public class CancelOrder
{
    public required string OrderId { get; set; }
    public required string Reason { get; set; }
}
