namespace Contract;

/// <summary>
/// Command: Confirm order (mark as confirmed after all steps complete)
/// </summary>
public class ConfirmOrder
{
    public required string OrderId { get; set; }
}
