using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Commands.CancelBillingItem;

public class CancelBillingItemCommandHandler : IRequestHandler<CancelBillingItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelBillingItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CancelBillingItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.BillingItems
            .FirstOrDefaultAsync(bi => bi.Id == request.BillingItemId, cancellationToken);

        if (item is null)
        {
            throw new InvalidOperationException("El cargo no existe.");
        }

        if (IsClientUser() && item.ClientId != _currentUserService.ClientId)
        {
            throw new UnauthorizedAccessException("No tiene permiso para cancelar este cargo.");
        }

        if (item.Status == BillingItemStatus.Paid)
        {
            throw new InvalidOperationException("No se puede cancelar un cargo pagado.");
        }

        if (item.Status == BillingItemStatus.Cancelled)
        {
            return;
        }

        item.Status = BillingItemStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private bool IsClientUser()
    {
        return Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;
    }
}
