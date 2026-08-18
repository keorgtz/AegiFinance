using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AegiFinance.Domain.Entities;

namespace AegiFinance.Application.Features.Users.Commands.AssignRolesToUser;

public class AssignRolesToUserCommandHandler : IRequestHandler<AssignRolesToUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AssignRolesToUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
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

        if (roles.Any(role => role.UserType.HasValue && role.UserType.Value != user.UserType))
        {
            throw new InvalidOperationException("El tipo de uno o más roles no corresponde al tipo de usuario.");
        }

        var previousRoles = user.Roles.Select(role => role.Name).OrderBy(name => name).ToList();
        user.Roles.Clear();
        foreach (var role in roles)
        {
            user.Roles.Add(role);
        }

        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(), EntityType = "UserRoles", EntityId = user.Id.ToString(), Action = "Assigned",
            Changes = JsonSerializer.Serialize(new { Previous = previousRoles, Current = roles.Select(role => role.Name).OrderBy(name => name) }),
            UserId = _currentUser.UserId, Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
