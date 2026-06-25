using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientCategories.Commands.DeleteClientCategory;

public class DeleteClientCategoryCommandHandler : IRequestHandler<DeleteClientCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteClientCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteClientCategoryCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para eliminar categorías.");
        }

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
