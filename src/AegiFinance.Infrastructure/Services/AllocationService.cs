using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Accounting;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class AllocationService : IAllocationService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AllocationService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AllocationResult> AutoAllocateAsync(Guid ledgerEntryId, CancellationToken cancellationToken = default)
    {
        var entry = await LoadIncomeEntryAsync(ledgerEntryId, cancellationToken);
        if (entry.ClientId is null)
            throw new InvalidOperationException("El ingreso debe estar asociado a un cliente para auto-asignarse.");

        var allocated = await GetAllocatedAmountAsync(ledgerEntryId, cancellationToken);
        var available = entry.Amount - allocated;
        var result = new AllocationResult { Success = true, AllocatedAmount = 0, RemainingAmount = available };
        if (available <= 0) return result;

        var items = await _context.BillingItems
            .Include(x => x.Adjustments)
            .Include(x => x.PaymentPromises)
            .Where(x => x.ClientId == entry.ClientId.Value && (x.Status == BillingItemStatus.Pending || x.Status == BillingItemStatus.Partial))
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.GeneratedAt)
            .ToListAsync(cancellationToken);

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        foreach (var item in items)
        {
            if (available <= 0) break;
            var remainingItem = ReceivableRules.Balance(item);
            if (remainingItem <= 0) continue;
            var amount = Math.Min(available, remainingItem);
            await CreateAllocationAsync(entry, item, amount, true, cancellationToken);
            available -= amount;
            result.AllocatedAmount += amount;
        }

        result.RemainingAmount = available;
        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<AllocationResult> ManualAllocateAsync(Guid ledgerEntryId, IReadOnlyList<ManualAllocationRequest> allocations, CancellationToken cancellationToken = default)
    {
        var entry = await LoadIncomeEntryAsync(ledgerEntryId, cancellationToken);
        var allocated = await GetAllocatedAmountAsync(ledgerEntryId, cancellationToken);
        var available = entry.Amount - allocated;
        var requested = allocations.Sum(x => x.Amount);
        if (requested > available)
            throw new InvalidOperationException("La suma de asignaciones supera el monto disponible del pago.");

        var itemIds = allocations.Select(x => x.BillingItemId).Distinct().ToList();
        var items = await _context.BillingItems.Include(x => x.Adjustments).Include(x => x.PaymentPromises).Where(x => itemIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var allocation in allocations)
        {
            if (!items.TryGetValue(allocation.BillingItemId, out var item))
                throw new InvalidOperationException("Uno de los cargos no existe.");

            var remainingItem = ReceivableRules.Balance(item);
            if (allocation.Amount > remainingItem)
                throw new InvalidOperationException("La asignación supera el saldo pendiente del cargo.");

            await CreateAllocationAsync(entry, item, allocation.Amount, false, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new AllocationResult
        {
            Success = true,
            AllocatedAmount = requested,
            RemainingAmount = available - requested
        };
    }

    public async Task UnallocateAsync(Guid allocationId, CancellationToken cancellationToken = default)
    {
        var allocation = await _context.SubscriptionAllocations
            .Include(x => x.LedgerEntry)
            .Include(x => x.BillingItem).ThenInclude(x => x.Adjustments)
            .FirstOrDefaultAsync(x => x.Id == allocationId, cancellationToken)
            ?? throw new InvalidOperationException("La asignación no existe.");

        if (allocation.LedgerEntry.IsReconciled)
            throw new InvalidOperationException("No se puede desasignar un movimiento conciliado.");

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        allocation.BillingItem.PaidAmount -= allocation.Amount;
        if (allocation.BillingItem.PaidAmount < 0) allocation.BillingItem.PaidAmount = 0;
        UpdateBillingItemStatus(allocation.BillingItem);
        _context.SubscriptionAllocations.Remove(allocation);
        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private async Task<LedgerEntry> LoadIncomeEntryAsync(Guid ledgerEntryId, CancellationToken cancellationToken)
    {
        var entry = await _context.LedgerEntries.FirstOrDefaultAsync(x => x.Id == ledgerEntryId, cancellationToken)
            ?? throw new InvalidOperationException("El movimiento no existe.");
        if (entry.EntryType != LedgerEntryType.Income)
            throw new InvalidOperationException("Solo movimientos de tipo Income pueden asignarse.");
        return entry;
    }

    private async Task<decimal> GetAllocatedAmountAsync(Guid ledgerEntryId, CancellationToken cancellationToken)
        => await _context.SubscriptionAllocations.Where(x => x.LedgerEntryId == ledgerEntryId).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

    private async Task CreateAllocationAsync(LedgerEntry entry, BillingItem item, decimal amount, bool isAutomatic, CancellationToken cancellationToken)
    {
        var allocation = new SubscriptionAllocation
        {
            Id = Guid.NewGuid(),
            LedgerEntryId = entry.Id,
            BillingItemId = item.Id,
            Amount = amount,
            AllocatedAt = DateTime.UtcNow,
            AllocatedBy = _currentUserService.UserId,
            IsAutomatic = isAutomatic
        };

        _context.SubscriptionAllocations.Add(allocation);
        item.PaidAmount += amount;
        UpdateBillingItemStatus(item);
        await Task.CompletedTask;
    }

    private static void UpdateBillingItemStatus(BillingItem item)
    {
        if (item.Status == BillingItemStatus.Cancelled) return;
        if (item.PaidAmount >= ReceivableRules.EffectiveAmount(item))
        {
            item.Status = item.PaidAmount > 0 ? BillingItemStatus.Paid : BillingItemStatus.Settled;
            foreach (var promise in item.PaymentPromises.Where(x => x.Status == PaymentPromiseStatus.Pending))
            {
                promise.Status = PaymentPromiseStatus.Fulfilled;
                promise.ResolvedAt = DateTime.UtcNow;
            }
        }
        else if (item.PaidAmount > 0)
            item.Status = BillingItemStatus.Partial;
        else
            item.Status = BillingItemStatus.Pending;
    }

}
