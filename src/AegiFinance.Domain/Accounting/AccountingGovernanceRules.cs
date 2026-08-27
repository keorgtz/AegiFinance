using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AegiFinance.Domain.Accounting;

public sealed record AccountingIntegritySnapshot(
    int DraftEntries,
    int UnbalancedEntries,
    int OrphanEntries,
    int PendingImports,
    int FailedImports,
    int UnreconciledBankLines);

public sealed record AccountingCloseCheck(string Key, string Label, string State, int Count, string Detail, bool BlocksClose);

public static class AccountingGovernanceRules
{
    public static IReadOnlyList<AccountingCloseCheck> BuildCloseChecklist(DateTime periodEnd, DateTime utcNow, AccountingIntegritySnapshot snapshot) =>
    [
        Check("period-ended", "Periodo finalizado", periodEnd.Date < utcNow.Date ? 0 : 1, "La fecha final debe haber transcurrido.", true),
        Check("drafts", "Sin asientos en borrador", snapshot.DraftEntries, "Contabilizá o eliminá cada borrador.", true),
        Check("balanced", "Asientos cuadrados", snapshot.UnbalancedEntries, "Debe y haber deben coincidir en cada asiento.", true),
        Check("orphans", "Sin asientos huérfanos", snapshot.OrphanEntries, "Cada asiento debe conservar líneas y referencias válidas.", true),
        Check("pending-imports", "Importaciones finalizadas", snapshot.PendingImports, "Confirmá o revertí las importaciones en vista previa.", true),
        Check("reconciliation", "Movimientos bancarios conciliados", snapshot.UnreconciledBankLines, "Quedan movimientos bancarios sin conciliar; el cierre puede continuar con esta advertencia.", false),
        Check("failed-imports", "Importaciones sin error", snapshot.FailedImports, "Existen intentos fallidos que deben quedar revisados como evidencia.", false)
    ];

    public static bool CanClose(IEnumerable<AccountingCloseCheck> checks) => checks.All(item => !item.BlocksClose || item.Count == 0);

    public static string VerificationCode(Guid periodId, AccountingIntegritySnapshot snapshot, IEnumerable<AccountingCloseCheck> checks)
    {
        var canonical = new StringBuilder(periodId.ToString("N"));
        canonical.Append('|').Append(snapshot.DraftEntries).Append('|').Append(snapshot.UnbalancedEntries).Append('|')
            .Append(snapshot.OrphanEntries).Append('|').Append(snapshot.PendingImports).Append('|')
            .Append(snapshot.FailedImports).Append('|').Append(snapshot.UnreconciledBankLines);
        foreach (var check in checks.OrderBy(item => item.Key, StringComparer.Ordinal))
            canonical.Append('|').Append(check.Key).Append(':').Append(check.State).Append(':').Append(check.Count.ToString(CultureInfo.InvariantCulture));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())))[..16];
    }

    public static void EnsureIndependentApproval(Guid requesterId, Guid approverId)
    {
        if (requesterId == approverId) throw new InvalidOperationException("La reapertura debe aprobarla una persona distinta de quien la solicitó.");
    }

    private static AccountingCloseCheck Check(string key, string label, int count, string detail, bool blocks) =>
        new(key, label, count == 0 ? "Passed" : blocks ? "Blocked" : "Warning", count, detail, blocks);
}
