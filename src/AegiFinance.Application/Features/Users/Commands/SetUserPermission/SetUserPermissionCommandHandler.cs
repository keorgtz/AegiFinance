using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.SetUserPermission;

public class SetUserPermissionCommandHandler : IRequestHandler<SetUserPermissionCommand>
{
    private readonly IApplicationDbContext _context;

    public SetUserPermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SetUserPermissionCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new InvalidOperationException("El usuario no existe.");
        }

        var permissionExists = await _context.Permissions.AsNoTracking().AnyAsync(p => p.Id == request.PermissionId, cancellationToken);
        if (!permissionExists)
        {
            throw new InvalidOperationException("El permiso no existe.");
        }

        var userPermission = await _context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == request.UserId && up.PermissionId == request.PermissionId, cancellationToken);

        if (userPermission is null)
        {
            _context.UserPermissions.Add(new UserPermission
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PermissionId = request.PermissionId,
                IsGranted = request.IsGranted
            });
        }
        else
        {
            userPermission.IsGranted = request.IsGranted;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
