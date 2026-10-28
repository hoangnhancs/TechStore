namespace VoucherService.DTOs;

public class ValidateVoucherResultDto
{
    public required string Code { get; set; }
    public required string Scope { get; set; }
    public required string Type { get; set; }
    public required long DiscountAmount { get; set; }
}
