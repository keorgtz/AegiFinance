using FluentValidation;

namespace AegiFinance.Application.Features.Currencies.Commands.UpdateCurrencyConfig;

public class UpdateCurrencyConfigCommandValidator : AbstractValidator<UpdateCurrencyConfigCommand>
{
    public UpdateCurrencyConfigCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la moneda es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de la moneda no puede exceder 100 caracteres.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("El símbolo de la moneda es obligatorio.")
            .MaximumLength(10).WithMessage("El símbolo de la moneda no puede exceder 10 caracteres.");
    }
}
