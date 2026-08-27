using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.GenerateBilling;

public class GenerateBillingCommandValidator : AbstractValidator<GenerateBillingCommand>
{
    public GenerateBillingCommandValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThan(2000).WithMessage("El año debe ser mayor a 2000.");

        RuleFor(x => x.Month)
            .NotNull().WithMessage("El mes es obligatorio para evitar periodos superpuestos.")
            .InclusiveBetween(1, 12)
            .WithMessage("El mes debe estar entre 1 y 12.");
    }
}
