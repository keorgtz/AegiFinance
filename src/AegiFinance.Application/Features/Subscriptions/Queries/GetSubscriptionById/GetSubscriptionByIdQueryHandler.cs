using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionById;

public class GetSubscriptionByIdQueryHandler : IRequestHandler<GetSubscriptionByIdQuery, SubscriptionDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSubscriptionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SubscriptionDetailDto> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Client)
            .Include(s => s.Service)
            .Include(s => s.PriceHistory)
            .Include(s => s.ChangeLogs)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser && (!_currentUserService.ClientId.HasValue || _currentUserService.ClientId.Value != subscription.ClientId))
        {
            throw new InvalidOperationException("No tiene permiso para consultar esta suscripción.");
        }

        return new SubscriptionDetailDto
        {
            Id = subscription.Id,
            Code = subscription.Code,
            ClientId = subscription.ClientId,
            ClientName = subscription.Client.Name,
            ServiceId = subscription.ServiceId,
            ServiceName = subscription.Service.Name,
            BillingType = subscription.BillingType.ToString(),
            Price = subscription.Price,
            Currency = subscription.Currency,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            BillingDay = subscription.BillingDay,
            Status = subscription.Status.ToString(),
            AutoRenew = subscription.AutoRenew,
            Notes = subscription.Notes,
            LastBillingDate = subscription.LastBillingDate,
            NextBillingDate = subscription.NextBillingDate,
            PriceHistory = subscription.PriceHistory
                .OrderByDescending(h => h.EffectiveDate)
                .ThenByDescending(h => h.CreatedAt)
                .Select(h => new SubscriptionPriceHistoryDto
                {
                    Id = h.Id,
                    SubscriptionId = h.SubscriptionId,
                    OldPrice = h.OldPrice,
                    NewPrice = h.NewPrice,
                    EffectiveDate = h.EffectiveDate,
                    Reason = h.Reason,
                    CreatedAt = h.CreatedAt
                }).ToList(),
            ChangeLogs = subscription.ChangeLogs
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new SubscriptionChangeLogDto
                {
                    Id = c.Id,
                    SubscriptionId = c.SubscriptionId,
                    ChangeType = c.ChangeType.ToString(),
                    OldValue = c.OldValue,
                    NewValue = c.NewValue,
                    Reason = c.Reason,
                    CreatedAt = c.CreatedAt
                }).ToList()
        };
    }
}
