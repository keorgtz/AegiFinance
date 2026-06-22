using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientCategories.Commands.CreateClientCategory;

public class CreateClientCategoryCommandHandler : IRequestHandler<CreateClientCategoryCommand, ClientCategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateClientCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientCategoryDto> Handle(CreateClientCategoryCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.ClientCategories
            .AsNoTracking()
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
        }

        var category = new ClientCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        _context.ClientCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClientCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}
