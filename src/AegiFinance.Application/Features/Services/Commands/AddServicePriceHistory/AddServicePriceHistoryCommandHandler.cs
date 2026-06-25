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
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }

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

        var mostRecent = await _context.ServicePriceHistories
            .Where(h => h.ServiceId == service.Id)
            .OrderByDescending(h => h.EffectiveDate)
            .ThenByDescending(h => h.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (mostRecent is null || request.EffectiveDate >= mostRecent.EffectiveDate)
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
