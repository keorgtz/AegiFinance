using FluentValidation;

namespace AegiFinance.Application.Features.Currencies.Commands.UpdateCurrencyConfig;

public class UpdateCurrencyConfigCommandValidator : AbstractValidator<UpdateCurrencyConfigCommand>
{
    public UpdateCurrencyConfigCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la moneda es obligatorio.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("El símbolo de la moneda es obligatorio.");
    }
}
