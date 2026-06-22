using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
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
        var clientId = request.ClientId;
        if (_currentUserService.UserType == UserType.Client.ToString())
        {
            if (_currentUserService.ClientId != clientId)
                throw new InvalidOperationException("No puedes consultar asignaciones de otro cliente.");
        }

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
                BillingItemDescription = x.BillingItem.Description,
                LedgerEntryDescription = x.LedgerEntry.Description
            })
            .ToListAsync(cancellationToken);
    }
}
