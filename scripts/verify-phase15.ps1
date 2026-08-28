$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 15 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase14.ps1")
if (-not $?) { $errors.Add("Phase 14 gate must pass before Phase 15.") }

Require-Text "src/AegiFinance.Infrastructure/Services/AutomationService.cs" '(?s)subscription\.renewed.*subscription\.expired.*expiration-reminder.*billing\.overdue-reminder.*reconciliation\.review-requested' "The required automation event families are incomplete."
Require-Text "src/AegiFinance.Infrastructure/Services/AutomationService.cs" '(?s)AttemptCount.*MaxAttempts.*DeadLetter.*RetryDelay.*Idempotency-Key' "Outbox delivery lacks idempotency, bounded retries or dead letters."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" '(?s)OrganizationId, item\.IdempotencyKey.*IsUnique.*AppendQueryFilter<OutboxMessage>' "Outbox messages are not uniquely idempotent and tenant scoped."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260828033824_Phase15OperationalReliability.cs" '(?s)OutboxMessages.*fn_AegiFinanceOrganizationAccess.*FILTER PREDICATE.*BLOCK PREDICATE' "Outbox SQL tenant isolation is incomplete."
Require-Text "src/AegiFinance.Worker/AutomationHostedService.cs" '(?s)ProduceAsync.*DispatchAsync.*aegifinance-worker-heartbeat' "The worker does not produce and dispatch durable automation or expose a heartbeat."
Require-Text "src/AegiFinance.Web/Middleware/RequestTelemetryMiddleware.cs" '(?s)ActivitySource.*Meter.*X-Trace-Id.*duration' "Request metrics and distributed trace correlation are incomplete."
Require-Text "src/AegiFinance.Web/Controllers/OperationsController.cs" '(?s)Authorize\(Policy = "ViewOperations"\).*outbox.*metrics' "Operational visibility lacks a business permission or metrics."
Require-Text "docs/PHASE15-OPERATIONS.md" '(?s)RPO: 6 hours.*RTO: 2 hours.*Restore rehearsal.*Production acceptance' "Recovery objectives or the production checklist are incomplete."
Require-Text "scripts/verify-backup-restore.sh" '(?s)RESTORE VERIFYONLY.*RESTORE DATABASE.*DBCC CHECKDB.*phase15-restore' "Backup restore cannot be rehearsed with verifiable evidence."
Require-Text "web/playwright.config.ts" '(?s)desktop-chrome.*mobile-chrome' "Critical E2E is not configured for desktop and mobile."
Require-Text "web/tests/e2e/critical-financial-flows.spec.ts" '(?s)login.*charge.*bank import and reconciliation.*payment application.*account statement.*offline' "The critical E2E workflow inventory is incomplete."
Require-Text "web/public/sw.js" '(?s)request\.method !== "GET".*url\.pathname\.startsWith\("/api/"\).*offline\.html' "The PWA could cache financial APIs or replay writes."
Require-Text "web/lib/api/client.ts" '(?s)!navigator\.onLine.*rest\.method.*!== "GET".*reintentar.*autom' "Offline financial writes are not explicitly blocked."
Require-Text "web/components/system/pwa-status.tsx" '(?s)Sin conexi.*system\.pwa\.update.*systemRequired' "PWA state feedback lacks its stable control policy or offline explanation."
Require-Text "web/lib/offline/drafts.ts" '(?s)saveExplicitDraft.*scope.*removeExplicitDraft' "Explicit, scoped offline drafts are unavailable."
Require-Text "tests/AegiFinance.LedgerTests/Program.cs" '(?s)AutomationRules\.IsReminderDay.*RenewalEnd.*RetryDelay' "Automation cadence and retry bounds lack regression coverage."

& (Join-Path $PSScriptRoot "verify-deployment.ps1")
if (-not $?) { $errors.Add("Docker deployment contract must remain valid in Phase 15.") }

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 15 statically verified: durable automation, tenant outbox, recovery rehearsal, observability, desktop/mobile E2E contract, safe PWA and deployment controls are present."
