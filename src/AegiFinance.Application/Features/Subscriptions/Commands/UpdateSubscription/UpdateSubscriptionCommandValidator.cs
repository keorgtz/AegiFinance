using FluentValidation;

namespace AegiFinance.Application.Features.Subscriptions.Commands.UpdateSubscription;

public class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("El cliente es obligatorio.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("El servicio es obligatorio.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("El precio debe ser mayor o igual a cero.");

        RuleFor(x => x.BillingDay)
            .InclusiveBetween(1, 28).WithMessage("El día de facturación debe estar entre 1 y 28.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("La fecha final no puede ser anterior a la fecha de inicio.");

        RuleFor(x => x.Currency)
            .NotEmpty().Length(3).WithMessage("La moneda debe ser un código ISO de 3 letras.");

        RuleFor(x => x.BillingType)
            .IsInEnum().WithMessage("El tipo de facturación no es válido.");
    }
}
