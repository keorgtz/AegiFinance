using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class LedgerService : ILedgerService
{
    private readonly ApplicationDbContext _context;

    public LedgerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerEntry> RegisterIncomeAsync(RegisterIncomeRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateBankAccountAsync(request.BankAccountId, cancellationToken);

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

        return entry;
    }

    public async Task<LedgerEntry> RegisterExpenseAsync(RegisterExpenseRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateBankAccountAsync(request.BankAccountId, cancellationToken);

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

        return entry;
    }

    public async Task<TransferGroup> RegisterTransferAsync(RegisterTransferRequest request, CancellationToken cancellationToken = default)
    {
        if (request.FromBankAccountId == request.ToBankAccountId)
        {
            throw new InvalidOperationException("La cuenta de origen y destino deben ser diferentes.");
        }

        await ValidateBankAccountAsync(request.FromBankAccountId, cancellationToken);
        await ValidateBankAccountAsync(request.ToBankAccountId, cancellationToken);

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
        await ValidateBankAccountAsync(request.BankAccountId, cancellationToken);

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

        return entry;
    }

    public async Task ReconcileAsync(Guid ledgerEntryId, CancellationToken cancellationToken = default)
    {
        var entry = await _context.LedgerEntries
            .FirstOrDefaultAsync(le => le.Id == ledgerEntryId, cancellationToken);

        if (entry is null)
        {
            throw new InvalidOperationException("El movimiento no existe.");
        }

        if (entry.IsReconciled)
        {
            throw new InvalidOperationException("El movimiento ya está conciliado.");
        }

        entry.IsReconciled = true;
        entry.ReconciledAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnreconcileAsync(Guid ledgerEntryId, CancellationToken cancellationToken = default)
    {
        var entry = await _context.LedgerEntries
            .FirstOrDefaultAsync(le => le.Id == ledgerEntryId, cancellationToken);

        if (entry is null)
        {
            throw new InvalidOperationException("El movimiento no existe.");
        }

        if (!entry.IsReconciled)
        {
            throw new InvalidOperationException("El movimiento no está conciliado.");
        }

        entry.IsReconciled = false;
        entry.ReconciledAt = null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateBankAccountAsync(Guid bankAccountId, CancellationToken cancellationToken)
    {
        var exists = await _context.BankAccounts
            .AsNoTracking()
            .AnyAsync(ba => ba.Id == bankAccountId, cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException("La cuenta bancaria no existe.");
        }
    }
}
