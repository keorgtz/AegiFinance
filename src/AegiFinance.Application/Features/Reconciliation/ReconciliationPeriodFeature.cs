using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Reconciliation;

public sealed class GetReconciliationPeriodsQuery : IRequest<IReadOnlyList<ReconciliationPeriodDto>>
{
    public Guid? BankAccountId { get; set; }
}
public sealed class CloseReconciliationPeriodCommand : IRequest<ReconciliationPeriodDto>
{
    public Guid BankAccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReconciliationDifferenceType DifferenceType { get; set; }
    public string? Justification { get; set; }
}

public sealed class GetReconciliationPeriodsQueryHandler : IRequestHandler<GetReconciliationPeriodsQuery, IReadOnlyList<ReconciliationPeriodDto>>
{
    private readonly IApplicationDbContext _context;
    public GetReconciliationPeriodsQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<ReconciliationPeriodDto>> Handle(GetReconciliationPeriodsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ReconciliationPeriods.AsNoTracking().Include(item => item.BankAccount).AsQueryable();
        if (request.BankAccountId.HasValue) query = query.Where(item => item.BankAccountId == request.BankAccountId);
        return (await query.OrderByDescending(item => item.EndDate).Take(100).ToListAsync(cancellationToken)).Select(Map).ToList();
    }
    internal static ReconciliationPeriodDto Map(ReconciliationPeriod item) => new()
    {
        Id = item.Id, BankAccountId = item.BankAccountId, BankAccountName = item.BankAccount.Name, StartDate = item.StartDate,
        EndDate = item.EndDate, Currency = item.Currency, BankAmount = item.BankAmount, LedgerAmount = item.LedgerAmount,
        DifferenceAmount = item.DifferenceAmount, DifferenceType = item.DifferenceType, Justification = item.Justification,
        Status = item.Status, ClosedAt = item.ClosedAt
    };
}

public sealed class CloseReconciliationPeriodCommandHandler : IRequestHandler<CloseReconciliationPeriodCommand, ReconciliationPeriodDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CloseReconciliationPeriodCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<ReconciliationPeriodDto> Handle(CloseReconciliationPeriodCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
        var start = request.StartDate.Date; var end = request.EndDate.Date;
        if (start > end) throw new InvalidOperationException("El inicio del periodo no puede ser posterior al cierre.");
        if ((end - start).TotalDays > 366) throw new InvalidOperationException("El periodo no puede exceder 366 días.");
        var account = await _context.BankAccounts.SingleOrDefaultAsync(item => item.Id == request.BankAccountId, cancellationToken) ?? throw new KeyNotFoundException("La cuenta bancaria no existe.");
        if (await _context.ReconciliationPeriods.AnyAsync(item => item.BankAccountId == account.Id && item.Status == ReconciliationPeriodStatus.Closed && item.StartDate <= end && item.EndDate >= start, cancellationToken))
            throw new InvalidOperationException("El periodo se superpone con un cierre existente.");
        var bankLines = await _context.BankStatementLines.AsNoTracking().Where(item => item.BankStatement.BankAccountId == account.Id && item.TransactionDate >= start && item.TransactionDate < end.AddDays(1)).ToListAsync(cancellationToken);
        var ledgerEntries = await _context.LedgerEntries.AsNoTracking().Where(item => item.BankAccountId == account.Id && item.Date >= start && item.Date < end.AddDays(1)).ToListAsync(cancellationToken);
        if (bankLines.Count == 0 && ledgerEntries.Count == 0)
            throw new InvalidOperationException("No se puede cerrar un periodo sin movimientos bancarios ni contables.");
        if (bankLines.Any(item => !item.IsReconciled) || ledgerEntries.Any(item => !item.IsReconciled))
            throw new InvalidOperationException("No se puede cerrar el periodo mientras existan líneas bancarias o movimientos contables sin conciliar.");
        var bankAmount = bankLines.Sum(item => item.Amount);
        var ledgerAmount = ledgerEntries.Sum(ReconciliationRules.SignedAmount);
        var difference = bankAmount - ledgerAmount;
        var settings = await _context.ReconciliationSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken) ?? ReconciliationSettingsFeature.New(organizationId);
        ReconciliationRules.ValidateDifference(difference, settings.AmountTolerance, request.DifferenceType, request.Justification);
        var period = new ReconciliationPeriod
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, BankAccountId = account.Id, BankAccount = account,
            StartDate = start, EndDate = end, Currency = account.Currency, BankAmount = bankAmount, LedgerAmount = ledgerAmount,
            DifferenceAmount = difference, DifferenceType = request.DifferenceType, Justification = request.Justification?.Trim(),
            Status = ReconciliationPeriodStatus.Closed, ClosedAt = DateTime.UtcNow, ClosedBy = _currentUser.UserId
        };
        _context.ReconciliationPeriods.Add(period);
        await _context.SaveChangesAsync(cancellationToken);
        return GetReconciliationPeriodsQueryHandler.Map(period);
    }
}
