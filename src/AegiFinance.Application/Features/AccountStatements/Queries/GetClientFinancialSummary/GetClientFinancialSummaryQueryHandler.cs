using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.AccountStatements.Queries.GetClientFinancialSummary;

public class GetClientFinancialSummaryQueryHandler : IRequestHandler<GetClientFinancialSummaryQuery, FinancialSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrencyConverter _currencyConverter;

    public GetClientFinancialSummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICurrencyConverter currencyConverter)
    {
        _context = context;
        _currentUserService = currentUserService;
        _currencyConverter = currencyConverter;
    }

    public async Task<FinancialSummaryDto> Handle(GetClientFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ClientId, cancellationToken)
            ?? throw new InvalidOperationException("El cliente no existe.");

        if (_currentUserService.UserType == UserType.Client.ToString() && _currentUserService.ClientId != request.ClientId)
            throw new InvalidOperationException("No puedes consultar el resumen de otro cliente.");

        var from = request.From?.Date;
        var to = request.To?.Date;

        var charges = await _context.BillingItems.AsNoTracking()
            .Where(x => x.ClientId == request.ClientId && (!from.HasValue || x.DueDate.Date >= from) && (!to.HasValue || x.DueDate.Date <= to))
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var payments = await _context.LedgerEntries.AsNoTracking()
            .Where(x => x.ClientId == request.ClientId && x.EntryType == LedgerEntryType.Income && (!from.HasValue || x.Date.Date >= from) && (!to.HasValue || x.Date.Date <= to))
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var adjustments = await _context.LedgerEntries.AsNoTracking()
            .Where(x => x.ClientId == request.ClientId && x.EntryType == LedgerEntryType.Adjustment && (!from.HasValue || x.Date.Date >= from) && (!to.HasValue || x.Date.Date <= to))
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var currentBalanceMxN = charges - payments - adjustments;
        decimal? rateUsed = null;
        if (!string.Equals(request.Currency, "MXN", StringComparison.OrdinalIgnoreCase))
            rateUsed = (await _currencyConverter.GetRateAsync(request.Currency, to ?? DateTime.UtcNow, cancellationToken)).Rate;

        return new FinancialSummaryDto
        {
            ClientId = client.Id,
            ClientName = client.Name,
            DisplayCurrency = request.Currency,
            ExchangeRateUsed = rateUsed,
            TotalCharges = await ConvertAsync(charges, request.Currency, to ?? DateTime.UtcNow, cancellationToken),
            TotalPayments = await ConvertAsync(payments, request.Currency, to ?? DateTime.UtcNow, cancellationToken),
            TotalAdjustments = await ConvertAsync(adjustments, request.Currency, to ?? DateTime.UtcNow, cancellationToken),
            CurrentBalance = await ConvertAsync(currentBalanceMxN, request.Currency, to ?? DateTime.UtcNow, cancellationToken)
        };
    }

    private async Task<decimal> ConvertAsync(decimal amount, string currency, DateTime date, CancellationToken cancellationToken)
    {
        if (string.Equals(currency, "MXN", StringComparison.OrdinalIgnoreCase)) return amount;
        return await _currencyConverter.ConvertAsync(amount, currency, date, cancellationToken);
    }
}
