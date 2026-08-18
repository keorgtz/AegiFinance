$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$path, [string]$pattern, [string]$message) {
    $full = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $full)) { $errors.Add("Missing Phase 5 artifact: $path"); return }
    if ((Get-Content -Raw -LiteralPath $full) -notmatch $pattern) { $errors.Add($message) }
}

& (Join-Path $PSScriptRoot "verify-phase4.ps1")
if (-not $?) { $errors.Add("Phase 4 gate must pass before Phase 5.") }

Require-Text "src/AegiFinance.Infrastructure/Services/AccountBalanceCalculator.cs" 'JournalLines' "Bank balances are not derived from the Major Ledger."
Require-Text "src/AegiFinance.Infrastructure/Services/AccountBalanceCalculator.cs" 'latestStatement.EndDate' "Bank and ledger balances are not compared at the same cutoff."
Require-Text "src/AegiFinance.Infrastructure/Services/MajorLedgerService.cs" 'PostOpeningBalanceAsync' "Opening balances are not posted as journal entries."
Require-Text "src/AegiFinance.Application/Features/BankAccounts/Commands/CreateBankAccount/CreateBankAccountCommandHandler.cs" 'BeginTransactionAsync' "Account creation and opening posting are not atomic."
Require-Text "src/AegiFinance.Domain/Accounting/BankAccountRules.cs" '!account.IsActive' "Inactive accounts are not blocked by a domain rule."
Require-Text "src/AegiFinance.Domain/Accounting/BankAccountRules.cs" 'source.Currency, destination.Currency' "Transfer currency compatibility is not enforced."
Require-Text "src/AegiFinance.Infrastructure/Services/MajorLedgerService.cs" 'JournalSourceType.Transfer' "Transfers are not represented as neutral Major Ledger entries."
Require-Text "src/AegiFinance.Domain/Entities/BankStatement.cs" 'IsBalanceVerified' "Verified bank balances cannot be distinguished from incomplete imports."
Require-Text "src/AegiFinance.Web/Controllers/BankAccountsController.cs" 'UpdateBankAccountBalances' "Recording a bank balance is not API-authorized."
Require-Text "web/app/(app)/ledger/page.tsx" 'ledger.bankAccounts.card.bankBalance' "Responsive bank-balance controls are missing."
Require-Text "web/components/modules/ledger/bank-account-form.tsx" 'ledger.bankAccounts.form.openingBalance' "Account opening fields do not have semantic UI permission keys."
Require-Text "web/components/modules/ledger/transfer-form.tsx" 'ledger.transfers.form.destination' "Transfer controls do not have semantic UI permission keys."
Require-Text "AGENTS.md" 'docker compose config' "The deployment verification rule is missing."

if ($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Phase 5 verified: Major Ledger balances, atomic openings, neutral transfers, inactive-account blocking, verified bank balances and permission-aware responsive UI are present."
