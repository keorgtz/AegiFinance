using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Commands.CloseBillingCycle;

public class CloseBillingCycleCommandHandler : IRequestHandler<CloseBillingCycleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CloseBillingCycleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CloseBillingCycleCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para cerrar ciclos de facturación.");
        }

        var cycle = await _context.BillingCycles
            .FirstOrDefaultAsync(c => c.Id == request.BillingCycleId, cancellationToken);

        if (cycle is null)
        {
            throw new InvalidOperationException("El ciclo de facturación no existe.");
        }

        if (cycle.Status == BillingCycleStatus.Closed)
        {
            return;
        }

        cycle.Status = BillingCycleStatus.Closed;
        cycle.ClosedAt = DateTime.UtcNow;
        cycle.ClosedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
