using System.ComponentModel.DataAnnotations.Schema;
using Shared.Core.EF.Domain.Entities;

namespace VoucherService.Entities;

/// <summary>
/// Audit + anti double-apply table. One row per (successfully) applied voucher attempt.
/// UNIQUE(OrderId) at DB level guarantees 1 order can only have 1 applied voucher (v1).
/// </summary>
public class AppliedVoucher : BaseEntity<string>
{
    public required string VoucherId { get; set; }

    // Snapshot of the code at apply time, independent from later edits/deletes on Voucher
    public required string VoucherCode { get; set; }

    public required string OrderId { get; set; }
    public required string UserId { get; set; }

    [Column(TypeName = "varchar(30)")]
    public required Voucher.DiscountScope Scope { get; set; }

    [Column(TypeName = "varchar(30)")]
    public required Voucher.DiscountType Type { get; set; }

    public required long DiscountAmount { get; set; }

    public long OrderSubTotalAtApply { get; set; }
    public long OrderShippingCostAtApply { get; set; }

    [Column(TypeName = "varchar(30)")]
    public AppliedVoucherStatus Status { get; set; } = AppliedVoucherStatus.Applied;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizedAt { get; set; }

    public AppliedVoucher() : base(Guid.NewGuid().ToString())
    {
    }

    public enum AppliedVoucherStatus
    {
        Applied,
        Confirmed,
        Released
    }
}
