using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Roles.Commands.AssignPermissionsToRole;

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignPermissionsToRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException("El rol no existe.");
        }

        var permissionIds = request.PermissionIds.Distinct().ToList();

        var permissions = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (permissions.Count != permissionIds.Count)
        {
            throw new InvalidOperationException("Uno o más permisos no existen.");
        }

        role.Permissions.Clear();
        foreach (var permission in permissions)
        {
            role.Permissions.Add(permission);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
