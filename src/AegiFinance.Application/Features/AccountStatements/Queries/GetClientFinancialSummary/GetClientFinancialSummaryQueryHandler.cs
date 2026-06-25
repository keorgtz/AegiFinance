using AegiFinance.Application.Common.Extensions;
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
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar el resumen financiero del cliente.");
        }

        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ClientId, cancellationToken)
            ?? throw new InvalidOperationException("El cliente no existe.");

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

        decimal rate = 1m;
        decimal? rateUsed = null;
        if (!string.Equals(request.Currency, "MXN", StringComparison.OrdinalIgnoreCase))
        {
            var rateResult = await _currencyConverter.GetRateAsync(request.Currency, to ?? DateTime.UtcNow, cancellationToken);
            rate = rateResult.Rate;
            rateUsed = rate;
        }

        return new FinancialSummaryDto
        {
            ClientId = client.Id,
            ClientName = client.Name,
            DisplayCurrency = request.Currency,
            ExchangeRateUsed = rateUsed,
            TotalCharges = charges * rate,
            TotalPayments = payments * rate,
            TotalAdjustments = adjustments * rate,
            CurrentBalance = currentBalanceMxN * rate
        };
    }
}
