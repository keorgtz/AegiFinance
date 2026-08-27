$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing deployment artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

function Forbid-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing deployment artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -match $pattern) { $errors.Add($message) }
}

function Require-IsolatedSqlFunctions([string]$path) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing deployment artifact: $path"); return }
    $content = Get-Content -Raw -LiteralPath $full
    foreach ($batch in [regex]::Matches($content, 'migrationBuilder\.Sql\("""(?<sql>.*?)"""\);', 'Singleline')) {
        $sql = $batch.Groups['sql'].Value.Trim()
        $functionCount = [regex]::Matches($sql, 'CREATE\s+FUNCTION', 'IgnoreCase').Count
        if ($functionCount -gt 1 -or ($functionCount -eq 1 -and $sql -notmatch '^CREATE\s+FUNCTION')) {
            $errors.Add("SQL functions must be isolated as the first statement of their migration batch: $path")
            return
        }
    }
}

function Require-FrontendApiRootsHaveControllers {
    $frontendRoots = Get-ChildItem -LiteralPath (Join-Path $root "web/lib/api") -Filter "*.ts" -File |
        ForEach-Object { [regex]::Matches((Get-Content -Raw -LiteralPath $_.FullName), '["''`]/(?<root>[a-z][a-z0-9-]*)') } |
        ForEach-Object { $_.Groups['root'].Value.ToLowerInvariant() } |
        Where-Object { $_ -ne 'api' } |
        Sort-Object -Unique

    $controllerRoots = Get-ChildItem -LiteralPath (Join-Path $root "src/AegiFinance.Web/Controllers") -Filter "*Controller.cs" -File |
        ForEach-Object {
            $content = Get-Content -Raw -LiteralPath $_.FullName
            $route = [regex]::Match($content, '\[Route\("api/(?<root>[^"/]+)')
            if ($route.Success) {
                $value = $route.Groups['root'].Value
                if ($value -eq '[controller]') { $_.BaseName.Replace('Controller', '').ToLowerInvariant() }
                else { $value.ToLowerInvariant() }
            }
        } |
        Sort-Object -Unique

    foreach ($frontendRoot in $frontendRoots) {
        if ($frontendRoot -notin $controllerRoots) {
            $errors.Add("Frontend API root '/$frontendRoot' has no matching controller route.")
        }
    }
}

