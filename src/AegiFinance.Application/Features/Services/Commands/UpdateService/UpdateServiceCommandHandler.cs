using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.UpdateService;

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateServiceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceDto> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para modificar servicios.");
        }

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

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpper();
        if (request.BillingType != service.BillingType || request.DefaultPrice != service.DefaultPrice || currency != service.Currency)
            throw new InvalidOperationException("La periodicidad, el precio y la moneda se cambian publicando una nueva versión del plan.");

        service.Name = request.Name;
        service.Description = request.Description;
        service.CategoryId = request.CategoryId;
        service.IsActive = request.IsActive;
        service.IsPublic = request.IsPublic;

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
