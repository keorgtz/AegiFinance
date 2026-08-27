using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Commands.CreatePaymentPromise;

public class CreatePaymentPromiseCommandHandler : IRequestHandler<CreatePaymentPromiseCommand, PaymentPromiseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreatePaymentPromiseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) => (_context, _currentUser) = (context, currentUser);

    public async Task<PaymentPromiseDto> Handle(CreatePaymentPromiseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser()) throw new UnauthorizedAccessException("No tiene permiso para registrar promesas de pago.");
        var item = await _context.BillingItems.Include(x => x.Adjustments).Include(x => x.PaymentPromises).FirstOrDefaultAsync(x => x.Id == request.BillingItemId, cancellationToken)
            ?? throw new InvalidOperationException("El cargo no existe.");
        if (item.Status is BillingItemStatus.Cancelled or BillingItemStatus.Paid or BillingItemStatus.Settled) throw new InvalidOperationException("El cargo no tiene saldo pendiente.");
        if (item.PaymentPromises.Any(x => x.Status == PaymentPromiseStatus.Pending)) throw new InvalidOperationException("El cargo ya tiene una promesa de pago pendiente.");
        var balance = ReceivableRules.Balance(item);
        if (request.PromisedAmount > balance) throw new InvalidOperationException("La promesa supera el saldo pendiente.");
        var promise = new PaymentPromise { Id = Guid.NewGuid(), BillingItemId = item.Id, BillingItem = item,
            PromisedAmount = request.PromisedAmount, PromiseDate = request.PromiseDate.Date,
            Status = PaymentPromiseStatus.Pending, Notes = request.Notes?.Trim() };
        _context.PaymentPromises.Add(promise);
        await _context.SaveChangesAsync(cancellationToken);
        return new PaymentPromiseDto(promise.Id, promise.PromisedAmount, promise.PromiseDate, promise.Status.ToString(), promise.Notes, promise.ResolvedAt);
    }
}
