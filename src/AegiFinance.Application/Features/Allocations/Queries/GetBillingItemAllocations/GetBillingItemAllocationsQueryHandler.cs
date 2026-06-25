using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Allocations.Queries.GetBillingItemAllocations;

public class GetBillingItemAllocationsQueryHandler : IRequestHandler<GetBillingItemAllocationsQuery, List<SubscriptionAllocationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBillingItemAllocationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<SubscriptionAllocationDto>> Handle(GetBillingItemAllocationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar asignaciones de pagos.");
        }

        var billingItemExists = await _context.BillingItems.AsNoTracking().AnyAsync(x => x.Id == request.BillingItemId, cancellationToken);
        if (!billingItemExists)
        {
            throw new InvalidOperationException("El cargo no existe.");
        }

        return await _context.SubscriptionAllocations
            .AsNoTracking()
            .Include(x => x.BillingItem)
            .Include(x => x.LedgerEntry)
            .Where(x => x.BillingItemId == request.BillingItemId)
            .OrderByDescending(x => x.AllocatedAt)
            .Select(x => new SubscriptionAllocationDto
            {
                Id = x.Id,
                LedgerEntryId = x.LedgerEntryId,
                BillingItemId = x.BillingItemId,
                Amount = x.Amount,
                AllocatedAt = x.AllocatedAt,
                AllocatedBy = x.AllocatedBy,
                IsAutomatic = x.IsAutomatic,
                BillingItemDescription = x.BillingItem.Description,
                LedgerEntryDescription = x.LedgerEntry.Description
            })
            .ToListAsync(cancellationToken);
    }
}
