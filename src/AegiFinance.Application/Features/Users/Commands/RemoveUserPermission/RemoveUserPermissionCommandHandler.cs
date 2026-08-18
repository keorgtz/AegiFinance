using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.RemoveUserPermission;

public class RemoveUserPermissionCommandHandler : IRequestHandler<RemoveUserPermissionCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveUserPermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveUserPermissionCommand request, CancellationToken cancellationToken)
    {
        var userPermission = await _context.UserPermissionOverrides
            .FirstOrDefaultAsync(up => up.UserId == request.UserId &&
                up.PermissionId == request.PermissionId &&
                up.ClientId == request.ClientId &&
                up.SubscriptionId == request.SubscriptionId, cancellationToken);

        if (userPermission is null)
        {
            throw new InvalidOperationException("El override de permiso no existe.");
        }

        _context.UserPermissionOverrides.Remove(userPermission);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
