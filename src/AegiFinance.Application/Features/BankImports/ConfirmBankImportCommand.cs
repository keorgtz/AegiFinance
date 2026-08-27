using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankImports;

public sealed record ConfirmBankImportCommand(Guid Id) : IRequest<BankImportBatchDto>;

public sealed class ConfirmBankImportCommandHandler : IRequestHandler<ConfirmBankImportCommand, BankImportBatchDto>
{
    private readonly IApplicationDbContext _context;
    public ConfirmBankImportCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BankImportBatchDto> Handle(ConfirmBankImportCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var attempt = await _context.BankImportAttempts.Include(item => item.BankAccount).Include(item => item.Rows)
            .SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("El lote de importación no existe.");
        if (attempt.Status != BankImportStatus.Preview) throw new InvalidOperationException("Sólo se puede confirmar un lote en vista previa.");

        var candidates = attempt.Rows.Where(item => item.Status == BankImportRowStatus.Valid).OrderBy(item => item.RowNumber).ToList();
        if (candidates.Count == 0) throw new InvalidOperationException("El lote no tiene filas válidas para importar.");
        var hashes = candidates.Select(item => item.DeduplicationHash).Where(item => item != null).Cast<string>().ToList();
        var existingHashes = await _context.BankStatementLines.AsNoTracking().Where(item => item.DeduplicationHash != null && hashes.Contains(item.DeduplicationHash))
            .Select(item => item.DeduplicationHash!).ToHashSetAsync(cancellationToken);
        foreach (var duplicate in candidates.Where(item => item.DeduplicationHash is null || existingHashes.Contains(item.DeduplicationHash))) duplicate.Status = BankImportRowStatus.Duplicate;
        candidates = candidates.Where(item => item.Status == BankImportRowStatus.Valid).ToList();
        if (candidates.Count == 0) throw new InvalidOperationException("Todas las filas válidas ya habían sido importadas. No se creó un estado duplicado.");

        var balanceEvidence = ResolveBalances(candidates);
        var statement = new BankStatement
        {
            Id = Guid.NewGuid(), BankAccountId = attempt.BankAccountId, StatementDate = DateTime.UtcNow,
            StartDate = candidates.Min(item => item.TransactionDate!.Value), EndDate = candidates.Max(item => item.TransactionDate!.Value),
            OpeningBalance = balanceEvidence.OpeningBalance,
            ClosingBalance = balanceEvidence.ClosingBalance, IsBalanceVerified = balanceEvidence.Verified,
            FileUrl = attempt.FileName, FileHash = attempt.FileHash, ImportAttemptId = attempt.Id
        };
        _context.BankStatements.Add(statement);

        foreach (var row in candidates)
        {
            var line = new BankStatementLine
            {
                Id = Guid.NewGuid(), BankStatementId = statement.Id, TransactionDate = row.TransactionDate!.Value,
                Description = row.Description!, Reference = row.Reference, Amount = row.Amount!.Value,
                Currency = row.Currency!, BankBalance = row.Balance, DeduplicationHash = row.DeduplicationHash,
                SourceRowNumber = row.RowNumber, IsReconciled = false
            };
            _context.BankStatementLines.Add(line);
            row.BankStatementLineId = line.Id;
        }

        attempt.BankStatementId = statement.Id;
        attempt.Status = BankImportStatus.Committed;
        attempt.RecordsImported = candidates.Count;
        attempt.ValidRecords = candidates.Count;
        attempt.DuplicateRecords = attempt.Rows.Count(item => item.Status == BankImportRowStatus.Duplicate);
        attempt.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return BankImportQueries.Map(attempt, attempt.BankAccount.Name);
    }

    private static (decimal OpeningBalance, decimal ClosingBalance, bool Verified) ResolveBalances(IReadOnlyList<BankImportRow> rows)
    {
        if (rows.Any(item => !item.Balance.HasValue)) return (0, 0, false);
        if (rows.Count == 1) return (rows[0].Balance.GetValueOrDefault() - rows[0].Amount.GetValueOrDefault(), rows[0].Balance.GetValueOrDefault(), true);
        var forward = Enumerable.Range(1, rows.Count - 1).All(index => rows[index - 1].Balance.GetValueOrDefault() + rows[index].Amount.GetValueOrDefault() == rows[index].Balance.GetValueOrDefault());
        if (forward) return (rows[0].Balance.GetValueOrDefault() - rows[0].Amount.GetValueOrDefault(), rows[^1].Balance.GetValueOrDefault(), true);
        var reverse = Enumerable.Range(1, rows.Count - 1).All(index => rows[index].Balance.GetValueOrDefault() + rows[index - 1].Amount.GetValueOrDefault() == rows[index - 1].Balance.GetValueOrDefault());
        return reverse
            ? (rows[^1].Balance.GetValueOrDefault() - rows[^1].Amount.GetValueOrDefault(), rows[0].Balance.GetValueOrDefault(), true)
            : (0, rows[^1].Balance.GetValueOrDefault(), false);
    }
}
