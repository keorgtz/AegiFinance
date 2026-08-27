using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Commands.AddBillingAdjustment;

public class AddBillingAdjustmentCommandHandler : IRequestHandler<AddBillingAdjustmentCommand, BillingAdjustmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMajorLedgerService _majorLedger;

    public AddBillingAdjustmentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMajorLedgerService majorLedger)
        => (_context, _currentUser, _majorLedger) = (context, currentUser, majorLedger);

    public async Task<BillingAdjustmentDto> Handle(AddBillingAdjustmentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser()) throw new UnauthorizedAccessException("No tiene permiso para ajustar cargos.");
        var key = $"billing-adjustment:{request.IdempotencyKey.Trim()}";
        var existing = await _context.BillingAdjustments.AsNoTracking().FirstOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);
        if (existing is not null) return Map(existing);
        var item = await _context.BillingItems.Include(x => x.Adjustments).FirstOrDefaultAsync(x => x.Id == request.BillingItemId, cancellationToken)
            ?? throw new InvalidOperationException("El cargo no existe.");
        if (item.Status == BillingItemStatus.Cancelled) throw new InvalidOperationException("No se puede ajustar un cargo cancelado.");
        var currentBalance = ReceivableRules.Balance(item);
        if (request.Type == BillingAdjustmentType.CreditNote) ReceivableRules.ValidateCredit(item, request.Amount);

        var adjustment = new BillingAdjustment { Id = Guid.NewGuid(), BillingItemId = item.Id, BillingItem = item,
            Type = request.Type, Amount = request.Amount, EffectiveDate = request.EffectiveDate.Date,
            Reason = request.Reason.Trim(), IdempotencyKey = key };
        if (request.Type == BillingAdjustmentType.CreditNote && request.Amount == currentBalance)
            item.Status = BillingItemStatus.Settled;
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        _context.BillingAdjustments.Add(adjustment);
        await _majorLedger.PostBillingAdjustmentAsync(adjustment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Map(adjustment);
    }

    private static BillingAdjustmentDto Map(BillingAdjustment item) => new(item.Id, item.Type.ToString(), item.Amount, item.Reason, item.EffectiveDate, item.ReversedAt);
}
