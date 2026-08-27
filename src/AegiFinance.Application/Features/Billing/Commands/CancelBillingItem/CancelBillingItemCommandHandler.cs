using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Commands.CancelBillingItem;

public class CancelBillingItemCommandHandler : IRequestHandler<CancelBillingItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMajorLedgerService _majorLedger;

    public CancelBillingItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMajorLedgerService majorLedger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _majorLedger = majorLedger;
    }

    public async Task Handle(CancelBillingItemCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para cancelar cargos.");
        }

        var item = await _context.BillingItems
            .Include(bi => bi.Adjustments)
            .FirstOrDefaultAsync(bi => bi.Id == request.BillingItemId, cancellationToken);

        if (item is null)
        {
            throw new InvalidOperationException("El cargo no existe.");
        }

        if (item.PaidAmount > 0)
        {
            throw new InvalidOperationException("Desasigna los pagos aplicados antes de cancelar el cargo.");
        }

        if (item.Status == BillingItemStatus.Cancelled)
        {
            return;
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var journalEntry = await _context.JournalEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.IdempotencyKey == $"charge:{item.Id}", cancellationToken);
        if (journalEntry is not null && journalEntry.Status != JournalEntryStatus.Reversed)
        {
            await _majorLedger.ReverseAsync(journalEntry.Id, DateTime.UtcNow, request.Reason, cancellationToken);
        }
        foreach (var adjustment in item.Adjustments.Where(value => !value.ReversedAt.HasValue))
        {
            var adjustmentEntry = await _context.JournalEntries.AsNoTracking()
                .FirstOrDefaultAsync(entry => entry.IdempotencyKey == $"billing-adjustment:{adjustment.Id}", cancellationToken);
            if (adjustmentEntry is not null && adjustmentEntry.Status != JournalEntryStatus.Reversed)
                await _majorLedger.ReverseAsync(adjustmentEntry.Id, DateTime.UtcNow, request.Reason, cancellationToken);
            adjustment.ReversedAt = DateTime.UtcNow;
            adjustment.ReversedBy = _currentUserService.UserId;
            adjustment.ReversalReason = request.Reason;
        }

        item.Status = BillingItemStatus.Cancelled;
        item.CancellationReason = request.Reason;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