Require-Text "docker-compose.yml" 'MSSQL_SA_PASSWORD:\s*\$\{MSSQL_SA_PASSWORD:\?' "SQL password is not required by Compose."
Require-Text "docker-compose.yml" 'MSSQL_PID:\s*\$\{MSSQL_PID:-Express\}' "Production Compose does not default to a licensed SQL edition."
Require-Text "docker-compose.yml" 'Encrypt=True;TrustServerCertificate=True' "Container SQL connections are not encrypted consistently."
Require-Text "docker-compose.yml" 'ADMIN_SEED_PASSWORD:\?' "Initial administrator password is not required."
Require-Text "docker-compose.yml" 'aegifinance-dataprotection-keys:/app/keys' "Data-protection keys are not persistent."
Require-Text "docker-compose.yml" 'aegifinance-client-documents:/app/data/client-documents' "Client documents are not persistent."
Require-Text "src/AegiFinance.Web/Dockerfile" 'dotnet restore "src/AegiFinance.Web/AegiFinance.Web.csproj"' "API Docker restore incorrectly depends on the full solution."
Require-Text "src/AegiFinance.Web/Dockerfile" 'fonts-dejavu-core' "The API image is missing the cross-platform fonts required for statement PDF generation."
Require-Text "src/AegiFinance.Infrastructure/AegiFinance.Infrastructure.csproj" 'PDFsharp-MigraDoc' "The statement PDF dependency is missing from the reproducible restore contract."
Require-Text "src/AegiFinance.Worker/Dockerfile" 'dotnet restore "src/AegiFinance.Worker/AegiFinance.Worker.csproj"' "Worker Docker restore incorrectly depends on the full solution."
Require-Text "web/Dockerfile" 'npm ci --no-audit --no-fund' "Frontend image is not using a reproducible lockfile install."
Require-Text "web/.dockerignore" 'node_modules/' "Frontend Docker context includes local dependencies."
Require-Text "src/AegiFinance.Web/Seed/SeedData.cs" 'AdminSeed:Password' "Production still has an implicit administrator password."
Require-Text "src/AegiFinance.Web/Program.cs" 'Database.CanConnectAsync' "API health does not verify database connectivity."
Require-Text "src/AegiFinance.Web/Program.cs" 'JsonStringEnumConverter' "JSON enum names are not supported, breaking frontend form contracts."
Require-Text "DEPLOY-UBUNTU.md" 'docker compose config --quiet' "Ubuntu deployment procedure is missing Compose validation."
Require-Text "scripts/bootstrap-env.sh" 'openssl rand -base64 64' "Ubuntu environment bootstrap does not generate a strong JWT secret."
Require-Text "scripts/bootstrap-env.sh" 'chmod 600' "Generated deployment secrets are not restricted to the owner."
Forbid-Text "src/AegiFinance.Infrastructure/Migrations/20260818183012_Phase1DynamicPermissions.cs" '\(\[ClientId\] IS NOT NULL OR \[SubscriptionId\] IS NOT NULL\)' "The permissions migration contains an OR expression unsupported by SQL Server filtered indexes."
Require-IsolatedSqlFunctions "src/AegiFinance.Infrastructure/Migrations/20260818183012_Phase1DynamicPermissions.cs"
Require-IsolatedSqlFunctions "src/AegiFinance.Infrastructure/Migrations/20260818220002_Phase6ClientScope.cs"
Require-IsolatedSqlFunctions "src/AegiFinance.Infrastructure/Migrations/20260818224818_Phase7VersionedPlans.cs"
Forbid-Text "src/AegiFinance.Infrastructure/Migrations/20260818224818_Phase7VersionedPlans.cs" "s\.\[CreatedAt\], NULL, NULL, NULL, 1, SYSUTCDATETIME" "The Phase 7 ServiceVersions data migration has more SELECT values than INSERT columns."
Forbid-Text "src/AegiFinance.Infrastructure/Data/AegiFinanceDbContext.cs" 'CurrentClientId\.Value' "Tenant query filters dereference a nullable client ID during parameter extraction."
Require-Text "src/AegiFinance.Web/Controllers/ClientCategoriesController.cs" '\[Route\("api/client-categories"\)\]' "Client category routes do not match the frontend API contract."
Require-Text "src/AegiFinance.Web/Controllers/ClientTagsController.cs" '\[Route\("api/client-tags"\)\]' "Client tag routes do not match the frontend API contract."
Require-Text "src/AegiFinance.Web/Controllers/ServiceCategoriesController.cs" '\[Route\("api/service-categories"\)\]' "Service category routes do not match the frontend API contract."
Require-Text "src/AegiFinance.Web/Controllers/ExchangeRatesController.cs" '\[Route\("api/exchange-rates"\)\]' "Exchange-rate routes do not match the frontend API contract."
Require-FrontendApiRootsHaveControllers

if (Get-Command docker -ErrorAction SilentlyContinue) {
    Push-Location $root
    try {
        $env:MSSQL_SA_PASSWORD = "Deployment_check_2026!"
        $env:JWT_SECRET = "Deployment_check_JWT_secret_with_more_than_32_characters"
        $env:ADMIN_SEED_PASSWORD = "Deployment_check_Admin_2026!"
        $env:CORS_ALLOWED_ORIGINS = "https://aegifinance.example.com"
        docker compose config --quiet
        if ($LASTEXITCODE -ne 0) { $errors.Add("docker compose config failed.") }
    }
    finally { Pop-Location }
}
else {
    Write-Warning "Docker is unavailable; run 'docker compose config --quiet' on the Ubuntu host before deployment."
}

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Deployment contract verified: secrets, private services, persistence, health, reproducible builds and Ubuntu instructions are present."
