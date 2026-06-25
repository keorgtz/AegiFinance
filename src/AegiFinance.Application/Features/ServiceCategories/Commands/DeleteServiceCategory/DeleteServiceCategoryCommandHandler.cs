using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ServiceCategories.Commands.DeleteServiceCategory;

public class DeleteServiceCategoryCommandHandler : IRequestHandler<DeleteServiceCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteServiceCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para eliminar categorías de servicio.");
        }

        var category = await _context.ServiceCategories
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new InvalidOperationException("La categoría no existe.");
        }

        if (category.Services.Any())
        {
            throw new InvalidOperationException("No se puede eliminar una categoría con servicios asociados.");
        }

        _context.ServiceCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
