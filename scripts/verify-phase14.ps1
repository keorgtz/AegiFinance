$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 14 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase13.ps1")
if (-not $?) { $errors.Add("Phase 13 gate must pass before Phase 14.") }

Require-Text "src/AegiFinance.Domain/Enums/FinancialReportKind.cs" '(?s)Portfolio.*Collections.*Revenue.*Expenses.*Aging.*Reconciliation.*CashFlow.*TrialBalance.*AccountLedger' "The required report families are incomplete."
Require-Text "src/AegiFinance.Domain/Reporting/FinancialReportRules.cs" '(?s)RequiredPermission.*ViewReceivablesReports.*ViewCollectionsReports.*ViewFinancialReports.*ViewReconciliationReports.*ViewAccountingReports.*NextRun' "Report permissions or scheduling rules are not centralized."
Require-Text "src/AegiFinance.Infrastructure/Services/FinancialReportingService.cs" '(?s)JournalLines\.IgnoreQueryFilters.*AccountingPeriod\.OrganizationId.*JournalEntryStatus\.Draft.*CreateCsv\(FinancialReportDto report\)' "Reports and CSV export do not share a tenant-scoped Major Ledger model."
Require-Text "src/AegiFinance.Infrastructure/Services/FinancialReportingService.cs" '(?s)Portfolio\(.*Aging\(.*ReconciliationAsync.*TrialBalance\(.*AccountLedger\(' "The reporting engine does not cover all financial and accounting views."
Require-Text "src/AegiFinance.Infrastructure/Services/FinancialReportingService.cs" '(?s)HasPermissionAsync.*RequiredPermission.*ResultHash.*SHA256.*ReportRunStatus\.Completed' "Scheduled runs do not revalidate access or preserve verifiable output evidence."
Require-Text "src/AegiFinance.Web/Controllers/ReportsController.cs" '(?s)ViewReports.*ExportReports.*ManageReportSchedules.*ViewScheduledReportRuns.*DownloadScheduledReports' "Report APIs do not enforce the complete permission policy set."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260828020525_Phase14ReportingAnalytics.cs" '(?s)ReportSchedules.*ClientId.*ReportRuns.*fn_AegiFinanceRootAccess.*FILTER PREDICATE.*BLOCK PREDICATE' "Report schedules and runs lack organization and client isolation in SQL RLS."
Require-Text "src/AegiFinance.Worker/ReportScheduleHostedService.cs" '(?s)PeriodicTimer.*IReportScheduleProcessor.*ProcessDueAsync' "The existing worker does not process scheduled reports."
Require-Text "docker-compose.yml" 'Reporting__PollIntervalSeconds:\s*\$\{REPORTING_POLL_INTERVAL_SECONDS:-60\}' "Compose does not expose the scheduled-report polling contract."
Require-Text "web/app/(app)/reports/page.tsx" '(?s)reports\.kind\.portfolio.*reports\.kind\.collections.*reports\.kind\.revenue.*reports\.kind\.expenses.*reports\.kind\.aging.*reports\.kind\.reconciliation.*reports\.kind\.cashflow.*reports\.kind\.trialbalance.*reports\.kind\.accountledger' "Report selectors lack stable permission-aware controls."
Require-Text "web/app/(app)/reports/page.tsx" '(?s)summaryText.*report\.unit.*aria-label' "Charts lack period, unit or a textual summary."
Require-Text "web/app/(app)/reports/page.tsx" 'metric\.definition' "Metric definitions are not visible."
Require-Text "web/app/(app)/reports/page.tsx" '(?s)md:hidden.*hidden overflow-x-auto' "Report detail is not recomposed for mobile and desktop."
Require-Text "tests/AegiFinance.LedgerTests/Program.cs" '(?s)FinancialReportRules\.RequiredPermission.*FinancialReportRules\.NextRun' "Report access and scheduling rules lack regression coverage."

& (Join-Path $PSScriptRoot "verify-deployment.ps1")
if (-not $?) { $errors.Add("Docker deployment contract must remain valid in Phase 14.") }

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 14 verified: reconciled reports, shared filters and CSV, visible metric definitions, scheduled access-controlled runs, responsive UI and deployment contract are present."
