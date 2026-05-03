namespace Contract;

/// <summary>
/// Command: Set order to WaitingForPayment status (used for COD orders after stock reserved)
/// </summary>
public class SetOrderWaitingForConfirmation
{
    public required string OrderId { get; set; }
}
