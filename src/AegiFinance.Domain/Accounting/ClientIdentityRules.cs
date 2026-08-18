using System.Text;
using System.Globalization;

namespace AegiFinance.Domain.Accounting;

public static class ClientIdentityRules
{
    public static string Normalize(string value)
    {
        var normalized = value.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        return new string(normalized.Where(character =>
            CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark &&
            char.IsLetterOrDigit(character)).ToArray());
    }

    public static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Normalize(value);
}
