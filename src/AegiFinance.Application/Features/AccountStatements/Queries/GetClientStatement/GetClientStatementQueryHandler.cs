using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AccountStatements.Common;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.AccountStatements.Queries.GetClientStatement;

public sealed class GetClientStatementQueryHandler : IRequestHandler<GetClientStatementQuery, AccountStatementDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetClientStatementQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AccountStatementDto> Handle(GetClientStatementQuery request, CancellationToken cancellationToken)
    {
        AccountStatementRules.ValidatePeriod(request.From, request.To);
        var currency = NormalizeCurrency(request.Currency);
        if (_currentUser.IsClientUser() && _currentUser.ClientId != request.ClientId)
            throw new UnauthorizedAccessException("No tiene permiso para consultar el estado de cuenta de otro cliente.");

        var client = await _context.Clients.AsNoTracking().Include(item => item.Organization).Include(item => item.AccountManagerUser)
            .SingleOrDefaultAsync(item => item.Id == request.ClientId, cancellationToken)
            ?? throw new KeyNotFoundException("El cliente no existe.");
        var scope = await ResolveSubscriptionScopeAsync(request.ClientId, request.SubscriptionId, cancellationToken);
        var movements = scope.IsSubledger
            ? await LoadSubscriptionSubledgerAsync(request.ClientId, currency, scope.SubscriptionIds, cancellationToken)
            : await LoadMajorLedgerAsync(request.ClientId, currency, cancellationToken);

        var from = request.From?.Date;
        var to = request.To?.Date;
        var opening = movements.Where(item => from.HasValue && item.Date.Date < from.Value).Sum(item => item.Debit - item.Credit);
        var filtered = movements.Where(item => (!from.HasValue || item.Date.Date >= from.Value) && (!to.HasValue || item.Date.Date <= to.Value))
            .OrderBy(item => item.Date).ThenBy(item => item.Id).ToList();
        var overdue = await CalculateOverdueAsync(request.ClientId, currency, scope.SubscriptionIds, to ?? DateTime.UtcNow.Date, cancellationToken);
        var running = opening;
        var items = filtered.Select(item =>
        {
            running += item.Debit - item.Credit;
            return new AccountStatementItemDto
            {
                Id = item.Id, JournalEntryId = item.JournalEntryId, JournalEntryNumber = item.JournalEntryNumber,
                Date = item.Date, Type = item.Type, SourceType = item.SourceType, Description = item.Description,
                Reference = item.Reference, ReferenceId = item.ReferenceId, SubscriptionId = item.SubscriptionId,
                SubscriptionCode = item.SubscriptionCode, ServiceName = item.ServiceName, DueDate = item.DueDate,
                IsOverdue = item.Category == "Charge" && item.Debit > item.Credit && overdue.BillingItemIds.Contains(item.ReferenceId),
                Debit = item.Debit, Credit = item.Credit, Balance = running, Currency = currency
            };
        }).ToList();

        var valueLines = filtered.Select(item => new AccountStatementValueLine(item.Date, item.Id, item.Debit, item.Credit)).ToList();
        var final = AccountStatementRules.ClosingBalance(opening, valueLines);
        AccountStatementRules.EnsureBalanced(opening, final, valueLines);
        var verification = AccountStatementRules.VerificationCode(client.Id, currency, from, to, opening, final, valueLines);
        var selected = request.SubscriptionId.HasValue
            ? await _context.Subscriptions.AsNoTracking().Include(item => item.Service).SingleAsync(item => item.Id == request.SubscriptionId, cancellationToken)
            : null;

        return new AccountStatementDto
        {
            ClientId = client.Id, ClientName = client.Name, OrganizationName = client.Organization.Name,
            StatementDate = DateTime.UtcNow, StartDate = from, EndDate = to, DisplayCurrency = currency,
            SubscriptionId = selected?.Id, SubscriptionCode = selected?.Code, ServiceName = selected?.Service.Name,
            InitialBalance = opening,
            TotalCharges = filtered.Where(item => item.Category == "Charge").Sum(item => item.Debit - item.Credit),
            TotalPayments = filtered.Where(item => item.Category == "Payment").Sum(item => item.Credit - item.Debit),
            TotalAdjustments = filtered.Where(item => item.Category == "Adjustment").Sum(item => item.Debit - item.Credit),
            FinalBalance = final,
            OverdueBalance = overdue.Amount,
            IsSubledgerView = scope.IsSubledger,
            ScopeDescription = scope.IsSubledger ? "Subledger de suscripciones autorizadas; anticipos sin plan no se incluyen." : "Cuentas por cobrar del Major Ledger.",
            ContactName = client.AccountManagerUser?.Name, ContactEmail = client.AccountManagerUser?.Email,
            VerificationCode = verification, FormulaValid = true, Items = items
        };
    }

    private async Task<SubscriptionScope> ResolveSubscriptionScopeAsync(Guid clientId, Guid? requestedId, CancellationToken cancellationToken)
    {
        if (requestedId.HasValue && !await _context.Subscriptions.AsNoTracking().AnyAsync(item => item.Id == requestedId && item.ClientId == clientId, cancellationToken))
            throw new KeyNotFoundException("La suscripción no pertenece al cliente.");
        if (!_currentUser.IsClientUser() || !_currentUser.UserId.HasValue)
            return requestedId.HasValue ? new SubscriptionScope(true, [requestedId.Value]) : new SubscriptionScope(false, []);
        var allowed = await _context.SubscriptionPermissions.AsNoTracking().Where(item => item.UserId == _currentUser.UserId.Value)
            .Select(item => item.SubscriptionId).ToListAsync(cancellationToken);
        if (allowed.Count == 0)
            return requestedId.HasValue ? new SubscriptionScope(true, [requestedId.Value]) : new SubscriptionScope(false, []);
        if (requestedId.HasValue && !allowed.Contains(requestedId.Value))
            throw new UnauthorizedAccessException("No tiene permiso para consultar esta suscripción.");
        return new SubscriptionScope(true, requestedId.HasValue ? [requestedId.Value] : allowed);
    }

    private async Task<List<AccountStatementMovement>> LoadMajorLedgerAsync(Guid clientId, string currency, CancellationToken cancellationToken)
    {
        var rows = await _context.JournalLines.AsNoTracking()
            .Where(line => line.ClientId == clientId && line.Account.Purpose == GeneralLedgerAccountPurpose.AccountsReceivable &&
                line.JournalEntry.Status != JournalEntryStatus.Draft && line.JournalEntry.Currency == currency)
            .Select(line => new
            {
                line.Id, line.JournalEntryId, line.JournalEntry.EntryNumber, line.JournalEntry.Date, line.JournalEntry.Description,
                line.JournalEntry.Reference, line.JournalEntry.SourceType,
                ReversedSourceType = line.JournalEntry.ReversesJournalEntry != null ? line.JournalEntry.ReversesJournalEntry.SourceType : (JournalSourceType?)null,
                line.JournalEntry.SourceId, line.Debit, line.Credit, line.BillingItemId,
                SubscriptionId = line.BillingItem != null ? (Guid?)line.BillingItem.SubscriptionId : null,
                SubscriptionCode = line.BillingItem != null ? line.BillingItem.Subscription.Code : null,
                ServiceName = line.BillingItem != null ? line.BillingItem.Subscription.Service.Name : null,
                DueDate = line.BillingItem != null ? (DateTime?)line.BillingItem.DueDate : null
            }).ToListAsync(cancellationToken);
        return rows.Select(row =>
        {
            var effectiveSource = row.SourceType == JournalSourceType.Reversal ? row.ReversedSourceType ?? JournalSourceType.Adjustment : row.SourceType;
            return new AccountStatementMovement
            {
                Id = row.Id, JournalEntryId = row.JournalEntryId, JournalEntryNumber = row.EntryNumber, Date = row.Date,
                Type = row.SourceType == JournalSourceType.Reversal ? "Reversal" : TypeName(effectiveSource),
                Category = CategoryName(effectiveSource), SourceType = effectiveSource.ToString(), Description = row.Description,
                Reference = row.Reference, ReferenceId = row.BillingItemId ?? ParseId(row.SourceId) ?? row.JournalEntryId,
                SubscriptionId = row.SubscriptionId, SubscriptionCode = row.SubscriptionCode, ServiceName = row.ServiceName,
                DueDate = row.DueDate, Debit = row.Debit, Credit = row.Credit
            };
        }).ToList();
    }

    private async Task<List<AccountStatementMovement>> LoadSubscriptionSubledgerAsync(Guid clientId, string currency, IReadOnlyCollection<Guid> subscriptionIds, CancellationToken cancellationToken)
    {
        var journal = (await LoadMajorLedgerAsync(clientId, currency, cancellationToken))
            .Where(item => item.SubscriptionId.HasValue && subscriptionIds.Contains(item.SubscriptionId.Value) && item.Category != "Payment").ToList();
        var allocations = await _context.SubscriptionAllocations.AsNoTracking()
            .Where(item => subscriptionIds.Contains(item.BillingItem.SubscriptionId) && item.BillingItem.Currency == currency)
            .Select(item => new
            {
                item.Id, item.LedgerEntryId, item.AllocatedAt, item.Amount, item.IsReversed, item.ReversedAt,
                Description = item.LedgerEntry.Description, Reference = item.LedgerEntry.Reference,
                item.BillingItem.SubscriptionId, SubscriptionCode = item.BillingItem.Subscription.Code,
                ServiceName = item.BillingItem.Subscription.Service.Name, item.BillingItem.DueDate,
                JournalEntryId = item.PaymentApplication != null ? item.PaymentApplication.Payments.Where(payment => payment.LedgerEntryId == item.LedgerEntryId).Select(payment => (Guid?)payment.JournalEntryId).FirstOrDefault() : null,
                JournalEntryNumber = item.PaymentApplication != null ? item.PaymentApplication.Payments.Where(payment => payment.LedgerEntryId == item.LedgerEntryId).Select(payment => payment.JournalEntry.EntryNumber).FirstOrDefault() : null
            }).ToListAsync(cancellationToken);
        foreach (var allocation in allocations)
        {
            journal.Add(new AccountStatementMovement
            {
                Id = allocation.Id, JournalEntryId = allocation.JournalEntryId ?? Guid.Empty,
                JournalEntryNumber = allocation.JournalEntryNumber ?? "Asignación histórica", Date = allocation.AllocatedAt,
                Type = "Payment", Category = "Payment", SourceType = JournalSourceType.Payment.ToString(),
                Description = allocation.Description, Reference = allocation.Reference, ReferenceId = allocation.LedgerEntryId,
                SubscriptionId = allocation.SubscriptionId, SubscriptionCode = allocation.SubscriptionCode, ServiceName = allocation.ServiceName,
                DueDate = allocation.DueDate, Credit = allocation.Amount
            });
            if (allocation.IsReversed && allocation.ReversedAt.HasValue)
                journal.Add(new AccountStatementMovement
                {
                    Id = ReversalId(allocation.Id), JournalEntryId = allocation.JournalEntryId ?? Guid.Empty,
                    JournalEntryNumber = allocation.JournalEntryNumber ?? "Asignación histórica", Date = allocation.ReversedAt.Value,
                    Type = "Reversal", Category = "Payment", SourceType = JournalSourceType.Payment.ToString(),
                    Description = $"Reversión · {allocation.Description}", Reference = allocation.Reference, ReferenceId = allocation.LedgerEntryId,
                    SubscriptionId = allocation.SubscriptionId, SubscriptionCode = allocation.SubscriptionCode, ServiceName = allocation.ServiceName,
                    DueDate = allocation.DueDate, Debit = allocation.Amount
                });
        }
        return journal;
    }

    private async Task<OverdueResult> CalculateOverdueAsync(Guid clientId, string currency, IReadOnlyCollection<Guid> subscriptionIds, DateTime cutoff, CancellationToken cancellationToken)
    {
        var query = _context.BillingItems.AsNoTracking()
            .Where(item => item.ClientId == clientId && item.Currency == currency && item.GeneratedAt.Date <= cutoff.Date && item.DueDate.Date < cutoff.Date);
        if (subscriptionIds.Count > 0) query = query.Where(item => subscriptionIds.Contains(item.SubscriptionId));
        var ids = await query.Select(item => item.Id).ToListAsync(cancellationToken);
        if (ids.Count == 0) return new OverdueResult(0, new HashSet<Guid>());
        var charges = await _context.BillingItems.AsNoTracking().Include(item => item.Adjustments).Where(item => ids.Contains(item.Id)).ToListAsync(cancellationToken);
        var paid = await _context.SubscriptionAllocations.AsNoTracking()
            .Where(item => ids.Contains(item.BillingItemId) && item.AllocatedAt.Date <= cutoff.Date && (!item.IsReversed || item.ReversedAt > cutoff))
            .GroupBy(item => item.BillingItemId).Select(group => new { Id = group.Key, Amount = group.Sum(item => item.Amount) })
            .ToDictionaryAsync(item => item.Id, item => item.Amount, cancellationToken);
        var balances = charges.Select(item =>
        {
            if (item.Status == BillingItemStatus.Cancelled && item.UpdatedAt.Date <= cutoff.Date) return new { item.Id, Balance = 0m };
            var lateFees = item.Adjustments.Where(adjustment => adjustment.Type == BillingAdjustmentType.LateFee && adjustment.EffectiveDate.Date <= cutoff.Date && (!adjustment.ReversedAt.HasValue || adjustment.ReversedAt.Value.Date > cutoff.Date)).Sum(adjustment => adjustment.Amount);
            var credits = item.Adjustments.Where(adjustment => adjustment.Type == BillingAdjustmentType.CreditNote && adjustment.EffectiveDate.Date <= cutoff.Date && (!adjustment.ReversedAt.HasValue || adjustment.ReversedAt.Value.Date > cutoff.Date)).Sum(adjustment => adjustment.Amount);
            return new { item.Id, Balance = Math.Max(0, item.Amount + lateFees - credits - paid.GetValueOrDefault(item.Id)) };
        }).Where(item => item.Balance > 0).ToList();
        return new OverdueResult(balances.Sum(item => item.Balance), balances.Select(item => item.Id).ToHashSet());
    }

    private static string NormalizeCurrency(string value)
    {
        var currency = value.Trim().ToUpperInvariant();
        if (currency.Length != 3 || !currency.All(char.IsLetter)) throw new InvalidOperationException("La moneda debe ser un código ISO de tres letras.");
        return currency;
    }
    private static string TypeName(JournalSourceType source) => source switch { JournalSourceType.Charge => "Charge", JournalSourceType.Payment => "Payment", _ => "Adjustment" };
    private static string CategoryName(JournalSourceType source) => source switch { JournalSourceType.Charge => "Charge", JournalSourceType.Payment => "Payment", _ => "Adjustment" };
    private static Guid? ParseId(string? value) => Guid.TryParse(value, out var id) ? id : null;
    private static Guid ReversalId(Guid value) { var bytes = value.ToByteArray(); bytes[^1] ^= 0xFF; return new Guid(bytes); }
    private sealed record SubscriptionScope(bool IsSubledger, IReadOnlyCollection<Guid> SubscriptionIds);
    private sealed record OverdueResult(decimal Amount, IReadOnlySet<Guid> BillingItemIds);
}
