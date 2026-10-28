namespace VoucherService.DTOs;

public class VoucherDto
{
    public string Id { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Scope { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal Value { get; set; }
    public long? MaxDiscountAmount { get; set; }
    public long? MinOrderAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public int? MaxUsagePerUser { get; set; }
    public int UsedCount { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public bool IsActive { get; set; }
    public List<string>? ApplicableProductIds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
