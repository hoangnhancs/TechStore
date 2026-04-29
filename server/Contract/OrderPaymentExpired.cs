namespace Contract;

/// <summary>
/// Scheduled event: auto-cancel order when payment window expires
/// </summary>
public class OrderPaymentExpired
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
}
