$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 12 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

function Forbid-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 12 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -match $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase11.ps1")
if (-not $?) { $errors.Add("Phase 11 gate must pass before Phase 12.") }

Require-Text "src/AegiFinance.Domain/Accounting/AccountStatementRules.cs" '(?s)ClosingBalance.*openingBalance.*Debit.*Credit.*EnsureBalanced.*closingBalance' "The statement formula does not enforce opening plus movements equals closing."
Require-Text "src/AegiFinance.Domain/Accounting/AccountStatementRules.cs" 'SHA256|VerificationCode' "Statements do not have a deterministic verification fingerprint."
Require-Text "src/AegiFinance.Application/Features/AccountStatements/Queries/GetClientStatement/GetClientStatementQueryHandler.cs" 'GeneralLedgerAccountPurpose\.AccountsReceivable' "Statements are not sourced from the accounts-receivable Major Ledger."
Require-Text "src/AegiFinance.Application/Features/AccountStatements/Queries/GetClientStatement/GetClientStatementQueryHandler.cs" '(?s)SubscriptionPermissions.*SubscriptionId.*allowed' "Client subscription scope is not enforced."
Require-Text "src/AegiFinance.Application/Features/AccountStatements/Queries/GetClientStatement/GetClientStatementQueryHandler.cs" '(?s)SubscriptionAllocations.*ReversedAt' "Plan-filtered statements do not account for payment allocations and reversals."
Require-Text "src/AegiFinance.Application/Features/AccountStatements/Queries/GetClientStatement/GetClientStatementQueryHandler.cs" '(?s)OverdueBalance.*IsOverdue|IsOverdue.*OverdueBalance' "Overdue receivables are missing from the statement."
Forbid-Text "src/AegiFinance.Application/Features/AccountStatements/Queries/GetClientStatement/GetClientStatementQueryHandler.cs" 'DebitMXN|CreditMXN|OriginalAmountMXN|ExchangeRateUsed' "Statements still depend on lossy MXN projections instead of the selected ledger currency."
Require-Text "src/AegiFinance.Infrastructure/Services/AccountStatementExporter.cs" '(?s)CreatePdf\(AccountStatementDto statement\).*CreateCsv\(AccountStatementDto statement\).*VerificationCode' "PDF and CSV are not generated from the same verified statement model."
Require-Text "src/AegiFinance.Web/Controllers/AccountStatementsController.cs" '(?s)ViewAccountStatements.*ExportAccountStatements.*CreateAccountStatementInquiries.*ResolveAccountStatementInquiries' "Statement APIs do not expose the complete permission policy set."
Require-Text "src/AegiFinance.Application/Features/AccountStatements/AccountStatementOptionsFeature.cs" '(?s)GetAccountStatementClientsQuery.*GetAccountStatementSubscriptionsQuery.*RestrictedSubscriptionsAsync' "Statement selectors depend on broader client/subscription permissions or ignore authorized subscriptions."
Require-Text "src/AegiFinance.Application/Features/AccountStatements/AccountStatementInquiryFeature.cs" '(?s)IdempotencyKey.*SubscriptionPermissions.*JournalEntryId' "Movement inquiries are not idempotent or client scoped."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827181542_Phase12ClientPortal.cs" '(?s)fn_AegiFinanceAccountStatementInquiryAccess.*FILTER PREDICATE.*BLOCK PREDICATE' "Statement inquiries lack SQL tenant/client isolation."
Require-Text "web/app/(app)/account-statement/page.tsx" '(?s)statements\.export\.pdf.*statements\.filters\.subscription.*statements\.movement\.inquiry.*statements\.inquiry\.resolve' "The statement workspace lacks stable permission-aware controls."
Require-Text "web/app/(app)/account-statement/page.tsx" 'md:hidden' "The statement movement list is not recomposed for mobile."
Require-Text "tests/AegiFinance.LedgerTests/Program.cs" '(?s)AccountStatementRules\.ClosingBalance.*AccountStatementRules\.EnsureBalanced.*AccountStatementRules\.VerificationCode' "Statement financial invariants lack regression coverage."
Require-Text "src/AegiFinance.Web/Dockerfile" 'fonts-dejavu-core' "The deployable API image lacks fonts required for PDF generation."

& (Join-Path $PSScriptRoot "verify-deployment.ps1")
if (-not $?) { $errors.Add("Docker deployment contract must remain valid in Phase 12.") }

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 12 verified: Major Ledger statements, exact currency, plan scope, overdue balances, PDF/CSV parity, client inquiries, permissions, tenant isolation and deployment support are present."
