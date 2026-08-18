$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $fullPath = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        $errors.Add("Missing Phase 2 artifact: $path")
        return
    }
    if ((Get-Content -Raw -LiteralPath $fullPath) -notmatch $pattern) {
        $errors.Add($message)
    }
}

Require-Text "src/AegiFinance.Web/Controllers/DashboardController.cs" 'HttpGet\("summary"\)' "Dashboard summary endpoint is missing."
Require-Text "src/AegiFinance.Web/Controllers/DashboardController.cs" 'HttpGet\("attention"\)' "Dashboard attention endpoint is missing."
Require-Text "src/AegiFinance.Web/Controllers/DashboardController.cs" 'HttpGet\("activity"\)' "Dashboard activity endpoint is missing."
Require-Text "src/AegiFinance.Web/Controllers/DashboardController.cs" 'Authorize\(Policy = "ManageReconciliation"\)' "Bank import evidence is not protected by reconciliation permission."
Require-Text "src/AegiFinance.Application/Features/Dashboard/DashboardQueryFilters.cs" 'ApplyOperationalScope' "Shared operational filters are missing."
Require-Text "src/AegiFinance.Application/Features/Billing/Queries/GetBillingItems/GetBillingItemsQueryHandler.cs" 'ApplyOperationalScope' "Billing details do not share dashboard filters."
Require-Text "src/AegiFinance.Application/Features/Ledger/Queries/GetLedgerEntries/GetLedgerEntriesQueryHandler.cs" 'ApplyOperationalScope' "Ledger details do not share dashboard filters."
Require-Text "src/AegiFinance.Application/Features/BankReconciliation/Commands/UploadStatementCommand.cs" 'BankImportAttempt' "Bank imports do not preserve attempt evidence."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260818185620_Phase2OperationalDashboard.cs" 'CreateTable\(\s*name: "BankImportAttempts"' "Phase 2 migration does not create the import evidence table."
Require-Text "web/app/(app)/dashboard/page.tsx" '\["dashboard", "summary", filters\]' "Dashboard summary is not independently loadable."
Require-Text "web/app/(app)/dashboard/page.tsx" '\["dashboard", "attention", filters\]' "Dashboard attention queue is not independently loadable."
Require-Text "web/app/(app)/dashboard/page.tsx" '\["dashboard", "activity", filters\]' "Dashboard activity is not independently loadable."
Require-Text "web/app/(app)/dashboard/page.tsx" 'scopedAttentionHref' "Attention links do not preserve dashboard scope."
Require-Text "web/app/(app)/ledger/page.tsx" 'dashboard\.importFailures\.retry' "Failed import detail has no recoverable error state."
Require-Text "web/app/(app)/ledger/page.tsx" 'dashboard\.reconciliationLines\.retry' "Reconciliation differences have no recoverable detail state."
Require-Text "src/AegiFinance.Application/Features/Ledger/Queries/GetLedgerEntries/GetLedgerEntriesQueryHandler.cs" 'HasUnappliedBalance' "Unapplied payment links do not resolve to filtered ledger details."

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host "Phase 2 verified: operational endpoints, shared detail filters, independent loading and bank-import evidence are present."
