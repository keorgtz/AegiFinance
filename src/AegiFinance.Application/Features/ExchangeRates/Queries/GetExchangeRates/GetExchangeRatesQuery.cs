using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ExchangeRates.Queries.GetExchangeRates;

public class GetExchangeRatesQuery : IRequest<List<ExchangeRateDto>>
{
    public string? CurrencyCode { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
