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
            .NotEmpty().WithMessage("El nombre de la moneda es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de la moneda no puede exceder 100 caracteres.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("El símbolo de la moneda es obligatorio.")
            .MaximumLength(10).WithMessage("El símbolo de la moneda no puede exceder 10 caracteres.");
    }
}
