using System.Globalization;
using System.Text;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Accounting;

public static class PaymentApplicationRules
{
    public static void ValidatePayment(LedgerEntry payment)
    {
        if (payment.EntryType != LedgerEntryType.Income) throw new InvalidOperationException("Sólo los ingresos pueden aplicarse como pagos.");
        if (!payment.ClientId.HasValue) throw new InvalidOperationException("El pago debe estar asociado a un cliente.");
        if (payment.Amount <= 0) throw new InvalidOperationException("El pago debe tener un importe mayor a cero.");
    }

    public static void ValidateAllocation(decimal amount, decimal paymentAvailable, decimal chargeBalance)
    {
        if (amount <= 0) throw new InvalidOperationException("El importe aplicado debe ser mayor a cero.");
        if (amount > paymentAvailable) throw new InvalidOperationException("La aplicación supera el saldo disponible del pago.");
        if (amount > chargeBalance) throw new InvalidOperationException("La aplicación supera el saldo pendiente del cargo.");
    }

    public static IEnumerable<BillingItem> OrderCharges(IEnumerable<BillingItem> items, PaymentApplicationPriority priority, LedgerEntry referencePayment, Guid? preferredServiceId)
    {
        var query = items.Where(item => item.Status is BillingItemStatus.Pending or BillingItemStatus.Partial);
        return priority switch
        {
            PaymentApplicationPriority.Plan => query.OrderByDescending(item => preferredServiceId.HasValue && item.Subscription.ServiceId == preferredServiceId)
                .ThenBy(item => item.Subscription.Service.Code).ThenBy(item => item.DueDate).ThenBy(item => item.GeneratedAt),
            PaymentApplicationPriority.Reference => query.OrderByDescending(item => ReferenceScore(referencePayment, item))
                .ThenBy(item => item.DueDate).ThenBy(item => item.GeneratedAt),
            _ => query.OrderBy(item => item.DueDate).ThenBy(item => item.GeneratedAt)
        };
    }

    public static int ReferenceScore(LedgerEntry payment, BillingItem item)
    {
        var reference = Normalize(payment.Reference);
        if (reference.Length == 0) return 0;
        var candidates = new[] { item.IdempotencyKey, item.Subscription.Code, item.Description, item.Subscription.Service.Code }
            .Select(Normalize)
            .Where(value => value.Length > 0)
            .ToArray();
        if (candidates.Any(value => value == reference)) return 2;
        return candidates.Any(value => value.Contains(reference) || reference.Contains(value)) ? 1 : 0;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var decomposed = value.Normalize(NormalizationForm.FormD);
        return new string(decomposed.Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
            .Select(char.ToUpperInvariant).ToArray());
    }
}
