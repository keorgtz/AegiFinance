using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Queries.GetServiceById;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetServiceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceDetailDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.PriceHistory)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }

        return new ServiceDetailDto
        {
            Id = service.Id,
            Code = service.Code,
            Name = service.Name,
            Description = service.Description,
            CategoryId = service.CategoryId,
            CategoryName = service.Category?.Name,
            BillingType = service.BillingType.ToString(),
            DefaultPrice = service.DefaultPrice,
            Currency = service.Currency,
            IsActive = service.IsActive,
            IsPublic = service.IsPublic,
            PriceHistory = service.PriceHistory
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
                }).ToList()
        };
    }
}
