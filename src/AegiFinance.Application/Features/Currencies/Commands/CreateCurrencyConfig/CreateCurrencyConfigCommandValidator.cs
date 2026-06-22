using FluentValidation;

namespace AegiFinance.Application.Features.Currencies.Commands.CreateCurrencyConfig;

public class CreateCurrencyConfigCommandValidator : AbstractValidator<CreateCurrencyConfigCommand>
{
    public CreateCurrencyConfigCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de moneda es obligatorio.")
            .Length(3).WithMessage("El código de moneda debe tener 3 caracteres.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la moneda es obligatorio.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("El símbolo de la moneda es obligatorio.");
    }
}
