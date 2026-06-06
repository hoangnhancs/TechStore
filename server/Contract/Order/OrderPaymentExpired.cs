namespace Contract.Order;

/// <summary>
/// Scheduled event: auto-cancel order when payment window expires
/// </summary>
public record OrderPaymentExpired
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
}
