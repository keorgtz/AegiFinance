using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.SyncExchangeRate;

public class SyncExchangeRateCommand : IRequest<ExchangeRateDto?>
{
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
}
