using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetClientSubscriptions;

public class GetClientSubscriptionsQueryHandler : IRequestHandler<GetClientSubscriptionsQuery, List<SubscriptionListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetClientSubscriptionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<SubscriptionListDto>> Handle(GetClientSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser && (!_currentUserService.ClientId.HasValue || _currentUserService.ClientId.Value != request.ClientId))
        {
            throw new InvalidOperationException("No tiene permiso para consultar las suscripciones de este cliente.");
        }

        return await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Client)
            .Include(s => s.Service)
            .Where(s => s.ClientId == request.ClientId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubscriptionListDto
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
            })
            .ToListAsync(cancellationToken);
    }
}
