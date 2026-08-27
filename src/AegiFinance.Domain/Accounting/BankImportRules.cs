using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AegiFinance.Domain.Accounting;

public static class BankImportRules
{
    public static string DeduplicationHash(Guid bankAccountId, DateTime date, string description, string? reference, decimal amount, string currency, int occurrence)
    {
        if (occurrence < 1) throw new ArgumentOutOfRangeException(nameof(occurrence));
        var canonical = string.Join('|', bankAccountId.ToString("D"), date.Date.ToString("O"), Normalize(description), Normalize(reference), amount.ToString("0.00", CultureInfo.InvariantCulture), currency.Trim().ToUpperInvariant(), occurrence.ToString(CultureInfo.InvariantCulture));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
}
