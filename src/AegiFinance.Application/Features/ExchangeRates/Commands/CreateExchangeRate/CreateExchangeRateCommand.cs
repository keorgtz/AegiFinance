using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommand : IRequest<ExchangeRateDto>
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal RateToMXN { get; set; }
    public decimal RateFromMXN { get; set; }
    public DateTime EffectiveDate { get; set; }
}
