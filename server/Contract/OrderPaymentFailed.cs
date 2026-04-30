namespace Contract;

/// <summary>
/// Event published when payment fails — notifies FE so user can retry or change payment method
/// </summary>
public class OrderPaymentFailed
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}
