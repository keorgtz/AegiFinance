using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public sealed class AllocationService : IAllocationService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMajorLedgerService _majorLedger;

    public AllocationService(ApplicationDbContext context, ICurrentUserService currentUser, IMajorLedgerService majorLedger)
    {
        _context = context;
        _currentUser = currentUser;
        _majorLedger = majorLedger;
    }

    public Task<AllocationResult> AutoAllocateAsync(Guid ledgerEntryId, CancellationToken cancellationToken = default) =>
        ApplyAutomaticallyAsync(new AutoPaymentApplicationRequest([ledgerEntryId], null, null, $"legacy-auto:{ledgerEntryId}:{Guid.NewGuid():N}"), cancellationToken);

    public Task<AllocationResult> ManualAllocateAsync(Guid ledgerEntryId, IReadOnlyList<ManualAllocationRequest> allocations, CancellationToken cancellationToken = default) =>
        ApplyManuallyAsync(new ManualPaymentApplicationRequest(allocations.Select(item => new PaymentAllocationLineRequest(ledgerEntryId, item.BillingItemId, item.Amount)).ToList(), $"legacy-manual:{ledgerEntryId}:{Guid.NewGuid():N}"), cancellationToken);

    public async Task<AllocationResult> ApplyAutomaticallyAsync(AutoPaymentApplicationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateIdempotencyKey(request.IdempotencyKey);
        var existing = await ExistingResultAsync(request.IdempotencyKey, cancellationToken);
        if (existing is not null) return existing;
        if (request.LedgerEntryIds.Count is < 1 or > 20 || request.LedgerEntryIds.Distinct().Count() != request.LedgerEntryIds.Count)
            throw new InvalidOperationException("Seleccioná entre uno y veinte pagos distintos.");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await CreateAutomaticApplicationAsync(request, PaymentApplicationOrigin.Automatic, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Otro proceso aplicó uno de los pagos o cargos. Actualizá la vista e intentá de nuevo.");
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("La solicitud ya fue procesada o los datos cambiaron durante la aplicación.");
        }
    }

    public async Task<AllocationResult> ApplyManuallyAsync(ManualPaymentApplicationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateIdempotencyKey(request.IdempotencyKey);
        var existing = await ExistingResultAsync(request.IdempotencyKey, cancellationToken);
        if (existing is not null) return existing;
        if (request.Allocations.Count is < 1 or > 100) throw new InvalidOperationException("La aplicación manual debe contener entre una y cien líneas.");
        if (request.Allocations.Any(item => item.Amount <= 0)) throw new InvalidOperationException("Todos los importes aplicados deben ser mayores a cero.");
        if (request.Allocations.Select(item => new { item.LedgerEntryId, item.BillingItemId }).Distinct().Count() != request.Allocations.Count)
            throw new InvalidOperationException("No repitas la misma combinación de pago y cargo.");

        var paymentIds = request.Allocations.Select(item => item.LedgerEntryId).Distinct().ToList();
        var chargeIds = request.Allocations.Select(item => item.BillingItemId).Distinct().ToList();
        var payments = await LoadPaymentsAsync(paymentIds, cancellationToken);
        var charges = await _context.BillingItems.Include(item => item.Adjustments).Include(item => item.PaymentPromises)
            .Where(item => chargeIds.Contains(item.Id)).ToListAsync(cancellationToken);
        if (charges.Count != chargeIds.Count) throw new KeyNotFoundException("Uno o más cargos no existen.");
        var clientId = ValidateCommonScope(payments, charges);

        var paymentApplied = await ActivePaymentAllocations(paymentIds, cancellationToken);
        var chargeAppliedInRequest = request.Allocations.GroupBy(item => item.BillingItemId).ToDictionary(group => group.Key, group => group.Sum(item => item.Amount));
        foreach (var payment in payments)
        {
            var available = payment.Amount - paymentApplied.GetValueOrDefault(payment.Id);
            var requested = request.Allocations.Where(item => item.LedgerEntryId == payment.Id).Sum(item => item.Amount);
            PaymentApplicationRules.ValidateAllocation(requested, available, requested);
        }
        foreach (var charge in charges)
        {
            if (charge.Status is not (BillingItemStatus.Pending or BillingItemStatus.Partial)) throw new InvalidOperationException("Sólo pueden recibir pagos los cargos pendientes o parciales.");
            PaymentApplicationRules.ValidateAllocation(chargeAppliedInRequest[charge.Id], chargeAppliedInRequest[charge.Id], ReceivableRules.Balance(charge));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var application = NewApplication(clientId, payments[0].Currency, request.IdempotencyKey, PaymentApplicationPriority.Selection, PaymentApplicationOrigin.Manual, null, null);
            foreach (var payment in payments)
            {
                var before = payment.Amount - paymentApplied.GetValueOrDefault(payment.Id);
                var applied = request.Allocations.Where(item => item.LedgerEntryId == payment.Id).Sum(item => item.Amount);
                application.Payments.Add(await CreatePaymentEvidenceAsync(application, payment, before, applied, cancellationToken));
                payment.PaymentAllocationVersion++;
            }
            var chargeMap = charges.ToDictionary(item => item.Id);
            foreach (var line in request.Allocations) AddAllocation(application, line.LedgerEntryId, chargeMap[line.BillingItemId], line.Amount, false);
            CompleteTotals(application);
            _context.PaymentApplications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result(application);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Otro proceso aplicó uno de los pagos o cargos. Actualizá la vista e intentá de nuevo.");
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("La solicitud ya fue procesada o los datos cambiaron durante la aplicación.");
        }
    }

    public async Task ReverseApplicationAsync(Guid applicationId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("El motivo de reversión es obligatorio.");
        var application = await LoadApplicationAsync(applicationId, cancellationToken);
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Reverse(application, reason);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("La aplicación cambió mientras se revertía. Actualizá la vista e intentá de nuevo.");
        }
    }

    public async Task<AllocationResult> ReapplyAsync(Guid applicationId, PaymentApplicationPriority? priority, Guid? preferredServiceId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        ValidateIdempotencyKey(idempotencyKey);
        var existing = await ExistingResultAsync(idempotencyKey, cancellationToken);
        if (existing is not null) return existing;
        var original = await LoadApplicationAsync(applicationId, cancellationToken);
        var paymentIds = original.Payments.Select(item => item.LedgerEntryId).ToList();
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (original.Status == PaymentApplicationStatus.Active)
            {
                Reverse(original, "Revertida para reaplicación.");
                // Persist inside the same transaction so the availability query below no longer counts
                // the reversed lines. The transaction still makes reversal and reapplication atomic.
                await _context.SaveChangesAsync(cancellationToken);
            }
            else if (original.Status != PaymentApplicationStatus.Reversed)
                throw new InvalidOperationException("La aplicación no está disponible para reaplicación.");
            var result = await CreateAutomaticApplicationAsync(new AutoPaymentApplicationRequest(paymentIds, priority, preferredServiceId, idempotencyKey, original.Id), PaymentApplicationOrigin.Reapplication, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Los pagos o cargos cambiaron durante la reaplicación. Actualizá la vista e intentá de nuevo.");
        }
    }

    public async Task UnallocateAsync(Guid allocationId, CancellationToken cancellationToken = default)
    {
        var allocation = await _context.SubscriptionAllocations.Include(item => item.PaymentApplication).Include(item => item.BillingItem).ThenInclude(item => item.Adjustments)
            .SingleOrDefaultAsync(item => item.Id == allocationId, cancellationToken) ?? throw new KeyNotFoundException("La asignación no existe.");
        if (allocation.PaymentApplicationId.HasValue)
        {
            await ReverseApplicationAsync(allocation.PaymentApplicationId.Value, "Reversión solicitada desde una asignación.", cancellationToken);
            return;
        }
        if (allocation.IsReversed) throw new InvalidOperationException("La asignación ya fue revertida.");
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        allocation.IsReversed = true; allocation.ReversedAt = DateTime.UtcNow; allocation.ReversedBy = _currentUser.UserId; allocation.ReversalReason = "Reversión de asignación histórica.";
        RestoreChargeBalance(allocation.BillingItem, allocation.Amount);
        allocation.BillingItem.PaymentAllocationVersion++;
        UpdateBillingItemStatus(allocation.BillingItem);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<AllocationResult> CreateAutomaticApplicationAsync(AutoPaymentApplicationRequest request, PaymentApplicationOrigin origin, CancellationToken cancellationToken)
    {
        var payments = await LoadPaymentsAsync(request.LedgerEntryIds.Distinct().ToList(), cancellationToken);
        var clientId = ValidateCommonScope(payments, null);
        var settings = await _context.PaymentApplicationSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        var priority = request.Priority ?? settings?.DefaultPriority ?? PaymentApplicationPriority.DueDate;
        if (priority == PaymentApplicationPriority.Selection) throw new InvalidOperationException("La prioridad Selección sólo puede usarse en aplicación manual.");
        if (request.PreferredServiceId.HasValue && !await _context.Services.AnyAsync(item => item.Id == request.PreferredServiceId, cancellationToken))
            throw new KeyNotFoundException("El plan preferido no existe.");

        var paymentIds = payments.Select(item => item.Id).ToList();
        var paymentApplied = await ActivePaymentAllocations(paymentIds, cancellationToken);
        if (payments.All(payment => payment.Amount - paymentApplied.GetValueOrDefault(payment.Id) <= 0))
            throw new InvalidOperationException("Los pagos seleccionados no tienen saldo disponible para aplicar.");
        var charges = await _context.BillingItems.Include(item => item.Adjustments).Include(item => item.PaymentPromises)
            .Include(item => item.Subscription).ThenInclude(item => item.Service)
            .Where(item => item.ClientId == clientId && item.Currency == payments[0].Currency && (item.Status == BillingItemStatus.Pending || item.Status == BillingItemStatus.Partial))
            .ToListAsync(cancellationToken);
        var application = NewApplication(clientId, payments[0].Currency, request.IdempotencyKey, priority, origin, request.PreferredServiceId, request.ReappliesPaymentApplicationId);

        foreach (var payment in payments.OrderBy(item => item.Date).ThenBy(item => item.CreatedAt))
        {
            var before = payment.Amount - paymentApplied.GetValueOrDefault(payment.Id);
            var available = before;
            foreach (var charge in PaymentApplicationRules.OrderCharges(charges, priority, payment, request.PreferredServiceId))
            {
                if (available <= 0) break;
                var balance = ReceivableRules.Balance(charge);
                if (balance <= 0) continue;
                var amount = Math.Min(available, balance);
                PaymentApplicationRules.ValidateAllocation(amount, available, balance);
                AddAllocation(application, payment.Id, charge, amount, true);
                available -= amount;
            }
            application.Payments.Add(await CreatePaymentEvidenceAsync(application, payment, before, before - available, cancellationToken));
            payment.PaymentAllocationVersion++;
        }
        CompleteTotals(application);
        _context.PaymentApplications.Add(application);
        return Result(application);
    }

    private async Task<List<LedgerEntry>> LoadPaymentsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        var payments = await _context.LedgerEntries.Where(item => ids.Contains(item.Id)).ToListAsync(cancellationToken);
        if (payments.Count != ids.Count) throw new KeyNotFoundException("Uno o más pagos no existen.");
        foreach (var payment in payments) PaymentApplicationRules.ValidatePayment(payment);
        return payments;
    }

    private static Guid ValidateCommonScope(IReadOnlyList<LedgerEntry> payments, IReadOnlyList<BillingItem>? charges)
    {
        var clientId = payments[0].ClientId!.Value;
        var currency = payments[0].Currency;
        if (payments.Any(item => item.ClientId != clientId)) throw new InvalidOperationException("Todos los pagos combinados deben pertenecer al mismo cliente.");
        if (payments.Any(item => item.Currency != currency)) throw new InvalidOperationException("Todos los pagos combinados deben tener la misma moneda.");
        if (charges is not null && charges.Any(item => item.ClientId != clientId || item.Currency != currency))
            throw new InvalidOperationException("Los pagos y cargos deben pertenecer al mismo cliente y moneda.");
        return clientId;
    }

    private async Task<Dictionary<Guid, decimal>> ActivePaymentAllocations(IReadOnlyList<Guid> paymentIds, CancellationToken cancellationToken) =>
        await _context.SubscriptionAllocations.AsNoTracking().Where(item => paymentIds.Contains(item.LedgerEntryId) && !item.IsReversed)
            .GroupBy(item => item.LedgerEntryId).Select(group => new { Id = group.Key, Amount = group.Sum(item => item.Amount) }).ToDictionaryAsync(item => item.Id, item => item.Amount, cancellationToken);

    private PaymentApplication NewApplication(Guid clientId, string currency, string idempotencyKey, PaymentApplicationPriority priority, PaymentApplicationOrigin origin, Guid? serviceId, Guid? reappliesId) => new()
    {
        Id = Guid.NewGuid(), OrganizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa."), ClientId = clientId,
        ReceiptNumber = $"REC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}", IdempotencyKey = idempotencyKey.Trim(),
        Priority = priority, Origin = origin, Status = PaymentApplicationStatus.Active, PreferredServiceId = serviceId, Currency = currency,
        AppliedAt = DateTime.UtcNow, AppliedBy = _currentUser.UserId, ReappliesPaymentApplicationId = reappliesId
    };

    private async Task<PaymentApplicationPayment> CreatePaymentEvidenceAsync(PaymentApplication application, LedgerEntry payment, decimal before, decimal applied, CancellationToken cancellationToken)
    {
        var journal = await _majorLedger.PostLegacyEntryAsync(payment, cancellationToken);
        return new PaymentApplicationPayment
        {
            Id = Guid.NewGuid(), PaymentApplicationId = application.Id, LedgerEntryId = payment.Id, LedgerEntry = payment,
            JournalEntryId = journal.Id, JournalEntry = journal, AvailableBefore = before, AppliedAmount = applied, UnappliedAfter = before - applied
        };
    }

    private void AddAllocation(PaymentApplication application, Guid paymentId, BillingItem charge, decimal amount, bool automatic)
    {
        application.Allocations.Add(new SubscriptionAllocation
        {
            Id = Guid.NewGuid(), PaymentApplicationId = application.Id, LedgerEntryId = paymentId, BillingItemId = charge.Id,
            BillingItem = charge, Amount = amount, AllocatedAt = application.AppliedAt, AllocatedBy = _currentUser.UserId, IsAutomatic = automatic
        });
        charge.PaidAmount += amount;
        charge.PaymentAllocationVersion++;
        UpdateBillingItemStatus(charge);
    }

    private static void CompleteTotals(PaymentApplication application)
    {
        application.TotalPaymentAmount = application.Payments.Sum(item => item.AvailableBefore);
        application.AppliedAmount = application.Allocations.Sum(item => item.Amount);
        application.UnappliedAmount = application.Payments.Sum(item => item.UnappliedAfter);
    }

    private static void RestoreChargeBalance(BillingItem charge, decimal amount)
    {
        if (charge.PaidAmount < amount)
            throw new InvalidOperationException("El saldo pagado del cargo no coincide con la aplicación que se intenta revertir.");
        charge.PaidAmount -= amount;
    }

    private async Task<PaymentApplication> LoadApplicationAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.PaymentApplications.Include(item => item.Payments).ThenInclude(item => item.LedgerEntry)
            .Include(item => item.Allocations).ThenInclude(item => item.BillingItem).ThenInclude(item => item.Adjustments)
            .Include(item => item.Allocations).ThenInclude(item => item.BillingItem).ThenInclude(item => item.PaymentPromises)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw new KeyNotFoundException("La aplicación de pago no existe.");

    private void Reverse(PaymentApplication application, string reason)
    {
        if (application.Status != PaymentApplicationStatus.Active) throw new InvalidOperationException("La aplicación ya fue revertida.");
        foreach (var allocation in application.Allocations.Where(item => !item.IsReversed))
        {
            allocation.IsReversed = true; allocation.ReversedAt = DateTime.UtcNow; allocation.ReversedBy = _currentUser.UserId; allocation.ReversalReason = reason.Trim();
            RestoreChargeBalance(allocation.BillingItem, allocation.Amount);
            allocation.BillingItem.PaymentAllocationVersion++;
            UpdateBillingItemStatus(allocation.BillingItem);
        }
        foreach (var payment in application.Payments) payment.LedgerEntry.PaymentAllocationVersion++;
        application.Status = PaymentApplicationStatus.Reversed; application.ReversedAt = DateTime.UtcNow; application.ReversedBy = _currentUser.UserId; application.ReversalReason = reason.Trim();
    }

    private async Task<AllocationResult?> ExistingResultAsync(string key, CancellationToken cancellationToken)
    {
        var existing = await _context.PaymentApplications.AsNoTracking().SingleOrDefaultAsync(item => item.IdempotencyKey == key.Trim(), cancellationToken);
        return existing is null ? null : Result(existing);
    }

    private static AllocationResult Result(PaymentApplication application) => new()
    {
        Success = true, PaymentApplicationId = application.Id, ReceiptNumber = application.ReceiptNumber,
        AllocatedAmount = application.AppliedAmount, RemainingAmount = application.UnappliedAmount
    };

    private static void ValidateIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Trim().Length > 100) throw new InvalidOperationException("La clave de idempotencia es obligatoria y no puede exceder 100 caracteres.");
    }

    private static void UpdateBillingItemStatus(BillingItem item)
    {
        if (item.Status == BillingItemStatus.Cancelled) return;
        if (item.PaidAmount >= ReceivableRules.EffectiveAmount(item))
        {
            item.Status = item.PaidAmount > 0 ? BillingItemStatus.Paid : BillingItemStatus.Settled;
            foreach (var promise in item.PaymentPromises.Where(promise => promise.Status == PaymentPromiseStatus.Pending))
            { promise.Status = PaymentPromiseStatus.Fulfilled; promise.ResolvedAt = DateTime.UtcNow; }
        }
        else item.Status = item.PaidAmount > 0 ? BillingItemStatus.Partial : BillingItemStatus.Pending;
    }
}
