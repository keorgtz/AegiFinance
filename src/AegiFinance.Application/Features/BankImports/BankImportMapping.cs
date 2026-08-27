using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AegiFinance.Domain.Entities;

namespace AegiFinance.Application.Features.BankImports;

public static partial class BankImportMapping
{
    private static readonly string[] DateAliases = ["fecha", "date", "transactiondate", "fechadeoperacion", "fechamovimiento"];
    private static readonly string[] DescriptionAliases = ["descripcion", "description", "concepto", "detalle", "movimiento"];
    private static readonly string[] ReferenceAliases = ["referencia", "reference", "ref", "folio", "documento"];
    private static readonly string[] AmountAliases = ["importe", "amount", "monto", "cantidad"];
    private static readonly string[] DebitAliases = ["cargo", "debit", "debito", "retiro"];
    private static readonly string[] CreditAliases = ["abono", "credit", "credito", "deposito"];
    private static readonly string[] CurrencyAliases = ["moneda", "currency", "divisa"];
    private static readonly string[] BalanceAliases = ["saldo", "balance", "saldodisponible"];

    public static IReadOnlyList<BankImportAdapterDto> Adapters { get; } =
    [
        new("generic", "Genérico", "Detecta encabezados comunes en español e inglés."),
        new("bbva-mx", "BBVA México", "Reconoce fecha, concepto, referencia, cargo, abono y saldo."),
        new("santander-mx", "Santander México", "Reconoce fecha, descripción, depósitos, retiros y saldo."),
        new("banorte-mx", "Banorte México", "Reconoce fecha de operación, referencia, depósitos y retiros.")
    ];

    public static BankImportColumnMap Resolve(IReadOnlyList<string> headers, BankImportColumnMap? requested, BankImportProfile? profile, string adapterCode)
    {
        string? Pick(string? explicitName, string field, params string[] aliases)
        {
            if (!string.IsNullOrWhiteSpace(explicitName))
                return headers.FirstOrDefault(item => Normalize(item) == Normalize(explicitName));
            var prioritized = AdapterAliases(adapterCode, field).Concat(aliases).Distinct(StringComparer.Ordinal);
            return headers.FirstOrDefault(item => prioritized.Contains(Normalize(item), StringComparer.Ordinal));
        }

        return new BankImportColumnMap
        {
            Date = Pick(requested?.Date ?? profile?.DateColumn, "date", DateAliases),
            Description = Pick(requested?.Description ?? profile?.DescriptionColumn, "description", DescriptionAliases),
            Reference = Pick(requested?.Reference ?? profile?.ReferenceColumn, "reference", ReferenceAliases),
            Amount = Pick(requested?.Amount ?? profile?.AmountColumn, "amount", AmountAliases),
            Debit = Pick(requested?.Debit ?? profile?.DebitColumn, "debit", DebitAliases),
            Credit = Pick(requested?.Credit ?? profile?.CreditColumn, "credit", CreditAliases),
            Currency = Pick(requested?.Currency ?? profile?.CurrencyColumn, "currency", CurrencyAliases),
            Balance = Pick(requested?.Balance ?? profile?.BalanceColumn, "balance", BalanceAliases)
        };
    }

    private static IEnumerable<string> AdapterAliases(string adapterCode, string field) => (adapterCode, field) switch
    {
        ("bbva-mx", "date") => ["fechaoperacion", "fechadeoperacion"],
        ("bbva-mx", "description") => ["conceptoreferencia", "concepto"],
        ("bbva-mx", "reference") => ["referenciaampliada", "referencia"],
        ("santander-mx", "date") => ["fechamovimiento", "fecha"],
        ("santander-mx", "debit") => ["retiros", "cargo"],
        ("santander-mx", "credit") => ["depositos", "abono"],
        ("banorte-mx", "date") => ["fechadeoperacion", "fechaoperacion"],
        ("banorte-mx", "debit") => ["retiros", "retiro", "cargos"],
        ("banorte-mx", "credit") => ["depositos", "deposito", "abonos"],
        _ => []
    };

    public static string? Value(IReadOnlyDictionary<string, string> row, string? column) =>
        column is null ? null : row.TryGetValue(column, out var value) ? value.Trim() : null;

    public static bool TryDate(string? value, string? format, out DateTime parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (!string.IsNullOrWhiteSpace(format) && DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsed)) return true;
        var cultures = new[] { CultureInfo.GetCultureInfo("es-MX"), CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("en-US") };
        foreach (var culture in cultures)
            if (DateTime.TryParse(value, culture, DateTimeStyles.AllowWhiteSpaces, out parsed)) return true;
        return false;
    }

    public static bool TryAmount(string? value, out decimal parsed)
    {
        parsed = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;
        var cleaned = CurrencyCharacters().Replace(value.Trim(), string.Empty);
        var negative = cleaned.StartsWith('(') && cleaned.EndsWith(')');
        cleaned = cleaned.Trim('(', ')').Replace(" ", string.Empty);
        if (cleaned.Contains(',') && cleaned.Contains('.'))
        {
            var lastComma = cleaned.LastIndexOf(',');
            var lastDot = cleaned.LastIndexOf('.');
            cleaned = lastComma > lastDot ? cleaned.Replace(".", string.Empty).Replace(',', '.') : cleaned.Replace(",", string.Empty);
        }
        else if (cleaned.Count(c => c == ',') == 1 && cleaned.Split(',')[1].Length <= 2) cleaned = cleaned.Replace(',', '.');
        else cleaned = cleaned.Replace(",", string.Empty);
        if (!decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out parsed)) return false;
        if (negative) parsed = -Math.Abs(parsed);
        return true;
    }

    public static string Hash(params string?[] values)
    {
        var canonical = string.Join('|', values.Select(value => (value ?? string.Empty).Trim().ToUpperInvariant()));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    public static string Json<T>(T value) => JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    public static string Normalize(string value) => Diacritics().Replace(value.Normalize(NormalizationForm.FormD), string.Empty)
        .Where(char.IsLetterOrDigit).Aggregate(new StringBuilder(), (builder, character) => builder.Append(char.ToLowerInvariant(character))).ToString();

    [GeneratedRegex(@"[$€£¥]|MXN|USD|EUR", RegexOptions.IgnoreCase)]
    private static partial Regex CurrencyCharacters();

    [GeneratedRegex(@"\p{Mn}")]
    private static partial Regex Diacritics();
}
