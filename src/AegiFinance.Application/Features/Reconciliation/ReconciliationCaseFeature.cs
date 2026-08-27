using System.Text.Json;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Reconciliation;

public sealed class GetReconciliationCasesQuery : IRequest<IReadOnlyList<ReconciliationCaseDto>>
{
    public Guid? BankAccountId { get; set; }
    public ReconciliationStatus? Status { get; set; }
    public int Limit { get; set; } = 100;
}
public sealed record GetReconciliationCaseQuery(Guid Id) : IRequest<ReconciliationCaseDto>;
public sealed class ConfirmReconciliationCaseCommand : IRequest<ReconciliationCaseDto>
{
    public Guid Id { get; set; }
    public ReconciliationDifferenceType DifferenceType { get; set; }
    public string? DifferenceReason { get; set; }
}
public sealed record RejectReconciliationCaseCommand(Guid Id, string? Reason) : IRequest;
public sealed record ReverseReconciliationCaseCommand(Guid Id, string Reason) : IRequest;

public sealed class GetReconciliationCasesQueryHandler : IRequestHandler<GetReconciliationCasesQuery, IReadOnlyList<ReconciliationCaseDto>>
{
    private readonly IApplicationDbContext _context;
    public GetReconciliationCasesQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<ReconciliationCaseDto>> Handle(GetReconciliationCasesQuery request, CancellationToken cancellationToken)
    {
        var query = ReconciliationCaseFeature.Query(_context);
        if (request.BankAccountId.HasValue) query = query.Where(item => item.BankAccountId == request.BankAccountId);
        if (request.Status.HasValue) query = query.Where(item => item.Status == request.Status);
        var cases = await query.OrderByDescending(item => item.GeneratedAt).Take(Math.Clamp(request.Limit, 1, 200)).ToListAsync(cancellationToken);
        return cases.Select(ReconciliationCaseFeature.Map).ToList();
    }
}

public sealed class GetReconciliationCaseQueryHandler : IRequestHandler<GetReconciliationCaseQuery, ReconciliationCaseDto>
{
    private readonly IApplicationDbContext _context;
    public GetReconciliationCaseQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<ReconciliationCaseDto> Handle(GetReconciliationCaseQuery request, CancellationToken cancellationToken)
    {
        var item = await ReconciliationCaseFeature.Query(_context).SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("El caso de conciliación no existe.");
        return ReconciliationCaseFeature.Map(item);
    }
}

public sealed class ConfirmReconciliationCaseCommandHandler : IRequestHandler<ConfirmReconciliationCaseCommand, ReconciliationCaseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public ConfirmReconciliationCaseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<ReconciliationCaseDto> Handle(ConfirmReconciliationCaseCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var item = await ReconciliationCaseFeature.Query(_context).SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("El caso de conciliación no existe.");
        if (item.Status != ReconciliationStatus.Suggested) throw new InvalidOperationException("Sólo se puede confirmar un caso sugerido.");
        var settings = await _context.ReconciliationSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken)
            ?? ReconciliationSettingsFeature.New(item.OrganizationId);
        item.DifferenceType = request.DifferenceType;
        item.DifferenceReason = string.IsNullOrWhiteSpace(request.DifferenceReason) ? null : request.DifferenceReason.Trim();
        ReconciliationRules.ValidateDifference(item.DifferenceAmount, settings.AmountTolerance, item.DifferenceType, item.DifferenceReason);

        await ReconciliationCaseFeature.ValidateAvailabilityAsync(_context, item, settings.AmountTolerance, cancellationToken);
        item.Status = ReconciliationStatus.Confirmed; item.ConfirmedAt = DateTime.UtcNow; item.ConfirmedBy = _currentUser.UserId;
        await ReconciliationCaseFeature.RefreshFlagsAsync(_context, item, settings.AmountTolerance, true, cancellationToken);
        try { await _context.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { throw new InvalidOperationException("Otra conciliación utilizó una de estas partidas. Actualizá las sugerencias e intentá de nuevo."); }
        return ReconciliationCaseFeature.Map(item);
    }
}

