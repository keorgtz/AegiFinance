$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing deployment artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

Require-Text "docker-compose.yml" 'MSSQL_SA_PASSWORD:\s*\$\{MSSQL_SA_PASSWORD:\?' "SQL password is not required by Compose."
Require-Text "docker-compose.yml" 'MSSQL_PID:\s*\$\{MSSQL_PID:-Express\}' "Production Compose does not default to a licensed SQL edition."
Require-Text "docker-compose.yml" 'Encrypt=True;TrustServerCertificate=True' "Container SQL connections are not encrypted consistently."
Require-Text "docker-compose.yml" 'ADMIN_SEED_PASSWORD:\?' "Initial administrator password is not required."
Require-Text "docker-compose.yml" 'aegifinance-dataprotection-keys:/app/keys' "Data-protection keys are not persistent."
Require-Text "docker-compose.yml" 'aegifinance-client-documents:/app/data/client-documents' "Client documents are not persistent."
Require-Text "src/AegiFinance.Web/Dockerfile" 'dotnet restore "src/AegiFinance.Web/AegiFinance.Web.csproj"' "API Docker restore incorrectly depends on the full solution."
Require-Text "src/AegiFinance.Worker/Dockerfile" 'dotnet restore "src/AegiFinance.Worker/AegiFinance.Worker.csproj"' "Worker Docker restore incorrectly depends on the full solution."
Require-Text "web/Dockerfile" 'npm ci --no-audit --no-fund' "Frontend image is not using a reproducible lockfile install."
Require-Text "web/.dockerignore" 'node_modules/' "Frontend Docker context includes local dependencies."
Require-Text "src/AegiFinance.Web/Seed/SeedData.cs" 'AdminSeed:Password' "Production still has an implicit administrator password."
Require-Text "src/AegiFinance.Web/Program.cs" 'Database.CanConnectAsync' "API health does not verify database connectivity."
Require-Text "DEPLOY-UBUNTU.md" 'docker compose config --quiet' "Ubuntu deployment procedure is missing Compose validation."
Require-Text "scripts/bootstrap-env.sh" 'openssl rand -base64 64' "Ubuntu environment bootstrap does not generate a strong JWT secret."
Require-Text "scripts/bootstrap-env.sh" 'chmod 600' "Generated deployment secrets are not restricted to the owner."

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
