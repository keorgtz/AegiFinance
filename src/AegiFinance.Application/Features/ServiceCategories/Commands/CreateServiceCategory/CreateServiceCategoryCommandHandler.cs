using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ServiceCategories.Commands.CreateServiceCategory;

public class CreateServiceCategoryCommandHandler : IRequestHandler<CreateServiceCategoryCommand, ServiceCategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateServiceCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceCategoryDto> Handle(CreateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para crear categorías de servicio.");
        }

        var exists = await _context.ServiceCategories
            .AsNoTracking()
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
        }

        var category = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        _context.ServiceCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}
