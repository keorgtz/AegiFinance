using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AegiFinance.Domain.Accounting;

public sealed record AccountStatementValueLine(DateTime Date, Guid Id, decimal Debit, decimal Credit);

public static class AccountStatementRules
{
    public static decimal ClosingBalance(decimal openingBalance, IEnumerable<AccountStatementValueLine> lines) =>
        openingBalance + lines.Sum(line => line.Debit - line.Credit);

    public static void ValidatePeriod(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            throw new InvalidOperationException("La fecha inicial no puede ser posterior a la fecha final.");
    }

    public static void EnsureBalanced(decimal openingBalance, decimal closingBalance, IEnumerable<AccountStatementValueLine> lines)
    {
        if (ClosingBalance(openingBalance, lines) != closingBalance)
            throw new InvalidOperationException("El estado de cuenta no reconcilia con sus movimientos.");
    }

    public static string VerificationCode(Guid clientId, string currency, DateTime? from, DateTime? to, decimal openingBalance,
        decimal closingBalance, IEnumerable<AccountStatementValueLine> lines)
    {
        var canonical = new StringBuilder()
            .Append(clientId.ToString("N")).Append('|')
            .Append(currency.ToUpperInvariant()).Append('|')
            .Append(from?.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) ?? "OPEN").Append('|')
            .Append(to?.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) ?? "OPEN").Append('|')
            .Append(CanonicalAmount(openingBalance)).Append('|')
            .Append(CanonicalAmount(closingBalance));
        foreach (var line in lines.OrderBy(line => line.Date).ThenBy(line => line.Id))
            canonical.Append('|').Append(line.Date.Ticks.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(line.Id.ToString("N")).Append(':')
                .Append(CanonicalAmount(line.Debit)).Append(':')
                .Append(CanonicalAmount(line.Credit));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())))[..16];
    }

    private static string CanonicalAmount(decimal value) => value.ToString("0.############################", CultureInfo.InvariantCulture);
}