public sealed class RejectReconciliationCaseCommandHandler : IRequestHandler<RejectReconciliationCaseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public RejectReconciliationCaseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task Handle(RejectReconciliationCaseCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ReconciliationCases.SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken) ?? throw new KeyNotFoundException("El caso no existe.");
        if (item.Status != ReconciliationStatus.Suggested) throw new InvalidOperationException("Sólo se puede rechazar una sugerencia pendiente.");
        item.Status = ReconciliationStatus.Rejected; item.RejectedAt = DateTime.UtcNow; item.RejectedBy = _currentUser.UserId; item.ReversalReason = request.Reason?.Trim();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ReverseReconciliationCaseCommandHandler : IRequestHandler<ReverseReconciliationCaseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public ReverseReconciliationCaseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task Handle(ReverseReconciliationCaseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new InvalidOperationException("El motivo de reversión es obligatorio.");
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var item = await ReconciliationCaseFeature.Query(_context).SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken) ?? throw new KeyNotFoundException("El caso no existe.");
        if (item.Status != ReconciliationStatus.Confirmed) throw new InvalidOperationException("Sólo se puede revertir una conciliación confirmada.");
        var lineDates = item.BankLines.Select(link => link.BankStatementLine.TransactionDate.Date).ToList();
        var closedPeriods = await _context.ReconciliationPeriods.AsNoTracking().Where(period => period.BankAccountId == item.BankAccountId && period.Status == ReconciliationPeriodStatus.Closed).ToListAsync(cancellationToken);
        var closed = closedPeriods.Any(period => lineDates.Any(date => date >= period.StartDate && date <= period.EndDate));
        if (closed) throw new InvalidOperationException("La conciliación pertenece a un periodo cerrado y no puede revertirse.");
        item.Status = ReconciliationStatus.Reversed; item.ReversedAt = DateTime.UtcNow; item.ReversedBy = _currentUser.UserId; item.ReversalReason = request.Reason.Trim();
        var settings = await _context.ReconciliationSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken) ?? ReconciliationSettingsFeature.New(item.OrganizationId);
        await ReconciliationCaseFeature.RefreshFlagsAsync(_context, item, settings.AmountTolerance, false, cancellationToken);
        try { await _context.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { throw new InvalidOperationException("Las partidas cambiaron mientras se revertía la conciliación. Actualizá e intentá de nuevo."); }
    }
}

internal static class ReconciliationCaseFeature
{
    public static IQueryable<ReconciliationCase> Query(IApplicationDbContext context) => context.ReconciliationCases
        .Include(item => item.BankAccount)
        .Include(item => item.BankLines).ThenInclude(item => item.BankStatementLine)
        .Include(item => item.LedgerEntries).ThenInclude(item => item.LedgerEntry).ThenInclude(item => item.Client);

    public static async Task ValidateAvailabilityAsync(IApplicationDbContext context, ReconciliationCase item, decimal tolerance, CancellationToken cancellationToken)
    {
        foreach (var link in item.BankLines)
        {
            var applied = await context.ReconciliationCaseBankLines.AsNoTracking().Where(other => other.BankStatementLineId == link.BankStatementLineId && other.ReconciliationCase.Status == ReconciliationStatus.Confirmed)
                .SumAsync(other => other.AppliedAmount, cancellationToken);
            if (link.AppliedAmount <= 0 || applied + link.AppliedAmount > Math.Abs(link.BankStatementLine.Amount) + tolerance)
                throw new InvalidOperationException($"La línea bancaria del {link.BankStatementLine.TransactionDate:d} ya no tiene saldo suficiente.");
        }
        foreach (var link in item.LedgerEntries)
        {
            var applied = await context.ReconciliationCaseLedgerEntries.AsNoTracking().Where(other => other.LedgerEntryId == link.LedgerEntryId && other.ReconciliationCase.Status == ReconciliationStatus.Confirmed)
                .SumAsync(other => other.AppliedAmount, cancellationToken);
            if (link.AppliedAmount <= 0 || applied + link.AppliedAmount > Math.Abs(ReconciliationRules.SignedAmount(link.LedgerEntry)) + tolerance)
                throw new InvalidOperationException($"El movimiento contable del {link.LedgerEntry.Date:d} ya no tiene saldo suficiente.");
        }
    }

