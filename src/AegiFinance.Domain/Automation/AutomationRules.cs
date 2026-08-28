using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Automation;

public static class AutomationRules
{
    public static bool IsReminderDay(int daysRemaining) => daysRemaining is 7 or 3 or 1 or 0;

    public static DateTime RenewalEnd(DateTime endDate, BillingType billingType, int? customIntervalDays) => billingType switch
    {
        BillingType.Yearly => endDate.AddYears(1),
        BillingType.Custom => endDate.AddDays(customIntervalDays is > 0 ? customIntervalDays.Value : throw new InvalidOperationException("Custom renewal requires an interval.")),
        BillingType.Monthly => endDate.AddMonths(1),
        _ => throw new InvalidOperationException("This billing type cannot be renewed automatically.")
    };

    public static TimeSpan RetryDelay(int attempt) => TimeSpan.FromSeconds(Math.Min(900, Math.Pow(2, Math.Clamp(attempt, 1, 20)) * 15));
}
