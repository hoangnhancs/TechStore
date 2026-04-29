namespace Contract;

/// <summary>
/// Command: retry payment with a new payment method
/// Published by FE after payment fails
/// </summary>
public class RetryPayment
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
    public required string PaymentMethod { get; set; }
    public required string Currency { get; set; }
    public required long Amount { get; set; }
}
