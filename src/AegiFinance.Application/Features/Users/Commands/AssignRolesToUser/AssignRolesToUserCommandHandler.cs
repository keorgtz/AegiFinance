using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.AssignRolesToUser;

public class AssignRolesToUserCommandHandler : IRequestHandler<AssignRolesToUserCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignRolesToUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssignRolesToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("El usuario no existe.");
        }

        var roleIds = request.RoleIds.Distinct().ToList();

        var roles = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        if (roles.Count != roleIds.Count)
        {
            throw new InvalidOperationException("Uno o más roles no existen.");
        }

        user.Roles.Clear();
        foreach (var role in roles)
        {
            user.Roles.Add(role);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
