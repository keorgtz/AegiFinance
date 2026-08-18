using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.UnlockUser;

public sealed record UnlockUserCommand(Guid UserId) : IRequest;

public sealed class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand>
{
    private readonly IApplicationDbContext _context;
    public UnlockUserCommandHandler(IApplicationDbContext context) => _context = context;
    public async Task Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("El usuario no existe.");
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
