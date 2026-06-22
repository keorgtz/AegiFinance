using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ChangeSubscriptionPrice;

public class ChangeSubscriptionPriceCommandHandler : IRequestHandler<ChangeSubscriptionPriceCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ChangeSubscriptionPriceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SubscriptionDto> Handle(ChangeSubscriptionPriceCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Service)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        EnsureClientAccess(subscription.ClientId);

        var oldPrice = subscription.Price;
        subscription.Price = request.NewPrice;

        subscription.PriceHistory.Add(new SubscriptionPriceHistory
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            Subscription = subscription,
            OldPrice = oldPrice,
            NewPrice = request.NewPrice,
            EffectiveDate = request.EffectiveDate,
            Reason = request.Reason
        });

        subscription.ChangeLogs.Add(new SubscriptionChangeLog
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            ChangeType = SubscriptionChangeType.PriceChange,
            OldValue = oldPrice.ToString("F2"),
            NewValue = request.NewPrice.ToString("F2"),
            Reason = request.Reason
        });

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(subscription);
    }

    private static SubscriptionDto MapToDto(Subscription subscription)
    {
        return new SubscriptionDto
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
            NextBillingDate = subscription.NextBillingDate
        };
    }

    private void EnsureClientAccess(Guid clientId)
    {
        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser && (!_currentUserService.ClientId.HasValue || _currentUserService.ClientId.Value != clientId))
        {
            throw new InvalidOperationException("No tiene permiso para modificar esta suscripción.");
        }
    }
}
