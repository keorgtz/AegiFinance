using FluentValidation;

namespace AegiFinance.Application.Features.Services.Commands.AddServicePriceHistory;

public class AddServicePriceHistoryCommandValidator : AbstractValidator<AddServicePriceHistoryCommand>
{
    public AddServicePriceHistoryCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("El servicio es obligatorio.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("El precio debe ser mayor o igual a cero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(3).WithMessage("La moneda no puede tener más de 3 caracteres.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("La fecha de vigencia es obligatoria.");
    }
}
