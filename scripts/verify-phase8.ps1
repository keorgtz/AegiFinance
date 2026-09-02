$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 8 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase7.ps1")
if (-not $?) { $errors.Add("Phase 7 gate must pass before Phase 8.") }

Require-Text "src/AegiFinance.Domain/Entities/BillingItem.cs" '(?s)IdempotencyKey.*PeriodStart.*PeriodEnd' "Charges do not preserve idempotency and billed period evidence."
Require-Text "src/AegiFinance.Domain/Accounting/ReceivableRules.cs" '(?s)EffectiveAmount.*LateFee.*CreditNote' "Receivable balance rules are not centralized."
Require-Text "src/AegiFinance.Infrastructure/Services/MajorLedgerService.cs" 'PostBillingAdjustmentAsync' "Credit notes and late fees are not posted to the Major Ledger."
Require-Text "src/AegiFinance.Application/Features/Billing/Commands/CancelBillingItem/CancelBillingItemCommandHandler.cs" 'ReverseAsync' "Charge cancellation does not create accounting reversals."
Require-Text "src/AegiFinance.Domain/Accounting/JournalEntryRules.cs" '(?s)MaxDescriptionLength.*BuildReversalDescription' "Accounting reversals can exceed the journal description storage contract."
Require-Text "web/components/modules/billing/cancel-item-dialog.tsx" '(?s)max\(500.*maxLength=\{500\}' "Charge cancellation does not enforce its reason length before submission."
Require-Text "src/AegiFinance.Application/Features/Billing/Queries/GetReceivablesAging/GetReceivablesAgingQueryHandler.cs" 'GeneralLedgerAccountPurpose\.AccountsReceivable' "Aging does not reconcile against the receivables ledger auxiliary."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827020246_Phase8Receivables.cs" 'scheduled:.*SubscriptionId.*BillingCycleId' "Legacy charges are not backfilled with deterministic idempotency keys."
Require-Text "web/app/(app)/billing/page.tsx" 'Antigüedad de saldos' "The billing UI does not expose receivables aging."
Require-Text "web/components/modules/billing/manual-charge-dialog.tsx" 'CreateBillingItems' "Manual charges lack declarative UI permissions."
Require-Text "web/components/modules/billing/receivable-action-dialog.tsx" '(?s)AdjustBillingItems.*ManagePaymentPromises' "Receivable actions lack granular UI permissions."
Require-Text "tests/AegiFinance.LedgerTests/Program.cs" 'ReceivableRules\.Balance' "Receivable balance invariants lack regression coverage."

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 8 verified: idempotent charges, receivable adjustments, payment promises, aging, ledger reversals, permissions and deployment contract are present."
