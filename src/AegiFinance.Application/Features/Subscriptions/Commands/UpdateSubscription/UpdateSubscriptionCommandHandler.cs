using AegiFinance.Application.Common.Extensions;
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
    private readonly ICurrentUserService _currentUserService;

    public UpdateSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SubscriptionDto> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No se permite editar suscripciones desde el portal.");
        }

        var subscription = await _context.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Service)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpper();
        if (request.ClientId != subscription.ClientId || request.ServiceId != subscription.ServiceId ||
            request.BillingType != subscription.BillingType || request.Price != subscription.Price ||
            currency != subscription.Currency || request.StartDate != subscription.StartDate || request.BillingDay != subscription.BillingDay)
            throw new InvalidOperationException("Cliente, plan, precio, periodicidad y fecha inicial se cambian mediante condiciones versionadas, no editando la suscripción.");

        subscription.EndDate = request.EndDate;
        subscription.AutoRenew = request.AutoRenew;
        subscription.Notes = request.Notes;

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
            ServiceVersionId = subscription.ServiceVersionId,
            CustomIntervalDays = subscription.CustomIntervalDays,
            DiscountPercent = subscription.DiscountPercent,
            TaxPercent = subscription.TaxPercent,
            ProrationPolicy = subscription.ProrationPolicy.ToString(),
            ContractTerms = subscription.ContractTerms,
            Status = subscription.Status.ToString(),
            AutoRenew = subscription.AutoRenew,
            Notes = subscription.Notes,
            LastBillingDate = subscription.LastBillingDate,
            NextBillingDate = subscription.NextBillingDate
        };
    }
}
