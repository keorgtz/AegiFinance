using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptions;

public class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, PaginatedList<SubscriptionListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSubscriptionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<SubscriptionListDto>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Client)
            .Include(s => s.Service)
            .AsQueryable();

        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser)
        {
            if (!_currentUserService.ClientId.HasValue)
            {
                return new PaginatedList<SubscriptionListDto>(new List<SubscriptionListDto>(), 0, request.PageNumber, request.PageSize);
            }

            query = query.Where(s => s.ClientId == _currentUserService.ClientId.Value);
        }
        else if (request.ClientId.HasValue)
        {
            query = query.Where(s => s.ClientId == request.ClientId.Value);
        }

        if (request.ServiceId.HasValue)
        {
            query = query.Where(s => s.ServiceId == request.ServiceId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }

        if (request.StartDateFrom.HasValue)
        {
            query = query.Where(s => s.StartDate >= request.StartDateFrom.Value);
        }

        if (request.StartDateTo.HasValue)
        {
            query = query.Where(s => s.StartDate <= request.StartDateTo.Value);
        }

        query = query.OrderByDescending(s => s.CreatedAt);

        var projected = query.Select(s => new SubscriptionListDto
        {
            Id = s.Id,
            Code = s.Code,
            ClientName = s.Client.Name,
            ServiceName = s.Service.Name,
            BillingType = s.BillingType.ToString(),
            Price = s.Price,
            Currency = s.Currency,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status.ToString(),
            AutoRenew = s.AutoRenew,
            NextBillingDate = s.NextBillingDate
        });

        return await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
