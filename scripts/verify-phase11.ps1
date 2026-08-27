$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 11 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase10.ps1")
if (-not $?) { $errors.Add("Phase 10 gate must pass before Phase 11.") }

Require-Text "src/AegiFinance.Domain/Accounting/PaymentApplicationRules.cs" '(?s)amount > paymentAvailable.*amount > chargeBalance' "Payment application rules do not enforce both financial caps."
Require-Text "src/AegiFinance.Domain/Accounting/PaymentApplicationRules.cs" '(?s)PaymentApplicationPriority\.Plan.*PaymentApplicationPriority\.Reference.*OrderBy\(item => item\.DueDate\)' "Automatic application priorities are incomplete."
Require-Text "src/AegiFinance.Infrastructure/Services/AllocationService.cs" '(?s)ApplyManuallyAsync.*ApplyAutomaticallyAsync.*ReapplyAsync' "Manual, automatic or reapplication workflows are missing."
Require-Text "src/AegiFinance.Infrastructure/Services/AllocationService.cs" '(?s)IsReversed = true.*RestoreChargeBalance\(allocation\.BillingItem, allocation\.Amount\)' "Reversal does not preserve audit evidence or restore charge balances."
Require-Text "src/AegiFinance.Infrastructure/Services/AllocationService.cs" '(?s)PaidAmount < amount.*PaidAmount -= amount' "Reversal can hide a balance mismatch instead of restoring the exact amount."
Require-Text "src/AegiFinance.Infrastructure/Services/AllocationService.cs" 'ReceiptNumber' "Applications do not preserve a receipt number."
Require-Text "src/AegiFinance.Infrastructure/Services/AllocationService.cs" 'AppliedBy = _currentUser\.UserId' "Applications do not preserve the actor."
Require-Text "src/AegiFinance.Infrastructure/Services/AllocationService.cs" 'Priority = priority, Origin = origin' "Applications do not preserve rule and origin."
Require-Text "src/AegiFinance.Infrastructure/Services/AllocationService.cs" '(?s)JournalEntryId = journal\.Id.*UnappliedAfter' "Payment receipts are not linked to journal evidence and unapplied balances."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" '(?s)PaymentAllocationVersion.*IsConcurrencyToken' "Payment and charge allocation concurrency is not protected."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" 'OrganizationId, item\.IdempotencyKey.*IsUnique' "Payment applications are not idempotent within an organization."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827145532_Phase11PaymentApplications.cs" '(?s)fn_AegiFinancePaymentApplicationAccess.*fn_AegiFinanceBillingItemAccess.*SubscriptionAllocations' "Phase 11 database tenant isolation is incomplete."
Require-Text "src/AegiFinance.Web/Controllers/AllocationsController.cs" 'ViewPaymentApplications' "Viewing payment applications lacks an API permission."
Require-Text "src/AegiFinance.Web/Controllers/AllocationsController.cs" 'ApplyPayments' "Applying payments lacks an API permission."
Require-Text "src/AegiFinance.Web/Controllers/AllocationsController.cs" 'ReversePaymentApplications' "Reversing payment applications lacks an API permission."
Require-Text "src/AegiFinance.Web/Controllers/AllocationsController.cs" 'ReapplyPayments' "Reapplying payments lacks an API permission."
Require-Text "src/AegiFinance.Web/Controllers/AllocationsController.cs" 'ViewPaymentReceipts' "Viewing payment receipts lacks an API permission."
Require-Text "src/AegiFinance.Web/Controllers/AllocationsController.cs" 'ManagePaymentApplicationSettings' "Payment application settings lack an API permission."
Require-Text "web/app/(app)/allocations/page.tsx" '(?s)payments\.manual\.submit.*payments\.receipt\.close' "The payment application UI lacks stable permission-aware controls."
Require-Text "tests/AegiFinance.LedgerTests/Program.cs" '(?s)ValidatePayment.*ValidateAllocation.*ReferenceScore' "Payment application financial rules lack regression coverage."

& (Join-Path $PSScriptRoot "verify-deployment.ps1")
if (-not $?) { $errors.Add("Docker deployment contract must remain valid in Phase 11.") }

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 11 verified: manual and automatic applications, priorities, combined payments, advances, reversals, receipts, journals, tenant scope, permissions and deployment contract are present."
