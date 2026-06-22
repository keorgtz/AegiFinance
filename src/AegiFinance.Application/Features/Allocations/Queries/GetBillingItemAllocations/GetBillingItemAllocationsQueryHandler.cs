using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
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
        var billingItem = await _context.BillingItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.BillingItemId, cancellationToken)
            ?? throw new InvalidOperationException("El cargo no existe.");

        if (_currentUserService.UserType == UserType.Client.ToString() && _currentUserService.ClientId != billingItem.ClientId)
            throw new InvalidOperationException("No puedes consultar asignaciones de otro cliente.");

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
