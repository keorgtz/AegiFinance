$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 9 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

function Forbid-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 9 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -match $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase8.ps1")
if (-not $?) { $errors.Add("Phase 8 gate must pass before Phase 9.") }

Require-Text "src/AegiFinance.Application/Features/BankImports/PreviewBankImportCommand.cs" '(?s)\.csv.*\.xlsx.*BankImportRowStatus\.Duplicate' "CSV/XLSX preview or duplicate classification is missing."
Require-Text "src/AegiFinance.Domain/Accounting/BankImportRules.cs" 'SHA256\.HashData' "Bank row deduplication is not protected by a stable hash."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" '(?s)DeduplicationHash.*IsUnique' "The database does not enforce bank-line deduplication."
Require-Text "src/AegiFinance.Application/Features/BankImports/ConfirmBankImportCommand.cs" 'Status == BankImportRowStatus\.Valid' "Confirmation does not explicitly exclude invalid rows."
Forbid-Text "src/AegiFinance.Application/Features/BankImports/ConfirmBankImportCommand.cs" 'LedgerEntries\.Add' "Bank import silently creates operational ledger movements."
Require-Text "src/AegiFinance.Application/Features/BankImports/RollbackBankImportCommand.cs" '(?s)IsReconciled.*LedgerEntryId' "Rollback does not protect reconciled evidence."
Require-Text "src/AegiFinance.Web/Controllers/BankImportsController.cs" '(?s)CreateBankStatementImports.*ConfirmBankStatementImports.*RollbackBankStatementImports' "Bank import API permissions are incomplete."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" 'AppendQueryFilter<BankImportRow>' "Tenant isolation is missing for staged bank rows."
Require-Text "web/components/modules/ledger/bank-import-dialog.tsx" '(?s)ledger\.bankImports\.form\.file.*ledger\.bankImports\.preview\.confirm' "Import UI controls lack declarative permission keys."
Require-Text "web/components/modules/ledger/bank-import-dialog.tsx" '(?s)Incompleta.*Rechazada.*correction' "Row-level validation does not explain correction."
Require-Text "tests/AegiFinance.LedgerTests/Program.cs" 'BankImportRules\.DeduplicationHash' "Deduplication invariants lack regression coverage."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827031121_Phase9BankStatementImport.cs" 'BankImportRows' "The Phase 9 database migration is missing."

& (Join-Path $PSScriptRoot "verify-deployment.ps1")
if (-not $?) { $errors.Add("Docker deployment contract must remain valid in Phase 9.") }

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 9 verified: CSV/XLSX preview, profiles, row validation, deduplication, controlled confirmation/rollback, tenant scope, permissions and deployment contract are present."
