namespace Contract.Order;

public class OrderCancelled
{
    public required string OrderId { get; set; }
    public required string OrderNo { get; set; }
    public required string UserId { get; set; }
    public required string Reason { get; set; }
    public required string PaymentMethod { get; set; }
    public DateTime CancelledAt { get; set; }
}
