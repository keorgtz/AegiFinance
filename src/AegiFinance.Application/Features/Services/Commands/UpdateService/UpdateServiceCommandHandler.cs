using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.UpdateService;

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceDto> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .Include(s => s.Category)
            .Include(s => s.PriceHistory)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.ServiceCategories
                .AsNoTracking()
                .AnyAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

            if (!categoryExists)
            {
                throw new InvalidOperationException("La categoría seleccionada no existe.");
            }
        }

        var previousPrice = service.DefaultPrice;
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpper();

        service.Name = request.Name;
        service.Description = request.Description;
        service.CategoryId = request.CategoryId;
        service.BillingType = request.BillingType;
        service.DefaultPrice = request.DefaultPrice;
        service.Currency = currency;
        service.IsActive = request.IsActive;
        service.IsPublic = request.IsPublic;

        if (request.DefaultPrice != previousPrice)
        {
            service.PriceHistory.Add(new ServicePriceHistory
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                Service = service,
                Price = request.DefaultPrice,
                Currency = currency,
                EffectiveDate = DateTime.UtcNow,
                Reason = $"Cambio de precio de {previousPrice:C} a {request.DefaultPrice:C}"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(service);
    }

    private static ServiceDto MapToDto(Service service)
    {
        return new ServiceDto
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
            IsPublic = service.IsPublic
        };
    }
}
