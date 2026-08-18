using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("El usuario no existe.");
        }

        var sessions = await _context.UserSessions.Where(item => item.UserId == user.Id && item.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = "User deleted";
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
