using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Commands.DeleteSubscriptionPermission;

public class DeleteSubscriptionPermissionCommandHandler : IRequestHandler<DeleteSubscriptionPermissionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSubscriptionPermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteSubscriptionPermissionCommand request, CancellationToken cancellationToken)
    {
        var subscriptionPermission = await _context.SubscriptionPermissions
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (subscriptionPermission is null)
        {
            throw new InvalidOperationException("La asignación de visibilidad no existe.");
        }

        _context.SubscriptionPermissions.Remove(subscriptionPermission);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
