using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Currencies.Commands.UpdateCurrencyConfig;

public class UpdateCurrencyConfigCommand : IRequest<CurrencyConfigDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}
