using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.ActivateClient;

public class ActivateClientCommandHandler : IRequestHandler<ActivateClientCommand>
{
    private readonly IApplicationDbContext _context;

    public ActivateClientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ActivateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        client.Status = ClientStatus.Active;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
