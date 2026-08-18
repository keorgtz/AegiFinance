$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 6 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase5.ps1")
if (-not $?) { $errors.Add("Phase 5 gate must pass before Phase 6.") }

Require-Text "src/AegiFinance.Domain/Entities/Client.cs" 'OrganizationId' "Clients are not assigned to an organization."
Require-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" 'Client\.OrganizationId == CurrentOrganizationId' "Related client queries are not organization scoped."
Require-Text "src/AegiFinance.Infrastructure/Data/Interceptors/TenantSessionContextInterceptor.cs" 'AegiFinance\.OrganizationId' "The database session does not receive organization scope."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260818220002_Phase6ClientScope.cs" 'fn_AegiFinanceOrganizationAccess' "Organization RLS was not added to the migration."
Require-Text "src/AegiFinance.Domain/Accounting/ClientIdentityRules.cs" 'NormalizationForm\.FormD' "Duplicate matching does not normalize accents."
Require-Text "src/AegiFinance.Infrastructure/Services/ClientGovernanceService.cs" 'ClientDuplicateRules' "Duplicate rules are not configurable by organization."
Require-Text "src/AegiFinance.Infrastructure/Services/ClientGovernanceService.cs" 'AuditLogs' "The client timeline is not backed by audit evidence."
Require-Text "src/AegiFinance.Infrastructure/Services/ClientDocumentService.cs" '10 \* 1024 \* 1024' "Client documents do not enforce the 10 MB limit."
Require-Text "src/AegiFinance.Web/Controllers/ClientDocumentsController.cs" 'ManageClientDocuments' "Document writes are not API-authorized."
Require-Text "web/components/modules/clients/client-form.tsx" 'clients\.form\.presentation-currency' "Commercial client fields lack semantic UI permission keys."
Require-Text "web/components/modules/clients/client-form.tsx" 'grid-cols-1.*sm:grid-cols-2' "The client form is not recomposed for phone layouts."
Require-Text "web/components/modules/clients/client-documents-panel.tsx" 'clients\.documents\.upload' "Document controls lack declarative UI permissions."
Require-Text "web/app/(app)/clients/[id]/page.tsx" 'clients\.detail\.tab\.timeline' "The auditable timeline is not exposed through a permission-aware tab."
Require-Text "docker-compose.yml" 'aegifinance-client-documents' "Document persistence is missing from Docker Compose."

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 6 verified: client scope, configurable duplicate detection, commercial terms, documents, audit timeline, responsive UI permissions and Docker persistence are present."
