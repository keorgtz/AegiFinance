using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Accounting;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class LedgerService : ILedgerService
{
    private readonly ApplicationDbContext _context;
    private readonly IMajorLedgerService _majorLedger;

    public LedgerService(ApplicationDbContext context, IMajorLedgerService majorLedger)
    {
        _context = context;
        _majorLedger = majorLedger;
    }

    public async Task<LedgerEntry> RegisterIncomeAsync(RegisterIncomeRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateBankAccountAsync(request.BankAccountId, request.Currency, cancellationToken);
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var entry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            BankAccountId = request.BankAccountId,
            EntryType = LedgerEntryType.Income,
            Amount = request.Amount,
            Currency = request.Currency,
            Date = request.Date,
            Description = request.Description,
            Reference = request.Reference,
            ClientId = request.ClientId,
            BillingItemId = request.BillingItemId,
            IsReconciled = false
        };

        _context.LedgerEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        await _majorLedger.PostLegacyEntryAsync(entry, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return entry;
    }

    public async Task<LedgerEntry> RegisterExpenseAsync(RegisterExpenseRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateBankAccountAsync(request.BankAccountId, request.Currency, cancellationToken);
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var entry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            BankAccountId = request.BankAccountId,
            EntryType = LedgerEntryType.Expense,
            Amount = request.Amount,
            Currency = request.Currency,
            Date = request.Date,
            Description = request.Description,
            Reference = request.Reference,
            IsReconciled = false
        };

        _context.LedgerEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        await _majorLedger.PostLegacyEntryAsync(entry, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return entry;
    }

    public async Task<TransferGroup> RegisterTransferAsync(RegisterTransferRequest request, CancellationToken cancellationToken = default)
    {
        var sourceAccount = await GetBankAccountAsync(request.FromBankAccountId, cancellationToken);
        var destinationAccount = await GetBankAccountAsync(request.ToBankAccountId, cancellationToken);
        BankAccountRules.ValidateTransfer(sourceAccount, destinationAccount, request.Amount, request.Currency);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var description = request.Description ?? $"Transferencia entre cuentas";

            var fromEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                BankAccountId = request.FromBankAccountId,
                EntryType = LedgerEntryType.TransferOut,
                Amount = request.Amount,
                Currency = request.Currency,
                Date = request.Date,
                Description = description,
                IsReconciled = false
            };

            var toEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                BankAccountId = request.ToBankAccountId,
                EntryType = LedgerEntryType.TransferIn,
                Amount = request.Amount,
                Currency = request.Currency,
                Date = request.Date,
                Description = description,
                IsReconciled = false
            };

            _context.LedgerEntries.Add(fromEntry);
            _context.LedgerEntries.Add(toEntry);
            await _context.SaveChangesAsync(cancellationToken);

            var transferGroup = new TransferGroup
            {
                Id = Guid.NewGuid(),
                FromEntryId = fromEntry.Id,
                ToEntryId = toEntry.Id,
                Amount = request.Amount,
                Date = request.Date,
                Description = request.Description
            };

            _context.TransferGroups.Add(transferGroup);
            await _context.SaveChangesAsync(cancellationToken);
            await _majorLedger.PostTransferAsync(fromEntry, toEntry, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return transferGroup;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<LedgerEntry> RegisterAdjustmentAsync(RegisterAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateBankAccountAsync(request.BankAccountId, request.Currency, cancellationToken);
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var entry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            BankAccountId = request.BankAccountId,
            EntryType = LedgerEntryType.Adjustment,
            Amount = request.Amount,
            Currency = request.Currency,
            Date = request.Date,
            Description = $"{request.Description} - Motivo: {request.Reason}",
            IsReconciled = false
        };

        _context.LedgerEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        await _majorLedger.PostLegacyEntryAsync(entry, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return entry;
    }

    private async Task ValidateBankAccountAsync(Guid bankAccountId, string currency, CancellationToken cancellationToken)
    {
        var account = await GetBankAccountAsync(bankAccountId, cancellationToken);
        BankAccountRules.ValidateMovement(account, currency);
    }

    private async Task<BankAccount> GetBankAccountAsync(Guid bankAccountId, CancellationToken cancellationToken)
    {
        var account = await _context.BankAccounts.AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.Id == bankAccountId, cancellationToken);

        if (account is null)
            throw new InvalidOperationException("La cuenta bancaria no existe.");
        return account;
    }
}
