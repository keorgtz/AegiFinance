using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Queries.GetServicePriceHistory;

public class GetServicePriceHistoryQueryHandler : IRequestHandler<GetServicePriceHistoryQuery, List<ServicePriceHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServicePriceHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServicePriceHistoryDto>> Handle(GetServicePriceHistoryQuery request, CancellationToken cancellationToken)
    {
        var serviceExists = await _context.Services
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (!serviceExists)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }

        return await _context.ServicePriceHistories
            .AsNoTracking()
            .Where(h => h.ServiceId == request.ServiceId)
            .OrderByDescending(h => h.EffectiveDate)
            .ThenByDescending(h => h.CreatedAt)
            .Select(h => new ServicePriceHistoryDto
            {
                Id = h.Id,
                ServiceId = h.ServiceId,
                Price = h.Price,
                Currency = h.Currency,
                EffectiveDate = h.EffectiveDate,
                Reason = h.Reason,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
