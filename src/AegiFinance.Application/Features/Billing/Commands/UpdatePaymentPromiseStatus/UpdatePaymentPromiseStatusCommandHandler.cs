using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Commands.UpdatePaymentPromiseStatus;

public class UpdatePaymentPromiseStatusCommandHandler : IRequestHandler<UpdatePaymentPromiseStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public UpdatePaymentPromiseStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) => (_context, _currentUser) = (context, currentUser);
    public async Task Handle(UpdatePaymentPromiseStatusCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser()) throw new UnauthorizedAccessException("No tiene permiso para resolver promesas de pago.");
        var promise = await _context.PaymentPromises.FirstOrDefaultAsync(x => x.Id == request.PaymentPromiseId, cancellationToken)
            ?? throw new InvalidOperationException("La promesa de pago no existe.");
        if (promise.Status != PaymentPromiseStatus.Pending) throw new InvalidOperationException("La promesa ya fue resuelta.");
        promise.Status = request.Status; promise.ResolvedAt = DateTime.UtcNow; promise.ResolvedBy = _currentUser.UserId;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
