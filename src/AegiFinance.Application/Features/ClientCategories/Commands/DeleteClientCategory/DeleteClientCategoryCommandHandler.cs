using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientCategories.Commands.DeleteClientCategory;

public class DeleteClientCategoryCommandHandler : IRequestHandler<DeleteClientCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteClientCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteClientCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ClientCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new InvalidOperationException("La categoría no existe.");
        }

        _context.ClientCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
