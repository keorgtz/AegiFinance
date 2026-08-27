using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankImports;

public sealed record RollbackBankImportCommand(Guid Id) : IRequest;

public sealed class RollbackBankImportCommandHandler : IRequestHandler<RollbackBankImportCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public RollbackBankImportCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }

    public async Task Handle(RollbackBankImportCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var attempt = await _context.BankImportAttempts.Include(item => item.Rows).SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("El lote de importación no existe.");
        if (attempt.Status != BankImportStatus.Committed || !attempt.BankStatementId.HasValue) throw new InvalidOperationException("Sólo se puede revertir un lote confirmado.");
        var statement = await _context.BankStatements.Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == attempt.BankStatementId, cancellationToken)
            ?? throw new KeyNotFoundException("El estado bancario del lote no existe.");
        if (statement.Lines.Any(item => item.IsReconciled || item.LedgerEntryId.HasValue))
            throw new InvalidOperationException("No se puede revertir porque una o más líneas ya fueron conciliadas. Deshacé esas conciliaciones primero.");

        _context.BankStatementLines.RemoveRange(statement.Lines);
        _context.BankStatements.Remove(statement);
        attempt.Status = BankImportStatus.RolledBack;
        attempt.RolledBackAt = DateTime.UtcNow;
        attempt.RolledBackBy = _currentUser.UserId;
        foreach (var row in attempt.Rows) row.BankStatementLineId = null;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
