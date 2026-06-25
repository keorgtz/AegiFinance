using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Commands.CreateSubscriptionPermission;

public class CreateSubscriptionPermissionCommandHandler : IRequestHandler<CreateSubscriptionPermissionCommand, SubscriptionPermissionDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSubscriptionPermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionPermissionDto> Handle(CreateSubscriptionPermissionCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("El usuario no existe.");
        }

        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Service)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        if (user.ClientId.HasValue && subscription.ClientId != user.ClientId.Value)
        {
            throw new InvalidOperationException("La suscripción no pertenece al mismo cliente que el usuario.");
        }

        var exists = await _context.SubscriptionPermissions
            .AsNoTracking()
            .AnyAsync(sp => sp.UserId == request.UserId && sp.SubscriptionId == request.SubscriptionId, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("El usuario ya tiene visibilidad sobre esta suscripción.");
        }

        var subscriptionPermission = new SubscriptionPermission
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SubscriptionId = request.SubscriptionId
        };

        _context.SubscriptionPermissions.Add(subscriptionPermission);
        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriptionPermissionDto
        {
            Id = subscriptionPermission.Id,
            UserId = subscriptionPermission.UserId,
            UserName = user.Name,
            SubscriptionId = subscriptionPermission.SubscriptionId,
            SubscriptionCode = subscription.Code,
            ServiceName = subscription.Service.Name
        };
    }
}
