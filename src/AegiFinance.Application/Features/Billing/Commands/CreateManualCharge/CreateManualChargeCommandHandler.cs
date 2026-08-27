using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Commands.CreateManualCharge;

public class CreateManualChargeCommandHandler : IRequestHandler<CreateManualChargeCommand, BillingItemListDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMajorLedgerService _majorLedger;

    public CreateManualChargeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMajorLedgerService majorLedger)
        => (_context, _currentUser, _majorLedger) = (context, currentUser, majorLedger);

    public async Task<BillingItemListDto> Handle(CreateManualChargeCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser()) throw new UnauthorizedAccessException("No tiene permiso para crear cargos.");
        var key = $"manual-charge:{request.IdempotencyKey.Trim()}";
        var existing = await _context.BillingItems.AsNoTracking().Include(x => x.Client).Include(x => x.Subscription)
            .FirstOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);
        if (existing is not null) return Map(existing);

        var subscription = await _context.Subscriptions.Include(x => x.Client).Include(x => x.Service)
            .FirstOrDefaultAsync(x => x.Id == request.SubscriptionId, cancellationToken)
            ?? throw new InvalidOperationException("La suscripción no existe.");
        var currency = request.Currency.Trim().ToUpperInvariant();
        if (!string.Equals(currency, subscription.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La moneda del cargo debe coincidir con la moneda de la suscripción.");
        var chargeDate = request.ChargeDate.Date;
        var cycle = await _context.BillingCycles.FirstOrDefaultAsync(x => x.Year == chargeDate.Year && x.Month == chargeDate.Month, cancellationToken);
        if (cycle?.Status == BillingCycleStatus.Closed) throw new InvalidOperationException("No se pueden agregar cargos a un ciclo cerrado.");
        if (cycle is null)
        {
            cycle = new BillingCycle { Id = Guid.NewGuid(), Year = chargeDate.Year, Month = chargeDate.Month,
                StartDate = new DateTime(chargeDate.Year, chargeDate.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(chargeDate.Year, chargeDate.Month, DateTime.DaysInMonth(chargeDate.Year, chargeDate.Month), 23, 59, 59, DateTimeKind.Utc), Status = BillingCycleStatus.Open };
            _context.BillingCycles.Add(cycle);
        }

        var item = new BillingItem { Id = Guid.NewGuid(), Type = BillingItemType.ManualCharge, IdempotencyKey = key,
            SubscriptionId = subscription.Id, Subscription = subscription, BillingCycleId = cycle.Id, BillingCycle = cycle,
            ClientId = subscription.ClientId, Client = subscription.Client, Description = request.Description.Trim(),
            Amount = request.Amount, BaseAmount = request.Amount, Currency = currency,
            DueDate = request.DueDate.Date, PeriodStart = request.PeriodStart.Date, PeriodEnd = request.PeriodEnd.Date,
            Status = BillingItemStatus.Pending, GeneratedAt = chargeDate, GeneratedBy = _currentUser.UserId };
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        _context.BillingItems.Add(item);
        await _majorLedger.PostChargeAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Map(item);
    }

    private static BillingItemListDto Map(BillingItem item) => new() { Id = item.Id, SubscriptionCode = item.Subscription.Code,
        ClientName = item.Client.Name, Description = item.Description, Amount = item.Amount, Currency = item.Currency,
        BaseAmount = item.BaseAmount, DiscountAmount = item.DiscountAmount, TaxAmount = item.TaxAmount, ProrationFactor = item.ProrationFactor,
        DueDate = item.DueDate, Status = item.Status.ToString(), PaidAmount = item.PaidAmount, Type = item.Type.ToString(),
        PeriodStart = item.PeriodStart, PeriodEnd = item.PeriodEnd };
}
