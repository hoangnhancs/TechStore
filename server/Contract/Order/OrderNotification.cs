namespace Contract.Order;

/// <summary>
/// Event published by OrderSaga to notify FE of order outcome (success or failure).
/// Consumed by NotificationService to push SignalR event to client.
/// </summary>
public class OrderNotification
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
    public required bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
