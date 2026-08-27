$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 13 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase12.ps1")
if (-not $?) { $errors.Add("Phase 12 gate must pass before Phase 13.") }

Require-Text "src/AegiFinance.Domain/Accounting/AccountingGovernanceRules.cs" '(?s)BuildCloseChecklist.*drafts.*balanced.*orphans.*pending-imports.*CanClose.*VerificationCode' "Period close rules do not provide a deterministic blocking checklist."
Require-Text "src/AegiFinance.Domain/Accounting/AccountingGovernanceRules.cs" 'EnsureIndependentApproval' "Independent reopen approval is not enforced by the domain."
Require-Text "src/AegiFinance.Infrastructure/Services/AccountingGovernanceService.cs" '(?s)IsolationLevel\.Serializable.*VerificationCode.*CloseChecklistJson.*GovernanceVersion' "Period close is not serialized or does not preserve verification evidence."
Require-Text "src/AegiFinance.Infrastructure/Services/AccountingGovernanceService.cs" '(?s)RequestReopenAsync.*ReviewReopenAsync.*EnsureIndependentApproval.*AccountingPeriodStatus\.Open' "Reopen requests do not use independent approval before opening the period."
Require-Text "src/AegiFinance.Infrastructure/Services/AccountingGovernanceService.cs" '(?s)UnbalancedEntries.*OrphanEntries.*PendingImports.*UnreconciledBankLines' "Accounting integrity checks are incomplete."
Require-Text "src/AegiFinance.Infrastructure/Data/Interceptors/MajorLedgerInvariantInterceptor.cs" '(?s)verified checklist evidence.*independent approval evidence.*closed accounting period' "The persistence boundary does not enforce close, reopen and posting invariants."
Require-Text "src/AegiFinance.Infrastructure/Data/Interceptors/AuditInterceptor.cs" '(?s)OrganizationId.*UserSession.*IPAddress.*UserAgent' "Access/change audit evidence is not tenant-tagged or does not preserve session context."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" '(?s)AppendQueryFilter<AuditLog>.*AppendQueryFilter<AccountingPeriod>.*AppendQueryFilter<AccountingPeriodReopenRequest>' "Audit governance entities are not tenant filtered."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827184009_Phase13AccountingGovernance.cs" '(?s)#EntryOrganization.*#PeriodOrganization.*fn_AegiFinanceOrganizationAccess.*AccountingPeriodReopenRequests.*AuditLogs' "The Phase 13 migration does not isolate or safely split historical tenant evidence."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827184009_Phase13AccountingGovernance.cs" '(?s)COUNT\(DISTINCT bank\.\[OrganizationId\]\).*conflicting organization evidence' "Historical journal migration does not reject cross-tenant evidence."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827184009_Phase13AccountingGovernance.cs" '(?s)fn_AegiFinanceJournalPeriodAccess.*JournalEntries.*fn_AegiFinanceJournalLinePeriodAccess.*JournalLines' "Journal entries and lines are not protected by period-level SQL tenant isolation."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260827184009_Phase13AccountingGovernance.cs" 'TR_JournalEntries_RequireOpenPeriod' "The database does not block journal writes into closed periods."
Require-Text "src/AegiFinance.Web/Controllers/AccountingGovernanceController.cs" '(?s)ViewAccountingIntegrity.*CloseAccountingPeriods.*RequestAccountingPeriodReopen.*ApproveAccountingPeriodReopen.*ViewAccountingAudit' "Accounting governance APIs lack the required business permissions."
Require-Text "src/AegiFinance.Web/Controllers/AuditLogsController.cs" 'ViewAccountingAudit' "The audit trail still depends on an unrelated role-management permission."
Require-Text "web/app/(app)/audit/page.tsx" '(?s)audit\.tabs\.overview.*audit\.period\.close\.open.*audit\.period\.reopen\.open.*audit\.reopen\.review\.submit.*audit\.events\.detail\.open' "The audit workspace lacks stable permission-aware controls."
Require-Text "web/app/(app)/audit/page.tsx" '(?s)md:grid.*grid-cols-2.*role="alert"' "The audit workspace lacks responsive recomposition or explicit alert states."
Require-Text "tests/AegiFinance.LedgerTests/Program.cs" '(?s)AccountingGovernanceRules\.BuildCloseChecklist.*UnbalancedEntries.*EnsureIndependentApproval' "Accounting governance invariants lack regression coverage."

& (Join-Path $PSScriptRoot "verify-deployment.ps1")
if (-not $?) { $errors.Add("Docker deployment contract must remain valid in Phase 13.") }

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 13 verified: tenant audit, verified period close, independent reopen approval, integrity alerts, evidence, permissions and deployment contract are present."
