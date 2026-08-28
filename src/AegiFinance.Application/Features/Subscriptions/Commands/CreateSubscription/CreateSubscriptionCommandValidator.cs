using FluentValidation;

namespace AegiFinance.Application.Features.Subscriptions.Commands.CreateSubscription;

public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
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
        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("El descuento debe estar entre 0 y 100 por ciento.");
        RuleFor(x => x.TaxPercent)
            .InclusiveBetween(0, 100).WithMessage("El impuesto debe estar entre 0 y 100 por ciento.");
        RuleFor(x => x.CustomIntervalDays).NotNull().InclusiveBetween(1, 3660)
            .When(x => x.BillingType == Domain.Enums.BillingType.Custom)
            .WithMessage("Indica un intervalo personalizado de 1 a 3660 días.");
        RuleFor(x => x.AutoRenew)
            .Equal(false)
            .When(x => x.BillingType is Domain.Enums.BillingType.OneTime or Domain.Enums.BillingType.Hourly)
            .WithMessage("La renovación automática solo está disponible para facturación recurrente.");
        RuleFor(x => x.ContractTerms)
            .MaximumLength(4000).WithMessage("Las condiciones contractuales no pueden superar 4000 caracteres.");
    }
}
