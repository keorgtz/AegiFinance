using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Helpers;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Accounting;
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
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio seleccionado no existe.");
        }

        var code = await _codeGenerator.GenerateAsync(cancellationToken);
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpper();
        var startDate = request.StartDate.Date;
        var endDate = request.EndDate?.Date;
        if (!request.ServiceVersionId.HasValue)
        {
            EnsureApplicableVersion(service, startDate);
        }
        var serviceVersion = request.ServiceVersionId.HasValue
            ? service.Versions.FirstOrDefault(v => v.Id == request.ServiceVersionId && v.IsPublished && v.EffectiveFrom.Date <= startDate)
            : ServiceVersionRules.ResolveApplicable(service.Versions, startDate);
        if (serviceVersion is null)
            throw new InvalidOperationException("El plan no tiene una versión publicada aplicable.");
        var pricing = SubscriptionPricingRules.Calculate(request.Price, request.DiscountPercent, request.TaxPercent);

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            Code = code,
            ClientId = request.ClientId,
            Client = client,
            ServiceId = request.ServiceId,
            Service = service,
            ServiceVersionId = serviceVersion.Id,
            ServiceVersion = serviceVersion,
            BillingType = request.BillingType,
            Price = pricing.Total,
            Currency = currency,
            StartDate = startDate,
            EndDate = endDate,
            BillingDay = request.BillingDay,
            CustomIntervalDays = request.CustomIntervalDays,
            DiscountPercent = request.DiscountPercent,
            TaxPercent = request.TaxPercent,
            ProrationPolicy = request.ProrationPolicy,
            ContractTerms = request.ContractTerms,
            Status = SubscriptionStatus.Active,
            AutoRenew = request.AutoRenew,
            Notes = request.Notes
        };

        subscription.NextBillingDate = subscription.BillingType == BillingType.OneTime
            ? subscription.StartDate
            : SubscriptionDateCalculator.CalculateNextBillingDate(subscription.StartDate, subscription.BillingType, subscription.BillingDay, subscription.CustomIntervalDays);

        subscription.TermsVersions.Add(new SubscriptionTermsVersion
        {
            Id = Guid.NewGuid(), SubscriptionId = subscription.Id, Subscription = subscription,
            VersionNumber = 1, ServiceVersionId = serviceVersion.Id, ServiceVersion = serviceVersion,
            EffectiveFrom = subscription.StartDate, BillingType = subscription.BillingType,
            BasePrice = request.Price, Currency = currency, DiscountPercent = request.DiscountPercent,
            TaxPercent = request.TaxPercent, BillingDay = request.BillingDay,
            CustomIntervalDays = request.CustomIntervalDays, ProrationPolicy = request.ProrationPolicy,
            Terms = request.ContractTerms, Reason = "Condiciones iniciales"
        });

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

    private static void EnsureApplicableVersion(Service service, DateTime subscriptionStartDate)
    {
        if (ServiceVersionRules.ResolveApplicable(service.Versions, subscriptionStartDate) is not null)
        {
            return;
        }

        var nextPublishedDate = service.Versions
            .Where(version => version.IsPublished && version.EffectiveFrom.Date > subscriptionStartDate)
            .Select(version => (DateTime?)version.EffectiveFrom.Date)
            .OrderBy(date => date)
            .FirstOrDefault();

        var version = new ServiceVersion
        {
            Id = Guid.NewGuid(),
            ServiceId = service.Id,
            Service = service,
            VersionNumber = service.Versions.Select(item => item.VersionNumber).DefaultIfEmpty(0).Max() + 1,
            Name = service.Name,
            Description = service.Description,
            BillingType = service.BillingType,
            BasePrice = service.DefaultPrice,
            Currency = service.Currency,
            EffectiveFrom = subscriptionStartDate,
            EffectiveTo = nextPublishedDate?.AddTicks(-1),
            IsPublished = true
        };
        version.Concepts.Add(new ServiceVersionConcept
        {
            Id = Guid.NewGuid(),
            ServiceVersionId = version.Id,
            ServiceVersion = version,
            Code = "BASE",
            Name = service.Name,
            Quantity = 1,
            UnitPrice = service.DefaultPrice
        });
        service.Versions.Add(version);
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
