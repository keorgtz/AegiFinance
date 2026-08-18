using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Dashboard;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Queries.GetBillingItems;

public class GetBillingItemsQueryHandler : IRequestHandler<GetBillingItemsQuery, PaginatedList<BillingItemListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBillingItemsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<BillingItemListDto>> Handle(GetBillingItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BillingItems
            .AsNoTracking()
            .Include(bi => bi.Client)
            .Include(bi => bi.Subscription)
            .AsQueryable();

        Guid? clientId = request.ClientId;
        if (_currentUserService.IsClientUser())
        {
            if (!_currentUserService.ClientId.HasValue)
            {
                return new PaginatedList<BillingItemListDto>(new List<BillingItemListDto>(), 0, request.PageNumber, request.PageSize);
            }

            clientId = _currentUserService.ClientId.Value;
        }

        query = query.ApplyOperationalScope(clientId, request.DueDateFrom, request.DueDateTo, request.Currency?.ToUpperInvariant());

        if (request.BillingCycleId.HasValue)
        {
            query = query.Where(bi => bi.BillingCycleId == request.BillingCycleId.Value);
        }

        if (request.OutstandingOnly)
        {
            query = query.Outstanding();
        }
        else if (request.Status.HasValue)
        {
            query = query.Where(bi => bi.Status == request.Status.Value);
        }

        query = query.OrderByDescending(bi => bi.DueDate);

        var projected = query.Select(bi => new BillingItemListDto
        {
            Id = bi.Id,
            SubscriptionCode = bi.Subscription.Code,
            ClientName = bi.Client.Name,
            Description = bi.Description,
            Amount = bi.Amount,
            Currency = bi.Currency,
            DueDate = bi.DueDate,
            Status = bi.Status.ToString(),
            PaidAmount = bi.PaidAmount,
            CancellationReason = bi.CancellationReason
        });

        return await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
