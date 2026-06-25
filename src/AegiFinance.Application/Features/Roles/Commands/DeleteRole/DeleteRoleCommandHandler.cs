using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException("El rol no existe.");
        }

        if (role.Users.Count > 0)
        {
            throw new InvalidOperationException("No se puede eliminar un rol con usuarios asignados.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
