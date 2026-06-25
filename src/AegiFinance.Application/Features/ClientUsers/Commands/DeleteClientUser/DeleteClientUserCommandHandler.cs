using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientUsers.Commands.DeleteClientUser;

public class DeleteClientUserCommandHandler : IRequestHandler<DeleteClientUserCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteClientUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteClientUserCommand request, CancellationToken cancellationToken)
    {
        var clientUser = await _context.ClientUsers
            .FirstOrDefaultAsync(cu => cu.Id == request.Id, cancellationToken);

        if (clientUser is null)
        {
            throw new InvalidOperationException("El subusuario no existe.");
        }

        _context.ClientUsers.Remove(clientUser);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
