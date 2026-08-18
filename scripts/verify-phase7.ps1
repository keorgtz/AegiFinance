$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 7 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase6.ps1")
if (-not $?) { $errors.Add("Phase 6 gate must pass before Phase 7.") }

Require-Text "src/AegiFinance.Domain/Entities/ServiceVersion.cs" 'EffectiveFrom' "Plans do not preserve version validity."
Require-Text "src/AegiFinance.Domain/Entities/ServiceVersionConcept.cs" 'UnitPrice' "Versioned plan concepts are missing."
Require-Text "src/AegiFinance.Domain/Accounting/SubscriptionPricingRules.cs" 'DiscountAmount.*TaxAmount' "Discount, tax and proration calculations are not centralized."
Require-Text "src/AegiFinance.Domain/Entities/SubscriptionTermsVersion.cs" 'ServiceVersionId' "Subscription conditions are not versioned."
Require-Text "src/AegiFinance.Infrastructure/Services/BillingGenerationService.cs" 'SubscriptionTermsVersionId = terms\.Id' "Issued charges do not snapshot their effective terms."
Require-Text "src/AegiFinance.Application/Features/Subscriptions/Commands/RenewSubscription/RenewSubscriptionCommandHandler.cs" 'SubscriptionRenewals\.AnyAsync' "Renewal does not enforce idempotency."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" '(?s)IdempotencyKey.*IsUnique' "Renewal idempotency is not protected by a unique database index."
Require-Text "src/AegiFinance.Application/Features/Subscriptions/Commands/ChangeSubscriptionPlan/ChangeSubscriptionPlanCommandHandler.cs" 'LastBillingDate' "Plan changes can overwrite already billed periods."
Require-Text "src/AegiFinance.Web/Controllers/SubscriptionsController.cs" 'ManageSubscriptionAccess' "Subscription access management lacks its own API policy."
Require-Text "web/components/modules/services/service-version-form.tsx" 'services\.version\.publish' "Version publishing lacks a semantic UI permission key."
Require-Text "web/components/modules/subscriptions/subscription-form.tsx" 'subscriptions\.form\.proration' "Subscription pricing controls lack declarative permissions."
Require-Text "web/app/(app)/subscriptions/[id]/page.tsx" 'subscriptions\.detail\.tab\.terms' "Versioned subscription terms are not visible in the UI."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260818224818_Phase7VersionedPlans.cs" '(?s)INSERT INTO \[SubscriptionTermsVersions\].*\[SubscriptionId\]' "Phase 7 migration does not backfill immutable legacy terms."

& (Join-Path $PSScriptRoot "verify-deployment.ps1")
if (-not $?) { $errors.Add("Docker deployment contract must remain valid in Phase 7.") }

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 7 verified: versioned plans and terms, pricing snapshots, idempotent renewal, subscription access, responsive permission-aware UI and deployment contract are present."
