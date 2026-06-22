using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Currencies.Commands.CreateCurrencyConfig;

public class CreateCurrencyConfigCommand : IRequest<CurrencyConfigDto>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}
