using AegiFinance.Application.Common.Helpers;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.UpdateSubscription;

public class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Service)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        var clientExists = await _context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ClientId, cancellationToken);

        if (!clientExists)
        {
            throw new InvalidOperationException("El cliente seleccionado no existe.");
        }

        var serviceExists = await _context.Services
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (!serviceExists)
        {
            throw new InvalidOperationException("El servicio seleccionado no existe.");
        }

        subscription.ClientId = request.ClientId;
        subscription.ServiceId = request.ServiceId;
        subscription.BillingType = request.BillingType;
        subscription.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpper();
        subscription.StartDate = request.StartDate;
        subscription.EndDate = request.EndDate;
        subscription.BillingDay = request.BillingDay;
        subscription.AutoRenew = request.AutoRenew;
        subscription.Notes = request.Notes;

        if (subscription.BillingType == BillingType.Monthly || subscription.BillingType == BillingType.Yearly)
        {
            subscription.NextBillingDate = SubscriptionDateCalculator.CalculateNextBillingDate(
                subscription.LastBillingDate ?? subscription.StartDate,
                subscription.BillingType,
                subscription.BillingDay);
        }
        else
        {
            subscription.NextBillingDate = null;
        }

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
}
