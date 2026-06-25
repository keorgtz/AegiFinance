using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.DeactivateService;

public class DeactivateServiceCommandHandler : IRequestHandler<DeactivateServiceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateServiceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeactivateServiceCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para desactivar servicios.");
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }

        service.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
