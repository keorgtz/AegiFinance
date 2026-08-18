using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.RevokeUserSession;

public sealed record RevokeUserSessionCommand(Guid UserId, Guid SessionId, Guid ActorId) : IRequest;

public sealed class RevokeUserSessionCommandHandler : IRequestHandler<RevokeUserSessionCommand>
{
    private readonly IApplicationDbContext _context;
    public RevokeUserSessionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(RevokeUserSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.UserSessions.FirstOrDefaultAsync(item => item.Id == request.SessionId && item.UserId == request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("La sesión no existe.");
        if (session.RevokedAt is null)
        {
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedBy = request.ActorId;
            session.RevokedReason = "Remote administrator revocation";
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
