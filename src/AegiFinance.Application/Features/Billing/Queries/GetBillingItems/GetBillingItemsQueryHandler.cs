using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
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

        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser)
        {
            if (!_currentUserService.ClientId.HasValue)
            {
                return new PaginatedList<BillingItemListDto>(new List<BillingItemListDto>(), 0, request.PageNumber, request.PageSize);
            }

            query = query.Where(bi => bi.ClientId == _currentUserService.ClientId.Value);
        }
        else if (request.ClientId.HasValue)
        {
            query = query.Where(bi => bi.ClientId == request.ClientId.Value);
        }

        if (request.BillingCycleId.HasValue)
        {
            query = query.Where(bi => bi.BillingCycleId == request.BillingCycleId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(bi => bi.Status == request.Status.Value);
        }

        if (request.DueDateFrom.HasValue)
        {
            query = query.Where(bi => bi.DueDate >= request.DueDateFrom.Value);
        }

        if (request.DueDateTo.HasValue)
        {
            query = query.Where(bi => bi.DueDate <= request.DueDateTo.Value);
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
            PaidAmount = bi.PaidAmount
        });

        return await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
