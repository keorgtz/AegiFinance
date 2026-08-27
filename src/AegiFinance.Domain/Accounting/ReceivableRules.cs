using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Accounting;

public static class ReceivableRules
{
    public static decimal EffectiveAmount(BillingItem item) => item.Amount
        + item.Adjustments.Where(x => !x.ReversedAt.HasValue && x.Type == BillingAdjustmentType.LateFee).Sum(x => x.Amount)
        - item.Adjustments.Where(x => !x.ReversedAt.HasValue && x.Type == BillingAdjustmentType.CreditNote).Sum(x => x.Amount);

    public static decimal Balance(BillingItem item) => EffectiveAmount(item) - item.PaidAmount;

    public static void ValidateCredit(BillingItem item, decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("La nota de crédito debe ser mayor a cero.");
        if (amount > Balance(item)) throw new InvalidOperationException("La nota de crédito supera el saldo pendiente del cargo.");
    }
}
