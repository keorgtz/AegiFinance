using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.CreateService;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IServiceCodeGenerator _codeGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateServiceCommandHandler(IApplicationDbContext context, IServiceCodeGenerator codeGenerator, ICurrentUserService currentUserService)
    {
        _context = context;
        _codeGenerator = codeGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceDto> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para crear servicios.");
        }

        ServiceCategory? category = null;
        if (request.CategoryId.HasValue)
        {
            category = await _context.ServiceCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

            if (category is null)
            {
                throw new InvalidOperationException("La categoría seleccionada no existe.");
            }
        }

        var code = await _codeGenerator.GenerateAsync(cancellationToken);
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpper();

        var service = new Service
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Category = category,
            BillingType = request.BillingType,
            DefaultPrice = request.DefaultPrice,
            Currency = currency,
            IsActive = request.IsActive,
            IsPublic = request.IsPublic
        };

        if (request.DefaultPrice > 0)
        {
            service.PriceHistory.Add(new ServicePriceHistory
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                Service = service,
                Price = request.DefaultPrice,
                Currency = currency,
                EffectiveDate = DateTime.UtcNow
            });
        }

        _context.Services.Add(service);
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
