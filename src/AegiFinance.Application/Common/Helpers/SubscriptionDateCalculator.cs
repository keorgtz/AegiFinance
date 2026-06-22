using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Common.Helpers;

public static class SubscriptionDateCalculator
{
    public static DateTime CalculateNextBillingDate(DateTime startDate, BillingType billingType, int billingDay)
    {
        var nextDate = billingType == BillingType.Yearly
            ? startDate.AddYears(1)
            : startDate.AddMonths(1);

        var daysInMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
        var day = Math.Min(billingDay, daysInMonth);

        return new DateTime(nextDate.Year, nextDate.Month, day, 0, 0, 0, nextDate.Kind);
    }
}