    public static async Task RefreshFlagsAsync(IApplicationDbContext context, ReconciliationCase item, decimal tolerance, bool confirming, CancellationToken cancellationToken)
    {
        foreach (var link in item.BankLines)
        {
            var other = await context.ReconciliationCaseBankLines.AsNoTracking().Where(candidate => candidate.BankStatementLineId == link.BankStatementLineId && candidate.ReconciliationCaseId != item.Id && candidate.ReconciliationCase.Status == ReconciliationStatus.Confirmed)
                .SumAsync(candidate => candidate.AppliedAmount, cancellationToken);
            var total = other + (confirming ? link.AppliedAmount : 0);
            link.BankStatementLine.IsReconciled = total >= Math.Abs(link.BankStatementLine.Amount) - tolerance;
            link.BankStatementLine.ReconciliationVersion++;
        }
        foreach (var link in item.LedgerEntries)
        {
            var other = await context.ReconciliationCaseLedgerEntries.AsNoTracking().Where(candidate => candidate.LedgerEntryId == link.LedgerEntryId && candidate.ReconciliationCaseId != item.Id && candidate.ReconciliationCase.Status == ReconciliationStatus.Confirmed)
                .SumAsync(candidate => candidate.AppliedAmount, cancellationToken);
            var total = other + (confirming ? link.AppliedAmount : 0);
            link.LedgerEntry.IsReconciled = total >= Math.Abs(ReconciliationRules.SignedAmount(link.LedgerEntry)) - tolerance;
            link.LedgerEntry.ReconciledAt = link.LedgerEntry.IsReconciled ? DateTime.UtcNow : null;
            link.LedgerEntry.ReconciliationVersion++;
        }
    }

    public static ReconciliationCaseDto Map(ReconciliationCase item) => new()
    {
        Id = item.Id, BankAccountId = item.BankAccountId, BankAccountName = item.BankAccount.Name, Status = item.Status,
        MatchType = item.MatchType, Score = item.Score, BankAmount = item.BankAmount, LedgerAmount = item.LedgerAmount,
        DifferenceAmount = item.DifferenceAmount, DifferenceType = item.DifferenceType, DifferenceReason = item.DifferenceReason,
        IsAutomatic = item.IsAutomatic, GeneratedAt = item.GeneratedAt, ConfirmedAt = item.ConfirmedAt, ReversedAt = item.ReversedAt,
        Factors = DeserializeFactors(item.ExplanationJson),
        BankLines = item.BankLines.Select(link => new ReconciliationBankLineDto { Id = link.BankStatementLineId, Date = link.BankStatementLine.TransactionDate, Description = link.BankStatementLine.Description, Reference = link.BankStatementLine.Reference, Amount = link.BankStatementLine.Amount, AppliedAmount = link.AppliedAmount, Currency = link.BankStatementLine.Currency }).ToList(),
        LedgerEntries = item.LedgerEntries.Select(link => new ReconciliationLedgerEntryDto { Id = link.LedgerEntryId, Date = link.LedgerEntry.Date, Description = link.LedgerEntry.Description, Reference = link.LedgerEntry.Reference, ClientName = link.LedgerEntry.Client?.Name, Amount = ReconciliationRules.SignedAmount(link.LedgerEntry), AppliedAmount = link.AppliedAmount, Currency = link.LedgerEntry.Currency }).ToList()
    };

    private static IReadOnlyList<ReconciliationFactorDto> DeserializeFactors(string json)
    {
        try { return (JsonSerializer.Deserialize<List<ReconciliationFactor>>(json) ?? []).Select(item => new ReconciliationFactorDto { Code = item.Code, Label = item.Label, Points = item.Points, Detail = item.Detail }).ToList(); }
        catch (JsonException) { return []; }
    }
}
