using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.DeleteService;

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteServiceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para eliminar servicios.");
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }

        var hasSubscriptions = await _context.Subscriptions
            .AsNoTracking()
            .AnyAsync(s => s.ServiceId == service.Id, cancellationToken);

        if (hasSubscriptions)
        {
            throw new InvalidOperationException("No se puede eliminar un servicio con suscripciones asociadas. Desactívelo en su lugar.");
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
