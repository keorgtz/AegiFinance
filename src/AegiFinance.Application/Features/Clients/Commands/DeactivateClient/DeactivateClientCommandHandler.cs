using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.DeactivateClient;

public class DeactivateClientCommandHandler : IRequestHandler<DeactivateClientCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateClientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeactivateClientCommand request, CancellationToken cancellationToken)
    {
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
