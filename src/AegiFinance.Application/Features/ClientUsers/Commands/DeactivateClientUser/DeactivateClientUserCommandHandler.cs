using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientUsers.Commands.DeactivateClientUser;

public class DeactivateClientUserCommandHandler : IRequestHandler<DeactivateClientUserCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateClientUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeactivateClientUserCommand request, CancellationToken cancellationToken)
    {
        var clientUser = await _context.ClientUsers
            .FirstOrDefaultAsync(cu => cu.Id == request.Id, cancellationToken);

        if (clientUser is null)
        {
            throw new InvalidOperationException("El subusuario no existe.");
        }

        clientUser.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
