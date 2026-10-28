namespace VoucherService.DTOs;

/// <summary>
/// Preview request (no DB write) — FE checkout page calls this before placing the order,
/// when there is no OrderId yet.
/// </summary>
public class ValidateVoucherDto
{
    public required string Code { get; set; }
    public required long SubTotal { get; set; }
    public long ShippingCost { get; set; } = 0;
    public List<ValidateVoucherItemDto>? Items { get; set; }
}

public class ValidateVoucherItemDto
{
    public required string ProductId { get; set; }
    public required int Quantity { get; set; }
    public required long UnitPrice { get; set; }
}
