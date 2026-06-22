using FluentValidation;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.SyncExchangeRate;

public class SyncExchangeRateCommandValidator : AbstractValidator<SyncExchangeRateCommand>
{
    public SyncExchangeRateCommandValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("El código de moneda es obligatorio.")
            .Length(3).WithMessage("El código de moneda debe tener 3 caracteres.");
    }
}
