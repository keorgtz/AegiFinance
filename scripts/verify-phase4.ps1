$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 4 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

Require-Text "src/AegiFinance.Domain/Accounting/JournalEntryRules.cs" 'debit != credit' "Balanced-entry validation is missing."
Require-Text "src/AegiFinance.Infrastructure/Data/Interceptors/MajorLedgerInvariantInterceptor.cs" 'posted journal entry is immutable' "Posted-entry immutability is not enforced."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" 'IdempotencyKey\)\.IsUnique' "Journal idempotency is not protected by a unique index."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" 'AppendQueryFilter<JournalEntry>' "Journal entries are not tenant filtered."
Require-Text "src/AegiFinance.Infrastructure/Services/MajorLedgerService.cs" 'LegacyMigrationResultDto' "Controlled legacy migration is missing."
Require-Text "src/AegiFinance.Infrastructure/Services/MajorLedgerService.cs" 'difference == 0' "Legacy migration reconciliation is missing."
Require-Text "src/AegiFinance.Web/Controllers/MajorLedgerController.cs" 'Authorize\(Policy = "ReverseJournalEntries"\)' "Reversal is not API-authorized."
Require-Text "web/components/modules/ledger/major-ledger-panel.tsx" 'ledger.major.reverse.confirm' "Permission-aware reversal UI is missing."
Require-Text "AGENTS.md" 'Deployment rule — mandatory' "Docker Compose assessment rule is missing."

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 4 verified: double-entry, immutability, idempotency, tenant scope, reversal, migration reconciliation and permission-aware UI are present."
