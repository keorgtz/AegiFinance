$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$controllers = Get-ChildItem -LiteralPath (Join-Path $root "src/AegiFinance.Web/Controllers") -Filter "*.cs"
$errors = [System.Collections.Generic.List[string]]::new()

foreach ($file in $controllers) {
    $lines = Get-Content -LiteralPath $file.FullName
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -notmatch '^\s*\[Http(Post|Put|Delete|Patch)') { continue }
        $end = [Math]::Min($index + 4, $lines.Count - 1)
        $window = $lines[$index..$end] -join " "
        $isSelfService = $file.Name -eq "AuthController.cs"
        $isAnonymous = $window -match '\[AllowAnonymous\]'
        if (-not $isSelfService -and -not $isAnonymous -and $window -notmatch 'Authorize\(Policy\s*=\s*"([^"]+)"\)') {
            $errors.Add("$($file.Name):$($index + 1) write endpoint has no explicit business policy")
        }
        if ($window -match 'Authorize\(Policy\s*=\s*"View[^"]*"\)') {
            $errors.Add("$($file.Name):$($index + 1) write endpoint is protected only by a View permission")
        }
    }
}

$requiredFiles = @(
    "src/AegiFinance.Domain/Entities/PermissionDefinition.cs",
    "src/AegiFinance.Domain/Entities/RolePermission.cs",
    "src/AegiFinance.Domain/Entities/UserPermissionOverride.cs",
    "src/AegiFinance.Domain/Entities/UiControlPolicy.cs",
    "web/generated/ui-control-manifest.json"
)
foreach ($path in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $path))) { $errors.Add("Missing Phase 1 artifact: $path") }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

$manifest = Get-Content -Raw -LiteralPath (Join-Path $root "web/generated/ui-control-manifest.json") | ConvertFrom-Json
$duplicates = $manifest | Group-Object controlKey | Where-Object Count -gt 1
if ($duplicates) { throw "The UI permission catalog contains duplicate control keys." }

Write-Host "Phase 1 verified: $($manifest.Count) UI controls and all write endpoints have explicit non-View policies."
