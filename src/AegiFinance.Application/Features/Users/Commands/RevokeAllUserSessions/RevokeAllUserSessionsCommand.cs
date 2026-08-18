using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.RevokeAllUserSessions;

public sealed record RevokeAllUserSessionsCommand(Guid UserId, Guid ActorId) : IRequest;

public sealed class RevokeAllUserSessionsCommandHandler : IRequestHandler<RevokeAllUserSessionsCommand>
{
    private readonly IApplicationDbContext _context;
    public RevokeAllUserSessionsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(RevokeAllUserSessionsCommand request, CancellationToken cancellationToken)
    {
        var sessions = await _context.UserSessions.Where(item => item.UserId == request.UserId && item.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedBy = request.ActorId;
            session.RevokedReason = "Remote administrator revocation";
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
