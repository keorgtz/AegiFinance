using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ServiceCategories.Commands.CreateServiceCategory;

public class CreateServiceCategoryCommandHandler : IRequestHandler<CreateServiceCategoryCommand, ServiceCategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateServiceCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceCategoryDto> Handle(CreateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
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
