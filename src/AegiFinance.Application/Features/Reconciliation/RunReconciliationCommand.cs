using System.Text.Json;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Reconciliation;

public sealed class RunReconciliationCommand : IRequest<ReconciliationRunResultDto>
{
    public Guid BankAccountId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public sealed class RunReconciliationCommandHandler : IRequestHandler<RunReconciliationCommand, ReconciliationRunResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public RunReconciliationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }

    public async Task<ReconciliationRunResultDto> Handle(RunReconciliationCommand request, CancellationToken cancellationToken)
    {
        if (request.From.Date > request.To.Date) throw new InvalidOperationException("La fecha inicial no puede ser posterior a la final.");
        if ((request.To.Date - request.From.Date).TotalDays > 366) throw new InvalidOperationException("El periodo de búsqueda no puede exceder 366 días.");
        var organizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
        var account = await _context.BankAccounts.AsNoTracking().SingleOrDefaultAsync(item => item.Id == request.BankAccountId, cancellationToken)
            ?? throw new KeyNotFoundException("La cuenta bancaria no existe.");
        var settings = await _context.ReconciliationSettings.SingleOrDefaultAsync(cancellationToken);
        if (settings is null) { settings = ReconciliationSettingsFeature.New(organizationId); _context.ReconciliationSettings.Add(settings); }
        ReconciliationRules.ValidateSettings(settings);

        var priorSuggestions = await _context.ReconciliationCases.Where(item => item.BankAccountId == account.Id && item.Status == ReconciliationStatus.Suggested).ToListAsync(cancellationToken);
        foreach (var item in priorSuggestions) { item.Status = ReconciliationStatus.Rejected; item.RejectedAt = DateTime.UtcNow; item.ReversalReason = "Sustituida por una nueva ejecución del motor."; }

        var from = request.From.Date.AddDays(-settings.DateToleranceDays);
        var to = request.To.Date.AddDays(settings.DateToleranceDays + 1);
        var bankLines = await _context.BankStatementLines.Where(item => item.BankStatement.BankAccountId == account.Id && !item.IsReconciled && item.TransactionDate >= request.From.Date && item.TransactionDate < request.To.Date.AddDays(1))
            .OrderBy(item => item.TransactionDate).Take(500).ToListAsync(cancellationToken);
        var ledgerEntries = await _context.LedgerEntries.Include(item => item.Client)
            .Where(item => item.BankAccountId == account.Id && !item.IsReconciled && item.Currency == account.Currency && item.Date >= from && item.Date < to)
            .OrderBy(item => item.Date).Take(1000).ToListAsync(cancellationToken);

        var confirmedBank = await _context.ReconciliationCaseBankLines.AsNoTracking().Where(item => item.ReconciliationCase.Status == ReconciliationStatus.Confirmed)
            .GroupBy(item => item.BankStatementLineId).Select(group => new { Id = group.Key, Amount = group.Sum(item => item.AppliedAmount) }).ToDictionaryAsync(item => item.Id, item => item.Amount, cancellationToken);
        var confirmedLedger = await _context.ReconciliationCaseLedgerEntries.AsNoTracking().Where(item => item.ReconciliationCase.Status == ReconciliationStatus.Confirmed)
            .GroupBy(item => item.LedgerEntryId).Select(group => new { Id = group.Key, Amount = group.Sum(item => item.AppliedAmount) }).ToDictionaryAsync(item => item.Id, item => item.Amount, cancellationToken);
        decimal BankRemaining(BankStatementLine item) => Math.Max(0, Math.Abs(item.Amount) - confirmedBank.GetValueOrDefault(item.Id));
        decimal LedgerRemaining(LedgerEntry item) => Math.Max(0, Math.Abs(ReconciliationRules.SignedAmount(item)) - confirmedLedger.GetValueOrDefault(item.Id));
        bankLines = bankLines.Where(item => BankRemaining(item) > settings.AmountTolerance).ToList();
        ledgerEntries = ledgerEntries.Where(item => LedgerRemaining(item) > settings.AmountTolerance).ToList();

        var suggestions = new List<ReconciliationCase>();
        var coveredBankLines = new HashSet<Guid>();
        var coveredLedgerEntries = new HashSet<Guid>();
        foreach (var line in bankLines)
        {
            var candidates = ledgerEntries.Where(entry => !coveredLedgerEntries.Contains(entry.Id) && SameDirection(line.Amount, ReconciliationRules.SignedAmount(entry)) && Math.Abs((line.TransactionDate.Date - entry.Date.Date).Days) <= settings.DateToleranceDays)
                .Select(entry => new Candidate(entry, LedgerRemaining(entry), ReconciliationRules.Score(line.Amount < 0 ? -BankRemaining(line) : BankRemaining(line), line.TransactionDate, line.Description, line.Reference,
                    ReconciliationRules.SignedAmount(entry) < 0 ? -LedgerRemaining(entry) : LedgerRemaining(entry), entry.Date, entry.Description, entry.Reference, entry.Client?.Name, settings)))
                .OrderByDescending(item => item.Score.Score).Take(12).ToList();

            var bestSingle = candidates.FirstOrDefault(item => item.Score.Score >= settings.SuggestionThreshold);
            var bestCombination = FindCombination(line, candidates, BankRemaining(line), settings);
            ReconciliationCase? suggestion = null;
            if (bestCombination is not null && (bestSingle is null || bestCombination.Value.Score.Score > bestSingle.Score.Score))
                suggestion = CreateCase(organizationId, account.Id, [line], bestCombination.Value.Entries, ReconciliationMatchType.OneToMany, bestCombination.Value.Score, BankRemaining, LedgerRemaining);
            else if (bestSingle is not null)
            {
                var bankRemaining = BankRemaining(line); var ledgerRemaining = LedgerRemaining(bestSingle.Entry);
                var type = bestSingle.Score.AmountExact ? (bestSingle.Score.ReferenceExact ? ReconciliationMatchType.Exact : ReconciliationMatchType.Suggested) : ReconciliationMatchType.Partial;
                suggestion = CreateCase(organizationId, account.Id, [line], [bestSingle.Entry], type, bestSingle.Score, BankRemaining, LedgerRemaining);
            }
            if (suggestion is not null)
            {
                suggestions.Add(suggestion);
                coveredBankLines.UnionWith(suggestion.BankLines.Select(link => link.BankStatementLineId));
                coveredLedgerEntries.UnionWith(suggestion.LedgerEntries.Select(link => link.LedgerEntryId));
            }
        }

        foreach (var entry in ledgerEntries)
        {
            if (coveredLedgerEntries.Contains(entry.Id)) continue;
            var availableLines = bankLines.Where(line => !coveredBankLines.Contains(line.Id) && SameDirection(line.Amount, ReconciliationRules.SignedAmount(entry)) && Math.Abs((line.TransactionDate.Date - entry.Date.Date).Days) <= settings.DateToleranceDays).Take(12).ToList();
            var combination = FindBankCombination(entry, availableLines, LedgerRemaining(entry), settings, BankRemaining);
            if (combination is null) continue;
            suggestions.Add(CreateCase(organizationId, account.Id, combination.Value.Lines, [entry], ReconciliationMatchType.ManyToOne, combination.Value.Score, BankRemaining, LedgerRemaining));
            foreach (var line in combination.Value.Lines) coveredBankLines.Add(line.Id);
            coveredLedgerEntries.Add(entry.Id);
        }

        var autoConfirmed = 0;
        if (settings.AllowAutoConfirmExact)
        {
            var usedBankLines = new HashSet<Guid>(); var usedLedgerEntries = new HashSet<Guid>();
            foreach (var suggestion in suggestions.Where(item => item.MatchType == ReconciliationMatchType.Exact && item.Score >= settings.AutoConfirmThreshold))
            {
                if (suggestion.BankLines.Any(link => usedBankLines.Contains(link.BankStatementLineId)) || suggestion.LedgerEntries.Any(link => usedLedgerEntries.Contains(link.LedgerEntryId))) continue;
                usedBankLines.UnionWith(suggestion.BankLines.Select(link => link.BankStatementLineId));
                usedLedgerEntries.UnionWith(suggestion.LedgerEntries.Select(link => link.LedgerEntryId));
                suggestion.Status = ReconciliationStatus.Confirmed; suggestion.IsAutomatic = true; suggestion.ConfirmedAt = DateTime.UtcNow; suggestion.ConfirmedBy = _currentUser.UserId;
                foreach (var link in suggestion.BankLines) { link.BankStatementLine.IsReconciled = true; link.BankStatementLine.ReconciliationVersion++; }
                foreach (var link in suggestion.LedgerEntries) { link.LedgerEntry.IsReconciled = true; link.LedgerEntry.ReconciledAt = DateTime.UtcNow; link.LedgerEntry.ReconciliationVersion++; }
                autoConfirmed++;
            }
        }
        _context.ReconciliationCases.AddRange(suggestions);
        await _context.SaveChangesAsync(cancellationToken);
        return new ReconciliationRunResultDto
        {
            SuggestionsCreated = suggestions.Count,
            ExactMatches = suggestions.Count(item => item.MatchType == ReconciliationMatchType.Exact),
            CombinedMatches = suggestions.Count(item => item.MatchType is ReconciliationMatchType.OneToMany or ReconciliationMatchType.ManyToOne),
            PartialMatches = suggestions.Count(item => item.MatchType == ReconciliationMatchType.Partial),
            AutoConfirmed = autoConfirmed
        };
    }

    private static ReconciliationCase CreateCase(Guid organizationId, Guid accountId, IReadOnlyList<BankStatementLine> lines, IReadOnlyList<LedgerEntry> entries,
        ReconciliationMatchType type, ReconciliationScore score, Func<BankStatementLine, decimal> bankRemaining, Func<LedgerEntry, decimal> ledgerRemaining)
    {
        var bankTotal = lines.Sum(item => Signed(bankRemaining(item), item.Amount));
        var ledgerTotal = entries.Sum(item => Signed(ledgerRemaining(item), ReconciliationRules.SignedAmount(item)));
        var applied = Math.Min(Math.Abs(bankTotal), Math.Abs(ledgerTotal));
        var caseBankAmount = lines.Count == 1 ? Signed(applied, bankTotal) : bankTotal;
        var caseLedgerAmount = entries.Count == 1 ? Signed(applied, ledgerTotal) : ledgerTotal;
        var result = new ReconciliationCase
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, BankAccountId = accountId, Status = ReconciliationStatus.Suggested,
            MatchType = type, Score = score.Score, ExplanationJson = JsonSerializer.Serialize(score.Factors), BankAmount = caseBankAmount,
            LedgerAmount = caseLedgerAmount, DifferenceAmount = caseBankAmount - caseLedgerAmount, DifferenceType = ReconciliationDifferenceType.None,
            IsAutomatic = false, GeneratedAt = DateTime.UtcNow
        };
        foreach (var line in lines) result.BankLines.Add(new ReconciliationCaseBankLine { Id = Guid.NewGuid(), BankStatementLineId = line.Id, BankStatementLine = line, AppliedAmount = lines.Count == 1 ? applied : bankRemaining(line) });
        foreach (var entry in entries) result.LedgerEntries.Add(new ReconciliationCaseLedgerEntry { Id = Guid.NewGuid(), LedgerEntryId = entry.Id, LedgerEntry = entry, AppliedAmount = entries.Count == 1 ? applied : ledgerRemaining(entry) });
        return result;
    }

    private static (IReadOnlyList<LedgerEntry> Entries, ReconciliationScore Score)? FindCombination(BankStatementLine line, IReadOnlyList<Candidate> candidates, decimal target, ReconciliationSettings settings)
    {
        for (var size = 2; size <= Math.Min(3, candidates.Count); size++)
            foreach (var combination in Combinations(candidates, size))
            {
                var total = combination.Sum(item => item.Remaining);
                if (Math.Abs(total - target) > settings.AmountTolerance) continue;
                var entries = combination.Select(item => item.Entry).ToList();
                var score = ReconciliationRules.Score(line.Amount, line.TransactionDate, line.Description, line.Reference, Signed(total, line.Amount),
                    entries.Min(item => item.Date), string.Join(' ', entries.Select(item => item.Description)), string.Join(' ', entries.Select(item => item.Reference)),
                    string.Join(' ', entries.Select(item => item.Client?.Name)), settings);
                if (score.Score >= settings.SuggestionThreshold) return (entries, score);
            }
        return null;
    }

    private static (IReadOnlyList<BankStatementLine> Lines, ReconciliationScore Score)? FindBankCombination(LedgerEntry entry, IReadOnlyList<BankStatementLine> lines, decimal target,
        ReconciliationSettings settings, Func<BankStatementLine, decimal> remaining)
    {
        for (var size = 2; size <= Math.Min(3, lines.Count); size++)
            foreach (var combination in Combinations(lines, size))
            {
                var total = combination.Sum(remaining);
                if (Math.Abs(total - target) > settings.AmountTolerance) continue;
                var score = ReconciliationRules.Score(Signed(total, ReconciliationRules.SignedAmount(entry)), combination.Min(item => item.TransactionDate),
                    string.Join(' ', combination.Select(item => item.Description)), string.Join(' ', combination.Select(item => item.Reference)),
                    ReconciliationRules.SignedAmount(entry), entry.Date, entry.Description, entry.Reference, entry.Client?.Name, settings);
                if (score.Score >= settings.SuggestionThreshold) return (combination, score);
            }
        return null;
    }

    private static IEnumerable<List<T>> Combinations<T>(IReadOnlyList<T> source, int size)
    {
        IEnumerable<List<T>> Walk(int start, List<T> current)
        {
            if (current.Count == size) { yield return [.. current]; yield break; }
            for (var index = start; index <= source.Count - (size - current.Count); index++)
            { current.Add(source[index]); foreach (var item in Walk(index + 1, current)) yield return item; current.RemoveAt(current.Count - 1); }
        }
        return Walk(0, []);
    }

    private static bool SameDirection(decimal left, decimal right) => left != 0 && right != 0 && Math.Sign(left) == Math.Sign(right);
    private static decimal Signed(decimal magnitude, decimal sign) => sign < 0 ? -Math.Abs(magnitude) : Math.Abs(magnitude);
    private sealed record Candidate(LedgerEntry Entry, decimal Remaining, ReconciliationScore Score);
}
