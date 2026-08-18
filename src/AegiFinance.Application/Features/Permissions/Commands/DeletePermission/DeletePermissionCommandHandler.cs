using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Permissions.Commands.DeletePermission;

public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (permission is null)
        {
            throw new InvalidOperationException("El permiso no existe.");
        }

        if (permission.RolePermissions.Count > 0)
        {
            throw new InvalidOperationException("No se puede eliminar un permiso asignado a uno o más roles.");
        }

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
