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
        if (request.ExpiresAt.HasValue && request.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("La fecha de expiración debe estar en el futuro.");
        if (request.SubscriptionId.HasValue)
        {
            var subscription = await _context.Subscriptions.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == request.SubscriptionId.Value, cancellationToken)
                ?? throw new InvalidOperationException("La suscripción no existe.");
            if (request.ClientId.HasValue && subscription.ClientId != request.ClientId.Value)
                throw new InvalidOperationException("La suscripción no pertenece al cliente indicado.");
        }
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

        var userPermission = await _context.UserPermissionOverrides
            .FirstOrDefaultAsync(up => up.UserId == request.UserId &&
                up.PermissionId == request.PermissionId &&
                up.ClientId == request.ClientId &&
                up.SubscriptionId == request.SubscriptionId, cancellationToken);

        if (userPermission is null)
        {
            _context.UserPermissionOverrides.Add(new UserPermissionOverride
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PermissionId = request.PermissionId,
                IsGranted = request.IsGranted,
                ClientId = request.ClientId,
                SubscriptionId = request.SubscriptionId,
                ExpiresAt = request.ExpiresAt
            });
        }
        else
        {
            userPermission.IsGranted = request.IsGranted;
            userPermission.ExpiresAt = request.ExpiresAt;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
