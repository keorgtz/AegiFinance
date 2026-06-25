using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;

public class UpdateExchangeRateCommand : IRequest<ExchangeRateDto>
{
    public Guid Id { get; set; }
    public decimal RateToMXN { get; set; }
    public decimal RateFromMXN { get; set; }
    public DateTime EffectiveDate { get; set; }
}
