using FluentValidation;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;

public class UpdateExchangeRateCommandValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    public UpdateExchangeRateCommandValidator()
    {
        RuleFor(x => x.RateToMXN)
            .GreaterThan(0).WithMessage("La tasa a MXN debe ser mayor a cero.");

        RuleFor(x => x.RateFromMXN)
            .GreaterThan(0).WithMessage("La tasa desde MXN debe ser mayor a cero.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("La fecha efectiva es obligatoria.");
    }
}
