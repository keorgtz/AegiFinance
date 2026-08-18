using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Accounting;

public sealed record SubscriptionPriceBreakdown(decimal BaseAmount, decimal DiscountAmount, decimal TaxAmount, decimal Total, decimal ProrationFactor);

public static class SubscriptionPricingRules
{
    public static SubscriptionPriceBreakdown Calculate(decimal basePrice, decimal discountPercent, decimal taxPercent, decimal prorationFactor = 1m)
    {
        if (basePrice < 0 || discountPercent is < 0 or > 100 || taxPercent is < 0 or > 100 || prorationFactor is < 0 or > 1)
            throw new InvalidOperationException("Invalid subscription pricing values.");
        var proratedBase = decimal.Round(basePrice * prorationFactor, 2, MidpointRounding.AwayFromZero);
        var discount = decimal.Round(proratedBase * discountPercent / 100m, 2, MidpointRounding.AwayFromZero);
        var taxable = proratedBase - discount;
        var tax = decimal.Round(taxable * taxPercent / 100m, 2, MidpointRounding.AwayFromZero);
        return new(proratedBase, discount, tax, taxable + tax, prorationFactor);
    }

    public static SubscriptionTermsVersion ResolveTerms(IEnumerable<SubscriptionTermsVersion> versions, DateTime effectiveAt)
        => versions.Where(item => item.EffectiveFrom <= effectiveAt && (!item.EffectiveTo.HasValue || item.EffectiveTo > effectiveAt))
            .OrderByDescending(item => item.EffectiveFrom).ThenByDescending(item => item.VersionNumber).FirstOrDefault()
            ?? throw new InvalidOperationException("No subscription terms are effective for the billing date.");

    public static decimal FirstPeriodFactor(Subscription subscription, SubscriptionTermsVersion terms)
    {
        if (terms.ProrationPolicy != ProrationPolicy.Daily || subscription.LastBillingDate.HasValue || terms.BillingType != BillingType.Monthly)
            return 1m;
        var days = DateTime.DaysInMonth(subscription.StartDate.Year, subscription.StartDate.Month);
        var activeDays = days - subscription.StartDate.Day + 1;
        return decimal.Round((decimal)activeDays / days, 8, MidpointRounding.AwayFromZero);
    }
}
