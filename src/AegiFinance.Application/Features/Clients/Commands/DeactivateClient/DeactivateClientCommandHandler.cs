using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.DeactivateClient;

public class DeactivateClientCommandHandler : IRequestHandler<DeactivateClientCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateClientCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeactivateClientCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para desactivar clientes.");
        }

        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        client.Status = ClientStatus.Inactive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
