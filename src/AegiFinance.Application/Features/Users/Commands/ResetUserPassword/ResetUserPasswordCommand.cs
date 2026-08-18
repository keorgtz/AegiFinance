using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Validators;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace AegiFinance.Application.Features.Users.Commands.ResetUserPassword;

public sealed class ResetUserPasswordCommand : IRequest
{
    public Guid UserId { get; set; }
    public string TemporaryPassword { get; set; } = string.Empty;
}

public sealed class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator() => RuleFor(item => item.TemporaryPassword).NotEmpty().MustBeStrongPassword();
}

public sealed class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    public ResetUserPasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    { _context = context; _passwordHasher = passwordHasher; }

    public async Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("El usuario no existe.");
        user.PasswordHash = _passwordHasher.HashPassword(request.TemporaryPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.MustChangePassword = true;
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        var sessions = await _context.UserSessions.Where(item => item.UserId == user.Id && item.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var session in sessions) { session.RevokedAt = DateTime.UtcNow; session.RevokedReason = "Password reset"; }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
