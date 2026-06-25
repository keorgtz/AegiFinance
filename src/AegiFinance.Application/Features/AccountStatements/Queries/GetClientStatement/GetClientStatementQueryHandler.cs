using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AccountStatements.Common;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.AccountStatements.Queries.GetClientStatement;

public class GetClientStatementQueryHandler : IRequestHandler<GetClientStatementQuery, AccountStatementDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrencyConverter _currencyConverter;

    public GetClientStatementQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICurrencyConverter currencyConverter)
    {
        _context = context;
        _currentUserService = currentUserService;
        _currencyConverter = currencyConverter;
    }

    public async Task<AccountStatementDto> Handle(GetClientStatementQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser() && _currentUserService.ClientId != request.ClientId)
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar el estado de cuenta de otro cliente.");
        }

        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ClientId, cancellationToken)
            ?? throw new InvalidOperationException("El cliente no existe.");

        var from = request.From?.Date;
        var to = request.To?.Date;

        decimal rate = 1m;
        decimal? rateUsed = null;
        if (!string.Equals(request.Currency, "MXN", StringComparison.OrdinalIgnoreCase))
        {
            var rateResult = await _currencyConverter.GetRateAsync(request.Currency, to ?? DateTime.UtcNow, cancellationToken);
            rate = rateResult.Rate;
            rateUsed = rate;
        }

        var movements = await LoadMovementsAsync(request.ClientId, cancellationToken);
        var initialBalanceMxN = movements.Where(x => from.HasValue && x.Date.Date < from.Value).Sum(x => x.DebitMXN - x.CreditMXN);
        var filtered = movements
            .Where(x => (!from.HasValue || x.Date.Date >= from.Value) && (!to.HasValue || x.Date.Date <= to.Value))
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Type)
            .ToList();

        var runningBalanceMxN = initialBalanceMxN;
        var items = new List<AccountStatementItemDto>();
        foreach (var movement in filtered)
        {
            runningBalanceMxN += movement.DebitMXN - movement.CreditMXN;
            items.Add(new AccountStatementItemDto
            {
                Date = movement.Date,
                Type = movement.Type,
                Description = movement.Description,
                ReferenceId = movement.ReferenceId,
                Debit = movement.DebitMXN * rate,
                Credit = movement.CreditMXN * rate,
                Balance = runningBalanceMxN * rate,
                OriginalAmountMXN = movement.DebitMXN > 0 ? movement.DebitMXN : movement.CreditMXN,
                ExchangeRateUsed = rateUsed
            });
        }

        var totalChargesMxN = filtered.Sum(x => x.DebitMXN);
        var totalPaymentsMxN = filtered.Where(x => x.Type == "Payment").Sum(x => x.CreditMXN);
        var totalAdjustmentsMxN = filtered.Where(x => x.Type == "Adjustment").Sum(x => x.CreditMXN);
        var finalBalanceMxN = initialBalanceMxN + totalChargesMxN - totalPaymentsMxN - totalAdjustmentsMxN;

        return new AccountStatementDto
        {
            ClientId = client.Id,
            ClientName = client.Name,
            StatementDate = DateTime.UtcNow,
            StartDate = from,
            EndDate = to,
            DisplayCurrency = request.Currency,
            ExchangeRateUsed = rateUsed,
            InitialBalance = initialBalanceMxN * rate,
            TotalCharges = totalChargesMxN * rate,
            TotalPayments = totalPaymentsMxN * rate,
            TotalAdjustments = totalAdjustmentsMxN * rate,
            FinalBalance = finalBalanceMxN * rate,
            Items = items
        };
    }

    private async Task<List<AccountStatementMovement>> LoadMovementsAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var charges = await _context.BillingItems.AsNoTracking()
            .Where(x => x.ClientId == clientId)
            .Select(x => new AccountStatementMovement
            {
                Date = x.DueDate,
                Type = "Charge",
                Description = x.Description,
                ReferenceId = x.Id,
                DebitMXN = x.Amount,
                CreditMXN = 0m
            }).ToListAsync(cancellationToken);

        var credits = await _context.LedgerEntries.AsNoTracking()
            .Where(x => x.ClientId == clientId && (x.EntryType == LedgerEntryType.Income || x.EntryType == LedgerEntryType.Adjustment))
            .Select(x => new AccountStatementMovement
            {
                Date = x.Date,
                Type = x.EntryType == LedgerEntryType.Income ? "Payment" : "Adjustment",
                Description = x.Description,
                ReferenceId = x.Id,
                DebitMXN = 0m,
                CreditMXN = x.Amount
            }).ToListAsync(cancellationToken);

        return charges.Concat(credits).ToList();
    }
}
