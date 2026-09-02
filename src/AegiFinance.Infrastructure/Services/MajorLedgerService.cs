using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public sealed class MajorLedgerService : IMajorLedgerService
{
    private const string ReceivableCode = "1200";
    private const string TaxCode = "2100";
    private const string OpeningCode = "3100";
    private const string RevenueCode = "4100";
    private const string ExpenseCode = "5100";
    private const string DifferenceCode = "5900";

    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MajorLedgerService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task EnsureBaseChartAsync(CancellationToken cancellationToken = default)
    {
        var templates = new[]
        {
            (ReceivableCode, "Accounts receivable", GeneralLedgerAccountType.Asset, GeneralLedgerAccountPurpose.AccountsReceivable),
            (TaxCode, "Taxes payable", GeneralLedgerAccountType.Liability, GeneralLedgerAccountPurpose.Tax),
            (OpeningCode, "Opening balance equity", GeneralLedgerAccountType.Equity, GeneralLedgerAccountPurpose.OpeningBalance),
            (RevenueCode, "Service revenue", GeneralLedgerAccountType.Revenue, GeneralLedgerAccountPurpose.Revenue),
            (ExpenseCode, "Operating expenses", GeneralLedgerAccountType.Expense, GeneralLedgerAccountPurpose.Expense),
            (DifferenceCode, "Reconciliation differences", GeneralLedgerAccountType.Expense, GeneralLedgerAccountPurpose.Difference)
        };

        var currencies = await _context.CurrencyConfigs.AsNoTracking().Where(item => item.IsActive)
            .Select(item => item.Code).ToListAsync(cancellationToken);
        currencies.AddRange(await _context.BankAccounts.AsNoTracking().Select(item => item.Currency).Distinct().ToListAsync(cancellationToken));
        currencies = currencies.Distinct(StringComparer.OrdinalIgnoreCase).Select(item => item.ToUpperInvariant()).ToList();
        if (currencies.Count == 0) currencies.Add("MXN");

        var existingCodes = await _context.GeneralLedgerAccounts
            .IgnoreQueryFilters().Select(item => item.Code).ToListAsync(cancellationToken);
        var defaults = currencies.SelectMany(currency => templates.Select(item => new
        {
            Code = $"{item.Item1}-{currency}", Name = item.Item2, Type = item.Item3, Purpose = item.Item4, Currency = currency
        }));
        foreach (var item in defaults.Where(item => !existingCodes.Contains(item.Code, StringComparer.Ordinal)))
        {
            _context.GeneralLedgerAccounts.Add(new GeneralLedgerAccount
            {
                Id = Guid.NewGuid(), Code = item.Code, Name = item.Name,
                AccountType = item.Type, Purpose = item.Purpose,
                Currency = item.Currency, IsSystem = true, IsActive = true
            });
        }

        var linkedBankIds = await _context.GeneralLedgerAccounts.IgnoreQueryFilters()
            .Where(item => item.BankAccountId.HasValue)
            .Select(item => item.BankAccountId!.Value).ToListAsync(cancellationToken);
        var banks = await _context.BankAccounts.IgnoreQueryFilters()
            .Where(item => !item.IsDeleted && !linkedBankIds.Contains(item.Id)).ToListAsync(cancellationToken);
        foreach (var bank in banks)
        {
            _context.GeneralLedgerAccounts.Add(new GeneralLedgerAccount
            {
                Id = Guid.NewGuid(),
                Code = $"1100-{bank.Id.ToString("N")[..8].ToUpperInvariant()}",
                Name = $"Bank · {bank.Name}",
                AccountType = GeneralLedgerAccountType.Asset,
                Purpose = GeneralLedgerAccountPurpose.Bank,
                BankAccountId = bank.Id,
                Currency = bank.Currency,
                IsSystem = true,
                IsActive = bank.IsActive
            });
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<GeneralLedgerAccountDto>> GetAccountsAsync(DateTime? asOfDate, CancellationToken cancellationToken = default)
    {
        await EnsureBaseChartAsync(cancellationToken);
        var cutoff = (asOfDate ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
        var accounts = await _context.GeneralLedgerAccounts.AsNoTracking().OrderBy(item => item.Code).ToListAsync(cancellationToken);
        var movements = await _context.JournalLines.AsNoTracking()
            .Where(line => line.JournalEntry.Date <= cutoff && line.JournalEntry.Status != JournalEntryStatus.Draft)
            .GroupBy(line => line.AccountId)
            .Select(group => new { AccountId = group.Key, Debit = group.Sum(line => line.Debit), Credit = group.Sum(line => line.Credit) })
            .ToDictionaryAsync(item => item.AccountId, cancellationToken);

        return accounts.Select(account =>
        {
            movements.TryGetValue(account.Id, out var movement);
            var debit = movement?.Debit ?? 0;
            var credit = movement?.Credit ?? 0;
            return new GeneralLedgerAccountDto(account.Id, account.Code, account.Name, account.AccountType.ToString(),
                account.Purpose.ToString(), account.Currency, account.IsSystem, account.IsActive,
                account.ParentAccountId, account.BankAccountId, NormalBalance(account.AccountType, debit, credit));
        }).ToList();
    }

    public async Task<IReadOnlyList<AccountingPeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken = default)
        => await _context.AccountingPeriods.AsNoTracking().OrderByDescending(item => item.StartDate)
            .Select(item => new AccountingPeriodDto(item.Id, item.Name, item.StartDate, item.EndDate, item.Status.ToString(), item.ClosedAt,
                item.ClosedBy, item.CloseVerificationCode, item.ReopenedAt, item.ReopenedBy, item.ReopenReason, item.GovernanceVersion))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<JournalEntryDto>> GetJournalEntriesAsync(DateTime? from, DateTime? to, string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.JournalEntries.AsNoTracking()
            .Include(item => item.AccountingPeriod).Include(item => item.Lines).ThenInclude(line => line.Account)
            .AsQueryable();
        if (from.HasValue) query = query.Where(item => item.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(item => item.Date < to.Value.Date.AddDays(1));
        if (Enum.TryParse<JournalEntryStatus>(status, true, out var parsed)) query = query.Where(item => item.Status == parsed);
        var entries = await query.OrderByDescending(item => item.Date).ThenByDescending(item => item.EntryNumber)
            .Take(500).ToListAsync(cancellationToken);
        return entries.Select(MapEntry).ToList();
    }

    public async Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime asOfDate, string currency, CancellationToken cancellationToken = default)
    {
        var cutoff = asOfDate.Date.AddDays(1);
        var rows = await _context.JournalLines.AsNoTracking()
            .Where(line => line.JournalEntry.Date < cutoff && line.JournalEntry.Currency == currency && line.JournalEntry.Status != JournalEntryStatus.Draft)
            .GroupBy(line => new { line.AccountId, line.Account.Code, line.Account.Name, line.Account.AccountType })
            .Select(group => new TrialBalanceLineDto(group.Key.AccountId, group.Key.Code, group.Key.Name,
                group.Key.AccountType.ToString(), group.Sum(line => line.Debit), group.Sum(line => line.Credit),
                group.Key.AccountType == GeneralLedgerAccountType.Asset || group.Key.AccountType == GeneralLedgerAccountType.Expense
                    ? group.Sum(line => line.Debit - line.Credit)
                    : group.Sum(line => line.Credit - line.Debit)))
            .OrderBy(item => item.AccountCode).ToListAsync(cancellationToken);
        var totalDebit = rows.Sum(item => item.Debit);
        var totalCredit = rows.Sum(item => item.Credit);
        return new TrialBalanceDto(asOfDate.Date, currency, totalDebit, totalCredit, totalDebit == totalCredit, rows);
    }

    public async Task<IReadOnlyList<AccountLedgerLineDto>> GetAccountLedgerAsync(Guid accountId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var account = await _context.GeneralLedgerAccounts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == accountId, cancellationToken)
            ?? throw new InvalidOperationException("The general ledger account does not exist.");
        var query = _context.JournalLines.AsNoTracking().Include(line => line.JournalEntry)
            .Where(line => line.AccountId == accountId && line.JournalEntry.Status != JournalEntryStatus.Draft);
        if (from.HasValue) query = query.Where(line => line.JournalEntry.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(line => line.JournalEntry.Date < to.Value.Date.AddDays(1));
        var lines = await query.OrderBy(line => line.JournalEntry.Date).ThenBy(line => line.JournalEntry.EntryNumber).ToListAsync(cancellationToken);
        decimal running = 0;
        return lines.Select(line =>
        {
            running += NormalBalance(account.AccountType, line.Debit, line.Credit);
            return new AccountLedgerLineDto(line.JournalEntryId, line.JournalEntry.EntryNumber, line.JournalEntry.Date,
                line.JournalEntry.Description, line.JournalEntry.Status.ToString(), line.Debit, line.Credit,
                running, line.ClientId, line.LegacyLedgerEntryId);
        }).ToList();
    }

    public async Task<JournalEntryDto> CreateDraftAsync(CreateJournalEntryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new InvalidOperationException("An idempotency key is required.");
        var existing = await LoadByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing is not null) return MapEntry(existing);
        var period = await GetOpenPeriodAsync(request.Date, cancellationToken);
        var accountIds = request.Lines.Select(line => line.AccountId).Distinct().ToList();
        var activeCount = await _context.GeneralLedgerAccounts.CountAsync(item => accountIds.Contains(item.Id) && item.IsActive, cancellationToken);
        if (activeCount != accountIds.Count) throw new InvalidOperationException("Every journal line must use an active account.");
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(), EntryNumber = NewEntryNumber(request.Date), Date = request.Date,
            Description = request.Description.Trim(), Reference = request.Reference?.Trim(),
            Currency = request.Currency.ToUpperInvariant(), Status = JournalEntryStatus.Draft,
            SourceType = JournalSourceType.Manual, IdempotencyKey = request.IdempotencyKey.Trim(),
            AccountingPeriodId = period.Id, ClientId = request.ClientId,
            Lines = request.Lines.Select(line => new JournalLine
            {
                Id = Guid.NewGuid(), AccountId = line.AccountId, Debit = line.Debit, Credit = line.Credit,
                Description = line.Description?.Trim(), ClientId = line.ClientId ?? request.ClientId,
                BankAccountId = line.BankAccountId, BillingItemId = line.BillingItemId
            }).ToList()
        };
        JournalEntryRules.ValidateForPosting(entry);
        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        return MapEntry(await LoadEntryAsync(entry.Id, cancellationToken));
    }

    public async Task<JournalEntryDto> PostAsync(Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        var entry = await _context.JournalEntries.Include(item => item.AccountingPeriod).Include(item => item.Lines)
            .FirstOrDefaultAsync(item => item.Id == journalEntryId, cancellationToken)
            ?? throw new InvalidOperationException("The journal entry does not exist.");
        if (entry.Status != JournalEntryStatus.Draft) throw new InvalidOperationException("Only draft journal entries can be posted.");
        if (entry.AccountingPeriod.Status != AccountingPeriodStatus.Open) throw new InvalidOperationException("The accounting period is closed.");
        JournalEntryRules.ValidateForPosting(entry);
        entry.Status = JournalEntryStatus.Posted;
        entry.PostedAt = DateTime.UtcNow;
        entry.PostedBy = _currentUser.UserId;
        await _context.SaveChangesAsync(cancellationToken);
        return MapEntry(await LoadEntryAsync(entry.Id, cancellationToken));
    }

    public async Task<JournalEntryDto> ReverseAsync(Guid journalEntryId, DateTime reversalDate, string reason, CancellationToken cancellationToken = default)
    {
        var original = await _context.JournalEntries.Include(item => item.Lines)
            .FirstOrDefaultAsync(item => item.Id == journalEntryId, cancellationToken)
            ?? throw new InvalidOperationException("The journal entry does not exist.");
        var existing = await LoadByIdempotencyKeyAsync($"reversal:{journalEntryId}", cancellationToken);
        if (existing is not null) return MapEntry(existing);
        var period = await GetOpenPeriodAsync(reversalDate, cancellationToken);
        var reversal = JournalEntryRules.CreateReversal(original, period.Id, NewEntryNumber(reversalDate),
            $"reversal:{journalEntryId}", reversalDate, _currentUser.UserId, reason);
        original.Status = JournalEntryStatus.Reversed;
        original.ReversedAt = DateTime.UtcNow;
        original.ReversedBy = _currentUser.UserId;
        _context.JournalEntries.Add(reversal);
        await _context.SaveChangesAsync(cancellationToken);
        return MapEntry(await LoadEntryAsync(reversal.Id, cancellationToken));
    }

    public async Task<AccountingPeriodDto> CreatePeriodAsync(string name, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        if (endDate.Date < startDate.Date) throw new InvalidOperationException("The period end date must be on or after its start date.");
        var overlaps = await _context.AccountingPeriods.AnyAsync(item => startDate.Date <= item.EndDate && endDate.Date >= item.StartDate, cancellationToken);
        if (overlaps) throw new InvalidOperationException("The accounting period overlaps an existing period.");
        var period = new AccountingPeriod { Id = Guid.NewGuid(), OrganizationId = RequireOrganizationId(), Name = name.Trim(), StartDate = startDate.Date, EndDate = endDate.Date.AddDays(1).AddTicks(-1) };
        _context.AccountingPeriods.Add(period);
        await _context.SaveChangesAsync(cancellationToken);
        return MapPeriod(period);
    }

    public async Task<LegacyMigrationResultDto> MigrateLegacyAsync(CancellationToken cancellationToken = default)
    {
        await EnsureBaseChartAsync(cancellationToken);
        var alreadyMigrated = await _context.JournalLines.CountAsync(line => line.LegacyLedgerEntryId.HasValue, cancellationToken);
        var migrated = 0;
        var transfers = await _context.TransferGroups.Include(item => item.FromEntry).Include(item => item.ToEntry).ToListAsync(cancellationToken);
        foreach (var transfer in transfers)
        {
            if (await IsLegacyMigratedAsync(transfer.FromEntryId, cancellationToken) || await IsLegacyMigratedAsync(transfer.ToEntryId, cancellationToken)) continue;
            await PostTransferAsync(transfer.FromEntry, transfer.ToEntry, cancellationToken);
            migrated += 2;
        }

        var pending = await _context.LedgerEntries.AsNoTracking()
            .Where(item => !_context.JournalLines.Any(line => line.LegacyLedgerEntryId == item.Id))
            .OrderBy(item => item.Date).ToListAsync(cancellationToken);
        foreach (var entry in pending)
        {
            await PostLegacyEntryAsync(entry, cancellationToken);
            migrated++;
        }

        var banks = await _context.BankAccounts.AsNoTracking().Where(item => item.OpeningBalance != 0).ToListAsync(cancellationToken);
        foreach (var bank in banks) await PostOpeningBalanceAsync(bank, cancellationToken);

        var legacyEntries = await _context.LedgerEntries.AsNoTracking().ToListAsync(cancellationToken);
        var legacyTotal = banks.Sum(item => item.OpeningBalance) + legacyEntries.Sum(SignedLegacyAmount);
        var journalBankTotal = await _context.JournalLines.Where(line => line.BankAccountId.HasValue &&
                (line.LegacyLedgerEntryId.HasValue || line.JournalEntry.SourceType == JournalSourceType.OpeningBalance))
            .SumAsync(line => line.Debit - line.Credit, cancellationToken);
        var difference = legacyTotal - journalBankTotal;
        return new LegacyMigrationResultDto(migrated, alreadyMigrated, legacyTotal, journalBankTotal, difference, difference == 0);
    }

    public async Task<JournalEntry> PostLegacyEntryAsync(LedgerEntry ledgerEntry, CancellationToken cancellationToken = default)
    {
        await EnsureBaseChartAsync(cancellationToken);
        var existing = await LoadByIdempotencyKeyAsync($"legacy:{ledgerEntry.Id}", cancellationToken);
        if (existing is not null) return existing;
        var bank = await GetBankAccountAsync(ledgerEntry.BankAccountId, cancellationToken);
        var amount = Math.Abs(ledgerEntry.Amount);
        var counterCode = ledgerEntry.EntryType switch
        {
            LedgerEntryType.Income when ledgerEntry.ClientId.HasValue || ledgerEntry.BillingItemId.HasValue => ReceivableCode,
            LedgerEntryType.Income => RevenueCode,
            LedgerEntryType.Expense => ExpenseCode,
            _ => DifferenceCode
        };
        var counter = await GetAccountByCodeAsync(counterCode, ledgerEntry.Currency, cancellationToken);
        var bankIncrease = ledgerEntry.EntryType is LedgerEntryType.Income or LedgerEntryType.TransferIn ||
            ledgerEntry.EntryType == LedgerEntryType.Adjustment && ledgerEntry.Amount > 0;
        var lines = bankIncrease
            ? new[] { Line(bank.Id, amount, 0, ledgerEntry), Line(counter.Id, 0, amount, ledgerEntry, false) }
            : new[] { Line(counter.Id, amount, 0, ledgerEntry, false), Line(bank.Id, 0, amount, ledgerEntry) };
        return await AddPostedEntryAsync(ledgerEntry.Date, ledgerEntry.Description, ledgerEntry.Reference, ledgerEntry.Currency,
            ledgerEntry.ClientId, MapSource(ledgerEntry.EntryType), ledgerEntry.Id.ToString(), $"legacy:{ledgerEntry.Id}", lines, cancellationToken);
    }

    public async Task<JournalEntry> PostTransferAsync(LedgerEntry fromEntry, LedgerEntry toEntry, CancellationToken cancellationToken = default)
    {
        await EnsureBaseChartAsync(cancellationToken);
        var key = $"transfer:{fromEntry.Id}:{toEntry.Id}";
        var existing = await LoadByIdempotencyKeyAsync(key, cancellationToken);
        if (existing is not null) return existing;
        var from = await GetBankAccountAsync(fromEntry.BankAccountId, cancellationToken);
        var to = await GetBankAccountAsync(toEntry.BankAccountId, cancellationToken);
        var amount = Math.Abs(fromEntry.Amount);
        return await AddPostedEntryAsync(fromEntry.Date, fromEntry.Description, fromEntry.Reference, fromEntry.Currency, null,
            JournalSourceType.Transfer, $"{fromEntry.Id}:{toEntry.Id}", key,
            new[] { Line(to.Id, amount, 0, toEntry), Line(from.Id, 0, amount, fromEntry) }, cancellationToken);
    }

    public async Task<JournalEntry> PostChargeAsync(BillingItem billingItem, CancellationToken cancellationToken = default)
    {
        await EnsureBaseChartAsync(cancellationToken);
        var key = $"charge:{billingItem.Id}";
        var existing = await LoadByIdempotencyKeyAsync(key, cancellationToken);
        if (existing is not null) return existing;
        var receivable = await GetAccountByCodeAsync(ReceivableCode, billingItem.Currency, cancellationToken);
        var revenue = await GetAccountByCodeAsync(RevenueCode, billingItem.Currency, cancellationToken);
        return await AddPostedEntryAsync(billingItem.GeneratedAt, billingItem.Description, null, billingItem.Currency, billingItem.ClientId,
            JournalSourceType.Charge, billingItem.Id.ToString(), key,
            new[]
            {
                new JournalLine { Id = Guid.NewGuid(), AccountId = receivable.Id, Debit = billingItem.Amount, ClientId = billingItem.ClientId, BillingItemId = billingItem.Id },
                new JournalLine { Id = Guid.NewGuid(), AccountId = revenue.Id, Credit = billingItem.Amount, ClientId = billingItem.ClientId, BillingItemId = billingItem.Id }
            }, cancellationToken);
    }

    public async Task<JournalEntry> PostBillingAdjustmentAsync(BillingAdjustment adjustment, CancellationToken cancellationToken = default)
    {
        await EnsureBaseChartAsync(cancellationToken);
        var item = adjustment.BillingItem;
        var key = $"billing-adjustment:{adjustment.Id}";
        var existing = await LoadByIdempotencyKeyAsync(key, cancellationToken);
        if (existing is not null) return existing;
        var receivable = await GetAccountByCodeAsync(ReceivableCode, item.Currency, cancellationToken);
        var revenue = await GetAccountByCodeAsync(RevenueCode, item.Currency, cancellationToken);
        var lines = adjustment.Type == BillingAdjustmentType.CreditNote
            ? new[]
            {
                new JournalLine { Id = Guid.NewGuid(), AccountId = revenue.Id, Debit = adjustment.Amount, ClientId = item.ClientId, BillingItemId = item.Id },
                new JournalLine { Id = Guid.NewGuid(), AccountId = receivable.Id, Credit = adjustment.Amount, ClientId = item.ClientId, BillingItemId = item.Id }
            }
            : new[]
            {
                new JournalLine { Id = Guid.NewGuid(), AccountId = receivable.Id, Debit = adjustment.Amount, ClientId = item.ClientId, BillingItemId = item.Id },
                new JournalLine { Id = Guid.NewGuid(), AccountId = revenue.Id, Credit = adjustment.Amount, ClientId = item.ClientId, BillingItemId = item.Id }
            };
        return await AddPostedEntryAsync(adjustment.EffectiveDate, $"{adjustment.Type}: {adjustment.Reason}", null,
            item.Currency, item.ClientId, JournalSourceType.Adjustment, adjustment.Id.ToString(), key, lines, cancellationToken);
    }

    public async Task<JournalEntry?> PostOpeningBalanceAsync(BankAccount bankAccount, CancellationToken cancellationToken = default)
    {
        if (bankAccount.OpeningBalance == 0) return null;
        await EnsureBaseChartAsync(cancellationToken);
        var key = $"opening:{bankAccount.Id}";
        var existing = await LoadByIdempotencyKeyAsync(key, cancellationToken);
        if (existing is not null) return existing;
        var bank = await GetBankAccountAsync(bankAccount.Id, cancellationToken);
        var opening = await GetAccountByCodeAsync(OpeningCode, bankAccount.Currency, cancellationToken);
        var amount = Math.Abs(bankAccount.OpeningBalance);
        var lines = bankAccount.OpeningBalance > 0
            ? new[] { new JournalLine { Id = Guid.NewGuid(), AccountId = bank.Id, Debit = amount, BankAccountId = bankAccount.Id }, new JournalLine { Id = Guid.NewGuid(), AccountId = opening.Id, Credit = amount } }
            : new[] { new JournalLine { Id = Guid.NewGuid(), AccountId = opening.Id, Debit = amount }, new JournalLine { Id = Guid.NewGuid(), AccountId = bank.Id, Credit = amount, BankAccountId = bankAccount.Id } };
        return await AddPostedEntryAsync(bankAccount.OpeningDate, $"Opening balance · {bankAccount.Name}", null, bankAccount.Currency, null,
            JournalSourceType.OpeningBalance, bankAccount.Id.ToString(), key, lines, cancellationToken);
    }

    private async Task<JournalEntry> AddPostedEntryAsync(DateTime date, string description, string? reference, string currency,
        Guid? clientId, JournalSourceType sourceType, string sourceId, string key, IEnumerable<JournalLine> lines, CancellationToken cancellationToken)
    {
        var period = await GetOpenPeriodAsync(date, cancellationToken);
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(), EntryNumber = NewEntryNumber(date), Date = date, Description = description,
            Reference = reference, Currency = currency, Status = JournalEntryStatus.Posted, SourceType = sourceType,
            SourceId = sourceId, IdempotencyKey = key, AccountingPeriodId = period.Id, ClientId = clientId,
            PostedAt = DateTime.UtcNow, PostedBy = _currentUser.UserId, Lines = lines.ToList()
        };
        foreach (var line in entry.Lines) line.ClientId ??= clientId;
        JournalEntryRules.ValidateForPosting(entry);
        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        return entry;
    }

    private async Task<AccountingPeriod> GetOpenPeriodAsync(DateTime date, CancellationToken cancellationToken)
    {
        var period = await _context.AccountingPeriods.FirstOrDefaultAsync(item => item.StartDate <= date && item.EndDate >= date, cancellationToken);
        if (period is null)
        {
            var start = new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind == DateTimeKind.Unspecified ? DateTimeKind.Utc : date.Kind);
            period = new AccountingPeriod { Id = Guid.NewGuid(), OrganizationId = RequireOrganizationId(), Name = start.ToString("yyyy-MM"), StartDate = start, EndDate = start.AddMonths(1).AddTicks(-1) };
            _context.AccountingPeriods.Add(period);
            await _context.SaveChangesAsync(cancellationToken);
        }
        if (period.Status != AccountingPeriodStatus.Open) throw new InvalidOperationException($"Accounting period {period.Name} is closed.");
        return period;
    }

    private async Task<GeneralLedgerAccount> GetBankAccountAsync(Guid bankAccountId, CancellationToken cancellationToken)
        => await _context.GeneralLedgerAccounts.FirstOrDefaultAsync(item => item.BankAccountId == bankAccountId, cancellationToken)
           ?? throw new InvalidOperationException("The bank account is not linked to the chart of accounts.");

    private async Task<GeneralLedgerAccount> GetAccountByCodeAsync(string baseCode, string currency, CancellationToken cancellationToken)
        => await _context.GeneralLedgerAccounts.FirstAsync(item => item.Code == $"{baseCode}-{currency.ToUpperInvariant()}", cancellationToken);

    private async Task<bool> IsLegacyMigratedAsync(Guid id, CancellationToken cancellationToken)
        => await _context.JournalLines.AnyAsync(line => line.LegacyLedgerEntryId == id, cancellationToken);

    private async Task<JournalEntry?> LoadByIdempotencyKeyAsync(string key, CancellationToken cancellationToken)
        => await _context.JournalEntries.Include(item => item.AccountingPeriod).Include(item => item.Lines).ThenInclude(line => line.Account)
            .FirstOrDefaultAsync(item => item.IdempotencyKey == key, cancellationToken);

    private async Task<JournalEntry> LoadEntryAsync(Guid id, CancellationToken cancellationToken)
        => await _context.JournalEntries.AsNoTracking().Include(item => item.AccountingPeriod).Include(item => item.Lines).ThenInclude(line => line.Account)
            .FirstAsync(item => item.Id == id, cancellationToken);

    private static JournalLine Line(Guid accountId, decimal debit, decimal credit, LedgerEntry legacy, bool bankLine = true) => new()
    {
        Id = Guid.NewGuid(), AccountId = accountId, Debit = debit, Credit = credit,
        ClientId = legacy.ClientId, BillingItemId = legacy.BillingItemId,
        BankAccountId = bankLine ? legacy.BankAccountId : null,
        LegacyLedgerEntryId = bankLine ? legacy.Id : null
    };

    private static decimal SignedLegacyAmount(LedgerEntry item) => item.EntryType switch
    {
        LedgerEntryType.Income or LedgerEntryType.TransferIn => Math.Abs(item.Amount),
        LedgerEntryType.Expense or LedgerEntryType.TransferOut => -Math.Abs(item.Amount),
        _ => item.Amount
    };

    private static JournalSourceType MapSource(LedgerEntryType type) => type switch
    {
        LedgerEntryType.Income => JournalSourceType.Payment,
        LedgerEntryType.Expense => JournalSourceType.Expense,
        LedgerEntryType.TransferIn or LedgerEntryType.TransferOut => JournalSourceType.Transfer,
        _ => JournalSourceType.Adjustment
    };

    private static decimal NormalBalance(GeneralLedgerAccountType type, decimal debit, decimal credit)
        => type is GeneralLedgerAccountType.Asset or GeneralLedgerAccountType.Expense ? debit - credit : credit - debit;

    private static string NewEntryNumber(DateTime date) => $"JE-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    private Guid RequireOrganizationId() => _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No active organization is available.");

    private static AccountingPeriodDto MapPeriod(AccountingPeriod period) => new(period.Id, period.Name, period.StartDate, period.EndDate,
        period.Status.ToString(), period.ClosedAt, period.ClosedBy, period.CloseVerificationCode, period.ReopenedAt,
        period.ReopenedBy, period.ReopenReason, period.GovernanceVersion);

    private static JournalEntryDto MapEntry(JournalEntry entry)
    {
        var lines = entry.Lines.Select(line => new JournalLineDto(line.Id, line.AccountId, line.Account?.Code ?? string.Empty,
            line.Account?.Name ?? string.Empty, line.Debit, line.Credit, line.Description, line.ClientId,
            line.BankAccountId, line.BillingItemId, line.LegacyLedgerEntryId)).ToList();
        return new JournalEntryDto(entry.Id, entry.EntryNumber, entry.Date, entry.Description, entry.Reference,
            entry.Currency, entry.Status.ToString(), entry.SourceType.ToString(), entry.SourceId,
            entry.AccountingPeriodId, entry.AccountingPeriod?.Name ?? string.Empty, entry.ClientId,
            entry.PostedAt, entry.ReversedAt, entry.ReversesJournalEntryId,
            lines.Sum(line => line.Debit), lines.Sum(line => line.Credit), lines);
    }
}
