using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Helpers;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.CreateSubscription;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISubscriptionCodeGenerator _codeGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateSubscriptionCommandHandler(IApplicationDbContext context, ISubscriptionCodeGenerator codeGenerator, ICurrentUserService currentUserService)
    {
        _context = context;
        _codeGenerator = codeGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No se permite crear suscripciones desde el portal.");
        }

        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("El cliente seleccionado no existe.");
        }

        if (client.Status != ClientStatus.Active)
        {
            throw new InvalidOperationException("No se pueden crear suscripciones para un cliente que no está activo.");
        }

        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio seleccionado no existe.");
        }

        var code = await _codeGenerator.GenerateAsync(cancellationToken);
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpper();

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            Code = code,
            ClientId = request.ClientId,
            Client = client,
            ServiceId = request.ServiceId,
            Service = service,
            BillingType = request.BillingType,
            Price = request.Price,
            Currency = currency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BillingDay = request.BillingDay,
            Status = SubscriptionStatus.Active,
            AutoRenew = request.AutoRenew,
            Notes = request.Notes
        };

        if (subscription.BillingType == BillingType.Monthly || subscription.BillingType == BillingType.Yearly)
        {
            subscription.NextBillingDate = SubscriptionDateCalculator.CalculateNextBillingDate(
                subscription.StartDate,
                subscription.BillingType,
                subscription.BillingDay);
        }

        if (subscription.Price > 0)
        {
            subscription.PriceHistory.Add(new SubscriptionPriceHistory
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                Subscription = subscription,
                OldPrice = 0,
                NewPrice = subscription.Price,
                EffectiveDate = subscription.StartDate,
                Reason = "Precio inicial"
            });
        }

        _context.Subscriptions.Add(subscription);
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
