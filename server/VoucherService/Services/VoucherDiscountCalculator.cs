using VoucherService.Entities;

namespace VoucherService.Services;

/// <summary>
/// Pure function discount calculator — used by BOTH the preview endpoint (ValidateVoucherQuery)
/// and the real apply flow (ApplyVoucherCommand), so the two never diverge in result (plan mục F).
/// No DB access, no side effects.
/// </summary>
public static class VoucherDiscountCalculator
{
    public record CalcItem(string ProductId, int Quantity, long UnitPrice);

    public static long Calculate(
        Entities.Voucher.DiscountScope scope,
        Entities.Voucher.DiscountType type,
        decimal value,
        long? maxDiscountAmount,
        long subTotal,
        long shippingCost,
        List<CalcItem>? items,
        List<string>? applicableProductIds)
    {
        long baseAmount = scope switch
        {
            Entities.Voucher.DiscountScope.ShippingFee => shippingCost,
            Entities.Voucher.DiscountScope.OrderTotal => subTotal,
            Entities.Voucher.DiscountScope.ProductPrice => CalculateEligibleProductAmount(items, applicableProductIds),
            _ => 0
        };

        if (baseAmount <= 0)
            return 0;

        long discount = type switch
        {
            Entities.Voucher.DiscountType.Percentage => (long)Math.Floor(baseAmount * (value / 100m)),
            Entities.Voucher.DiscountType.FixedAmount => (long)value,
            _ => 0
        };

        if (maxDiscountAmount.HasValue && discount > maxDiscountAmount.Value)
            discount = maxDiscountAmount.Value;

        if (discount > baseAmount)
            discount = baseAmount;

        if (discount < 0)
            discount = 0;

        return discount;
    }

    private static long CalculateEligibleProductAmount(List<CalcItem>? items, List<string>? applicableProductIds)
    {
        if (items == null || items.Count == 0)
            return 0;

        var eligible = (applicableProductIds == null || applicableProductIds.Count == 0)
            ? items
            : items.Where(i => applicableProductIds.Contains(i.ProductId)).ToList();

        return eligible.Sum(i => (long)i.Quantity * i.UnitPrice);
    }
}
