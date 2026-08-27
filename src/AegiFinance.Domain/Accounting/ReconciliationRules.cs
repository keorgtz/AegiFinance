using System.Globalization;
using System.Text;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Accounting;

public sealed record ReconciliationFactor(string Code, string Label, decimal Points, string Detail);
public sealed record ReconciliationScore(decimal Score, bool AmountExact, bool ReferenceExact, IReadOnlyList<ReconciliationFactor> Factors);

public static class ReconciliationRules
{
    public static decimal SignedAmount(LedgerEntry entry) => entry.EntryType switch
    {
        LedgerEntryType.Expense or LedgerEntryType.TransferOut => -Math.Abs(entry.Amount),
        LedgerEntryType.Income or LedgerEntryType.TransferIn => Math.Abs(entry.Amount),
        _ => entry.Amount
    };

    public static ReconciliationScore Score(
        decimal bankAmount, DateTime bankDate, string bankDescription, string? bankReference,
        decimal ledgerAmount, DateTime ledgerDate, string ledgerDescription, string? ledgerReference,
        string? clientName, ReconciliationSettings settings)
    {
        ValidateSettings(settings);
        var factors = new List<ReconciliationFactor>();
        var difference = Math.Abs(bankAmount - ledgerAmount);
        var amountExact = difference <= settings.AmountTolerance;
        if (amountExact) factors.Add(new("amount", "Importe", settings.AmountWeight, $"Diferencia {difference:0.00}"));
        else
        {
            var largest = Math.Max(Math.Abs(bankAmount), Math.Abs(ledgerAmount));
            var ratio = largest == 0 ? 0 : Math.Min(Math.Abs(bankAmount), Math.Abs(ledgerAmount)) / largest;
            if (ratio >= 0.2m) factors.Add(new("amount-partial", "Importe parcial", Math.Round(settings.AmountWeight * ratio * 0.55m, 2), $"Coincide {ratio:P0} del importe"));
        }

        var days = Math.Abs((bankDate.Date - ledgerDate.Date).Days);
        if (days <= settings.DateToleranceDays)
        {
            var datePoints = settings.DateWeight * (1m - (decimal)days / Math.Max(1, settings.DateToleranceDays + 1));
            factors.Add(new("date", "Fecha", Math.Round(datePoints, 2), days == 0 ? "Mismo día" : $"{days} día(s) de diferencia"));
        }

        var bankRef = Normalize(bankReference);
        var ledgerRef = Normalize(ledgerReference);
        var referenceExact = bankRef.Length > 0 && bankRef == ledgerRef;
        if (referenceExact) factors.Add(new("reference", "Referencia", settings.ReferenceWeight, $"Referencia {bankReference}"));
        else if (bankRef.Length >= 4 && ledgerRef.Length >= 4 && (bankRef.Contains(ledgerRef) || ledgerRef.Contains(bankRef)))
            factors.Add(new("reference-partial", "Referencia parcial", settings.ReferenceWeight * 0.65m, "Las referencias se contienen parcialmente"));

        var normalizedBankDescription = Normalize(bankDescription);
        var normalizedClient = Normalize(clientName);
        if (normalizedClient.Length >= 4 && normalizedBankDescription.Contains(normalizedClient))
            factors.Add(new("client", "Cliente", settings.ClientWeight, $"El concepto contiene {clientName}"));

        var similarity = TokenSimilarity(bankDescription, ledgerDescription);
        if (similarity >= 0.25m)
            factors.Add(new("pattern", "Patrón", Math.Round(settings.PatternWeight * similarity, 2), $"Similitud de texto {similarity:P0}"));

        return new(Math.Min(100m, factors.Sum(item => item.Points)), amountExact, referenceExact, factors);
    }

    public static void ValidateSettings(ReconciliationSettings settings)
    {
        if (settings.DateToleranceDays is < 0 or > 30) throw new InvalidOperationException("La tolerancia de fecha debe estar entre 0 y 30 días.");
        if (settings.AmountTolerance is < 0 or > 1000) throw new InvalidOperationException("La tolerancia de importe no es válida.");
        if (settings.SuggestionThreshold is < 0 or > 100 || settings.AutoConfirmThreshold is < 0 or > 100)
            throw new InvalidOperationException("Los umbrales deben estar entre 0 y 100.");
        if (settings.AutoConfirmThreshold < settings.SuggestionThreshold)
            throw new InvalidOperationException("El umbral automático no puede ser menor al umbral de sugerencia.");
        if (settings.AmountWeight + settings.DateWeight + settings.ReferenceWeight + settings.ClientWeight + settings.PatternWeight != 100)
            throw new InvalidOperationException("Los pesos de conciliación deben sumar 100.");
    }

    public static void ValidateDifference(decimal difference, decimal tolerance, ReconciliationDifferenceType type, string? reason)
    {
        if (Math.Abs(difference) <= tolerance) return;
        if (type == ReconciliationDifferenceType.None || string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("La diferencia debe clasificarse y justificarse antes de confirmar.");
    }

    private static decimal TokenSimilarity(string left, string right)
    {
        var leftTokens = Tokens(left);
        var rightTokens = Tokens(right);
        if (leftTokens.Count == 0 || rightTokens.Count == 0) return 0;
        return (decimal)leftTokens.Intersect(rightTokens).Count() / leftTokens.Union(rightTokens).Count();
    }

    private static HashSet<string> Tokens(string value) => Normalize(value).Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Where(item => item.Length >= 3).ToHashSet(StringComparer.Ordinal);

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var normalized = new string(decomposed.Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Select(character => char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : ' ').ToArray());
        return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
