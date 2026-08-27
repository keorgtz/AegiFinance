$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 10 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

function Forbid-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 10 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -match $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase9.ps1")
if (-not $?) { $errors.Add("Phase 9 gate must pass before Phase 10.") }

Require-Text "src/AegiFinance.Domain/Accounting/ReconciliationRules.cs" '(?s)ReconciliationScore.*AmountExact.*ReferenceExact.*Factors' "The reconciliation score is not explainable."
Require-Text "src/AegiFinance.Domain/Accounting/ReconciliationRules.cs" 'AutoConfirmThreshold < settings\.SuggestionThreshold' "Automatic confirmation can be configured below the suggestion threshold."
Require-Text "src/AegiFinance.Application/Features/Reconciliation/RunReconciliationCommand.cs" '(?s)MatchType == ReconciliationMatchType\.Exact.*Score >= settings\.AutoConfirmThreshold' "The engine does not restrict automatic confirmation to exact matches above the threshold."
Require-Text "src/AegiFinance.Application/Features/Reconciliation/RunReconciliationCommand.cs" '(?s)OneToMany.*ManyToOne.*Partial' "Combined and partial matching modes are incomplete."
Require-Text "src/AegiFinance.Application/Features/Reconciliation/ReconciliationCaseFeature.cs" '(?s)ValidateAvailabilityAsync.*DbUpdateConcurrencyException' "Double reconciliation lacks an availability check and concurrency conflict handling."
Require-Text "src/AegiFinance.Application/Features/Reconciliation/ReconciliationCaseFeature.cs" 'ReconciliationVersion\+\+' "Confirmed allocations do not advance the reconciliation concurrency token."
Require-Text "src/AegiFinance.Application/Features/Reconciliation/ReconciliationCaseFeature.cs" '(?s)ReversedAt.*ReversedBy.*ReversalReason' "Reconciliation reversal is not auditable."
Require-Text "src/AegiFinance.Application/Features/Reconciliation/ReconciliationPeriodFeature.cs" '(?s)DifferenceType.*Justification.*ValidateDifference' "Period close does not require a classified, justified difference."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" '(?s)ReconciliationVersion.*IsConcurrencyToken' "Reconciliation entities lack optimistic concurrency control."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827141143_Phase10ReconciliationEngine.cs" '(?s)ReconciliationCases.*ReconciliationPeriods.*AegiFinanceTenantSecurityPolicy' "The Phase 10 schema or database tenant policy is incomplete."
Require-Text "src/AegiFinance.Web/Controllers/ReconciliationController.cs" '(?s)RunReconciliation.*ConfirmReconciliation.*ReverseReconciliation.*CloseReconciliationPeriods' "Reconciliation API permissions are incomplete."
Forbid-Text "src/AegiFinance.Web/Controllers/LedgerController.cs" 'reconcile|unreconcile' "The legacy ledger endpoint can bypass reconciliation evidence."
Require-Text "web/components/modules/ledger/reconciliation-panel.tsx" '(?s)ledger\.reconciliation\.run.*ledger\.reconciliation\.confirm\.submit.*ledger\.reconciliation\.reverse\.submit' "The reconciliation UI lacks permission-aware primary actions."
Require-Text "tests/AegiFinance.LedgerTests/Program.cs" '(?s)ReconciliationRules\.Score.*ValidateDifference' "Reconciliation financial rules lack regression coverage."

& (Join-Path $PSScriptRoot "verify-deployment.ps1")
if (-not $?) { $errors.Add("Docker deployment contract must remain valid in Phase 10.") }

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 10 verified: explainable matching, thresholds, combinations, partial allocations, human confirmation, auditable reversal, closed periods, tenant scope, permissions and deployment contract are present."
