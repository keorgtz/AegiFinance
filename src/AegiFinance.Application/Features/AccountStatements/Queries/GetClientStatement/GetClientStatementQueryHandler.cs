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
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ClientId, cancellationToken)
            ?? throw new InvalidOperationException("El cliente no existe.");

        EnsureClientAccess(request.ClientId);

        var from = request.From?.Date;
        var to = request.To?.Date;
        var movements = await LoadMovementsAsync(request.ClientId, cancellationToken);
        var initialBalance = movements.Where(x => from.HasValue && x.Date.Date < from.Value).Sum(x => x.DebitMXN - x.CreditMXN);
        var filtered = movements
            .Where(x => (!from.HasValue || x.Date.Date >= from.Value) && (!to.HasValue || x.Date.Date <= to.Value))
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Type)
            .ToList();

        decimal? rateUsed = null;
        if (!string.Equals(request.Currency, "MXN", StringComparison.OrdinalIgnoreCase))
        {
            rateUsed = (await _currencyConverter.GetRateAsync(request.Currency, to ?? DateTime.UtcNow, cancellationToken)).Rate;
        }

        var runningBalanceMxN = initialBalance;
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
                Debit = await ConvertAsync(movement.DebitMXN, request.Currency, movement.Date, cancellationToken),
                Credit = await ConvertAsync(movement.CreditMXN, request.Currency, movement.Date, cancellationToken),
                Balance = await ConvertAsync(runningBalanceMxN, request.Currency, movement.Date, cancellationToken),
                OriginalAmountMXN = movement.DebitMXN > 0 ? movement.DebitMXN : movement.CreditMXN,
                ExchangeRateUsed = rateUsed
            });
        }

        var totalChargesMxN = filtered.Sum(x => x.DebitMXN);
        var totalPaymentsMxN = filtered.Where(x => x.Type == "Payment").Sum(x => x.CreditMXN);
        var totalAdjustmentsMxN = filtered.Where(x => x.Type == "Adjustment").Sum(x => x.CreditMXN);
        var finalBalanceMxN = initialBalance + totalChargesMxN - totalPaymentsMxN - totalAdjustmentsMxN;

        return new AccountStatementDto
        {
            ClientId = client.Id,
            ClientName = client.Name,
            StatementDate = DateTime.UtcNow,
            StartDate = from,
            EndDate = to,
            DisplayCurrency = request.Currency,
            ExchangeRateUsed = rateUsed,
            InitialBalance = await ConvertAsync(initialBalance, request.Currency, from ?? DateTime.UtcNow, cancellationToken),
            TotalCharges = await ConvertAsync(totalChargesMxN, request.Currency, to ?? DateTime.UtcNow, cancellationToken),
            TotalPayments = await ConvertAsync(totalPaymentsMxN, request.Currency, to ?? DateTime.UtcNow, cancellationToken),
            TotalAdjustments = await ConvertAsync(totalAdjustmentsMxN, request.Currency, to ?? DateTime.UtcNow, cancellationToken),
            FinalBalance = await ConvertAsync(finalBalanceMxN, request.Currency, to ?? DateTime.UtcNow, cancellationToken),
            Items = items
        };
    }

    private void EnsureClientAccess(Guid clientId)
    {
        if (_currentUserService.UserType == UserType.Client.ToString() && _currentUserService.ClientId != clientId)
            throw new InvalidOperationException("No puedes consultar el estado de cuenta de otro cliente.");
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

    private async Task<decimal> ConvertAsync(decimal amount, string currency, DateTime date, CancellationToken cancellationToken)
    {
        if (string.Equals(currency, "MXN", StringComparison.OrdinalIgnoreCase)) return amount;
        return await _currencyConverter.ConvertAsync(amount, currency, date, cancellationToken);
    }
}
