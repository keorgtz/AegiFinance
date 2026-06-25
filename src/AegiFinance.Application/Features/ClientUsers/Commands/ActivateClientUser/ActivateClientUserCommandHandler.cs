using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientUsers.Commands.ActivateClientUser;

public class ActivateClientUserCommandHandler : IRequestHandler<ActivateClientUserCommand>
{
    private readonly IApplicationDbContext _context;

    public ActivateClientUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ActivateClientUserCommand request, CancellationToken cancellationToken)
    {
        var clientUser = await _context.ClientUsers
            .FirstOrDefaultAsync(cu => cu.Id == request.Id, cancellationToken);

        if (clientUser is null)
        {
            throw new InvalidOperationException("El subusuario no existe.");
        }

        clientUser.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
