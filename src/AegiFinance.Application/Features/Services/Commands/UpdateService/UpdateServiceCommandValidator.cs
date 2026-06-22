using FluentValidation;

namespace AegiFinance.Application.Features.Services.Commands.UpdateService;

public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del servicio es obligatorio.");

        RuleFor(x => x.DefaultPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio debe ser mayor o igual a cero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(3).WithMessage("La moneda no puede tener más de 3 caracteres.");
    }
}
