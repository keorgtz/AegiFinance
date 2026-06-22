using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientCategories.Commands.UpdateClientCategory;

public class UpdateClientCategoryCommandHandler : IRequestHandler<UpdateClientCategoryCommand, ClientCategoryDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateClientCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientCategoryDto> Handle(UpdateClientCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ClientCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new InvalidOperationException("La categoría no existe.");
        }

        var nameExists = await _context.ClientCategories
            .AsNoTracking()
            .AnyAsync(c => c.Id != request.Id && c.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("Ya existe otra categoría con ese nombre.");
        }

        category.Name = request.Name;
        category.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return new ClientCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}
