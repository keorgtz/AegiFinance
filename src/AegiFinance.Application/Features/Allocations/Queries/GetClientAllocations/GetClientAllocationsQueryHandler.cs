using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Allocations.Queries.GetClientAllocations;

public class GetClientAllocationsQueryHandler : IRequestHandler<GetClientAllocationsQuery, List<SubscriptionAllocationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetClientAllocationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<SubscriptionAllocationDto>> Handle(GetClientAllocationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar asignaciones de pagos.");
        }

        var clientId = request.ClientId;

        return await _context.SubscriptionAllocations
            .AsNoTracking()
            .Include(x => x.BillingItem)
            .Include(x => x.LedgerEntry)
            .Where(x => x.BillingItem.ClientId == clientId)
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
                IsReversed = x.IsReversed,
                PaymentApplicationId = x.PaymentApplicationId,
                ReversedAt = x.ReversedAt,
                ReversalReason = x.ReversalReason,
                BillingItemDescription = x.BillingItem.Description,
                LedgerEntryDescription = x.LedgerEntry.Description
            })
            .ToListAsync(cancellationToken);
    }
}
