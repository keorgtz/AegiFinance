using System.Text.Json;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Reconciliation;

public sealed class CreateManualReconciliationCommand : IRequest<ReconciliationCaseDto>
{
    public Guid BankAccountId { get; set; }
    public List<ReconciliationSelection> BankLines { get; set; } = [];
    public List<ReconciliationSelection> LedgerEntries { get; set; } = [];
    public ReconciliationDifferenceType DifferenceType { get; set; }
    public string? DifferenceReason { get; set; }
}

public sealed class CreateManualReconciliationCommandHandler : IRequestHandler<CreateManualReconciliationCommand, ReconciliationCaseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateManualReconciliationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<ReconciliationCaseDto> Handle(CreateManualReconciliationCommand request, CancellationToken cancellationToken)
    {
        if (request.BankLines.Count is < 1 or > 20 || request.LedgerEntries.Count is < 1 or > 20) throw new InvalidOperationException("Seleccioná entre 1 y 20 partidas de cada lado.");
        if (request.BankLines.Select(item => item.Id).Distinct().Count() != request.BankLines.Count || request.LedgerEntries.Select(item => item.Id).Distinct().Count() != request.LedgerEntries.Count)
            throw new InvalidOperationException("La selección contiene partidas repetidas.");
        if (request.BankLines.Any(item => item.AppliedAmount <= 0) || request.LedgerEntries.Any(item => item.AppliedAmount <= 0)) throw new InvalidOperationException("Los importes aplicados deben ser mayores a cero.");
        var organizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
        var lineIds = request.BankLines.Select(item => item.Id).ToList(); var entryIds = request.LedgerEntries.Select(item => item.Id).ToList();
        var lines = await _context.BankStatementLines.Include(item => item.BankStatement).Where(item => lineIds.Contains(item.Id) && item.BankStatement.BankAccountId == request.BankAccountId).ToListAsync(cancellationToken);
        var entries = await _context.LedgerEntries.Include(item => item.Client).Where(item => entryIds.Contains(item.Id) && item.BankAccountId == request.BankAccountId).ToListAsync(cancellationToken);
        if (lines.Count != lineIds.Count || entries.Count != entryIds.Count) throw new KeyNotFoundException("Una o más partidas no existen en la cuenta seleccionada.");
        var lineMap = request.BankLines.ToDictionary(item => item.Id); var entryMap = request.LedgerEntries.ToDictionary(item => item.Id);
        foreach (var line in lines) if (lineMap[line.Id].AppliedAmount > Math.Abs(line.Amount)) throw new InvalidOperationException("El importe aplicado supera una línea bancaria.");
        foreach (var entry in entries) if (entryMap[entry.Id].AppliedAmount > Math.Abs(ReconciliationRules.SignedAmount(entry))) throw new InvalidOperationException("El importe aplicado supera un movimiento contable.");
        var bankAmount = lines.Sum(item => item.Amount < 0 ? -lineMap[item.Id].AppliedAmount : lineMap[item.Id].AppliedAmount);
        var ledgerAmount = entries.Sum(item => ReconciliationRules.SignedAmount(item) < 0 ? -entryMap[item.Id].AppliedAmount : entryMap[item.Id].AppliedAmount);
        if (Math.Sign(bankAmount) != Math.Sign(ledgerAmount)) throw new InvalidOperationException("Las partidas bancarias y contables tienen sentidos opuestos.");
        var settings = await _context.ReconciliationSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken) ?? ReconciliationSettingsFeature.New(organizationId);
        var difference = bankAmount - ledgerAmount;
        ReconciliationRules.ValidateDifference(difference, settings.AmountTolerance, request.DifferenceType, request.DifferenceReason);
        var matchType = lines.Count > 1 ? ReconciliationMatchType.ManyToOne : entries.Count > 1 ? ReconciliationMatchType.OneToMany :
            Math.Abs(difference) <= settings.AmountTolerance && lineMap[lines[0].Id].AppliedAmount == Math.Abs(lines[0].Amount) && entryMap[entries[0].Id].AppliedAmount == Math.Abs(ReconciliationRules.SignedAmount(entries[0])) ? ReconciliationMatchType.Suggested : ReconciliationMatchType.Partial;
        var item = new ReconciliationCase
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, BankAccountId = request.BankAccountId, Status = ReconciliationStatus.Suggested,
            MatchType = matchType, Score = 0, ExplanationJson = JsonSerializer.Serialize(new[] { new ReconciliationFactor("manual", "Selección manual", 0, "Partidas elegidas por una persona") }),
            BankAmount = bankAmount, LedgerAmount = ledgerAmount, DifferenceAmount = difference, DifferenceType = request.DifferenceType,
            DifferenceReason = request.DifferenceReason?.Trim(), IsAutomatic = false, GeneratedAt = DateTime.UtcNow
        };
        foreach (var line in lines) item.BankLines.Add(new ReconciliationCaseBankLine { Id = Guid.NewGuid(), BankStatementLineId = line.Id, BankStatementLine = line, AppliedAmount = lineMap[line.Id].AppliedAmount });
        foreach (var entry in entries) item.LedgerEntries.Add(new ReconciliationCaseLedgerEntry { Id = Guid.NewGuid(), LedgerEntryId = entry.Id, LedgerEntry = entry, AppliedAmount = entryMap[entry.Id].AppliedAmount });
        _context.ReconciliationCases.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        var account = await _context.BankAccounts.AsNoTracking().SingleAsync(account => account.Id == request.BankAccountId, cancellationToken); item.BankAccount = account;
        return ReconciliationCaseFeature.Map(item);
    }
}
