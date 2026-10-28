using System.ComponentModel.DataAnnotations.Schema;
using Shared.Core.EF.Domain.Entities;

namespace VoucherService.Entities;

public class Voucher : BaseEntity<string>
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    [Column(TypeName = "varchar(30)")]
    public required DiscountScope Scope { get; set; }

    [Column(TypeName = "varchar(30)")]
    public required DiscountType Type { get; set; }

    public required decimal Value { get; set; }

    public long? MaxDiscountAmount { get; set; }
    public long? MinOrderAmount { get; set; }

    public int? MaxUsageCount { get; set; }
    public int? MaxUsagePerUser { get; set; }
    public int UsedCount { get; set; } = 0;

    public DateTime? StartAt { get; set; }
    public DateTime? ExpiredAt { get; set; }

    public bool IsActive { get; set; } = true;

    // jsonb, NULL = applies to every item (future targeting for scope ProductPrice)
    public List<string>? ApplicableProductIds { get; set; }

    public Voucher() : base(Guid.NewGuid().ToString())
    {
    }

    public enum DiscountScope
    {
        ShippingFee,
        OrderTotal,
        ProductPrice
    }

    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }
}
