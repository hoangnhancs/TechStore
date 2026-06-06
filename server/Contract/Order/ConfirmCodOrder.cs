namespace Contract.Order;

/// <summary>
/// Command: confirm COD order manually (from admin/user on FE)
/// </summary>
public class ConfirmCodOrder
{
    public required string OrderId { get; set; }
}
