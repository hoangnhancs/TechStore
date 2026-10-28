namespace VoucherService.DTOs;

public class CreateVoucherDto
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>ShippingFee | OrderTotal | ProductPrice</summary>
    public required string Scope { get; set; }

    /// <summary>Percentage | FixedAmount</summary>
    public required string Type { get; set; }

    public required decimal Value { get; set; }

    public long? MaxDiscountAmount { get; set; }
    public long? MinOrderAmount { get; set; }

    public int? MaxUsageCount { get; set; }
    public int? MaxUsagePerUser { get; set; }

    public DateTime? StartAt { get; set; }
    public DateTime? ExpiredAt { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string>? ApplicableProductIds { get; set; }
}
