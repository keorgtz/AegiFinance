using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.AddServicePriceHistory;

public class AddServicePriceHistoryCommandHandler : IRequestHandler<AddServicePriceHistoryCommand, ServicePriceHistoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddServicePriceHistoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ServicePriceHistoryDto> Handle(AddServicePriceHistoryCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para registrar cambios de precio.");
        }

        var service = await _context.Services
            .Include(s => s.Versions).ThenInclude(v => v.Concepts)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }
        if (service.Versions.Any(v => v.IsPublished && v.EffectiveFrom == request.EffectiveDate))
            throw new InvalidOperationException("Ya existe una versión publicada en esa fecha efectiva.");

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpper();

        var historyEntry = new ServicePriceHistory
        {
            Id = Guid.NewGuid(),
            ServiceId = service.Id,
            Service = service,
            Price = request.Price,
            Currency = currency,
            EffectiveDate = request.EffectiveDate,
            Reason = request.Reason
        };

        _context.ServicePriceHistories.Add(historyEntry);

        var source = service.Versions.Where(v => v.IsPublished && v.EffectiveFrom <= request.EffectiveDate)
            .OrderByDescending(v => v.EffectiveFrom).FirstOrDefault()
            ?? throw new InvalidOperationException("El plan no tiene una versión publicada para aplicar el cambio de precio.");
        var version = new ServiceVersion
        {
            Id = Guid.NewGuid(), ServiceId = service.Id, Service = service,
            VersionNumber = service.Versions.Select(v => v.VersionNumber).DefaultIfEmpty().Max() + 1,
            Name = source.Name, Description = source.Description, BillingType = source.BillingType,
            BasePrice = request.Price, Currency = currency, DefaultDiscountPercent = source.DefaultDiscountPercent,
            DefaultTaxPercent = source.DefaultTaxPercent, CustomIntervalDays = source.CustomIntervalDays,
            ProrationPolicy = source.ProrationPolicy, EffectiveFrom = request.EffectiveDate,
            Terms = source.Terms, IsPublished = true
        };
        foreach (var concept in source.Concepts)
            version.Concepts.Add(new ServiceVersionConcept { Id = Guid.NewGuid(), ServiceVersionId = version.Id, ServiceVersion = version, Code = concept.Code, Name = concept.Name, Description = concept.Description, Quantity = concept.Quantity, UnitPrice = source.Concepts.Count == 1 ? request.Price : concept.UnitPrice, TaxPercent = concept.TaxPercent, SortOrder = concept.SortOrder });
        service.Versions.Add(version);
        var published = service.Versions.Where(v => v.IsPublished).OrderBy(v => v.EffectiveFrom).ThenBy(v => v.VersionNumber).ToList();
        for (var index = 0; index < published.Count; index++) published[index].EffectiveTo = index + 1 < published.Count ? published[index + 1].EffectiveFrom : null;

        var mostRecent = await _context.ServicePriceHistories
            .Where(h => h.ServiceId == service.Id)
            .OrderByDescending(h => h.EffectiveDate)
            .ThenByDescending(h => h.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if ((mostRecent is null || request.EffectiveDate >= mostRecent.EffectiveDate) && request.EffectiveDate <= DateTime.UtcNow)
        {
            service.DefaultPrice = request.Price;
            service.Currency = currency;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ServicePriceHistoryDto
        {
            Id = historyEntry.Id,
            ServiceId = historyEntry.ServiceId,
            Price = historyEntry.Price,
            Currency = historyEntry.Currency,
            EffectiveDate = historyEntry.EffectiveDate,
            Reason = historyEntry.Reason,
            CreatedAt = historyEntry.CreatedAt
        };
    }
}
