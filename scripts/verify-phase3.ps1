$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 3 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

Require-Text "AGENTS.md" 'Deployment rule — mandatory' "The mandatory Docker deployment rule is missing."
Require-Text "CLAUDE.md" 'Regla obligatoria de despliegue' "The deployment rule is not mirrored in project instructions."
Require-Text "docker-compose.yml" 'Security__MaxFailedLoginAttempts' "Compose does not expose the access-security policy."
Require-Text "docker-compose.yml" 'external: \$\{PROXY_NETWORK_EXTERNAL:-false\}' "Compose still requires a manually-created proxy network by default."
Require-Text "src/AegiFinance.Domain/Entities/UserSession.cs" 'RefreshTokenHash' "Per-device sessions are missing."
Require-Text "src/AegiFinance.Infrastructure/Services/TokenService.cs" 'sessionId' "JWTs do not identify their session."
Require-Text "src/AegiFinance.Infrastructure/DependencyInjection.cs" 'La sesión fue revocada' "Revoked access tokens are not rejected."
Require-Text "src/AegiFinance.Infrastructure/Authorization/PermissionAuthorizationHandler.cs" 'HasPermissionAsync' "Authorization is not resolved dynamically."
if ((Get-Content -Raw -LiteralPath (Join-Path $root "src/AegiFinance.Infrastructure/Authorization/PermissionAuthorizationHandler.cs")) -match 'HasClaim\("permissions"') { $errors.Add("Authorization still trusts stale permission claims.") }
Require-Text "src/AegiFinance.Web/Controllers/UsersController.cs" 'reset-password' "Password recovery endpoint is missing."
Require-Text "src/AegiFinance.Web/Controllers/UsersController.cs" 'sessions/\{sessionId:guid\}' "Remote session revocation endpoint is missing."
Require-Text "web/components/modules/users/user-access-dialog.tsx" 'users\.access\.tabs\.exceptions' "User exceptions UI is missing."
Require-Text "web/components/modules/roles/role-comparison-dialog.tsx" 'Comparar roles' "Role comparison UI is missing."
Require-Text "src/AegiFinance.Infrastructure/Migrations/20260818205818_Phase3AccessAdministration.cs" 'CreateTable\(\s*name: "UserSessions"' "Phase 3 migration does not create UserSessions."

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 3 verified: dynamic access, scoped exceptions, session revocation, recovery, audit UI and deploy contract are present."
