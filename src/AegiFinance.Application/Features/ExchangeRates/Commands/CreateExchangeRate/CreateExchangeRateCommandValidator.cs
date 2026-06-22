using FluentValidation;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommandValidator : AbstractValidator<CreateExchangeRateCommand>
{
    public CreateExchangeRateCommandValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("El código de moneda es obligatorio.")
            .Length(3).WithMessage("El código de moneda debe tener 3 caracteres.");

        RuleFor(x => x.RateToMXN)
            .GreaterThan(0).WithMessage("La tasa a MXN debe ser mayor a cero.");

        RuleFor(x => x.RateFromMXN)
            .GreaterThan(0).WithMessage("La tasa desde MXN debe ser mayor a cero.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("La fecha efectiva es obligatoria.");
    }
}
