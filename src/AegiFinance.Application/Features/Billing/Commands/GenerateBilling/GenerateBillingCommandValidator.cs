using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.GenerateBilling;

public class GenerateBillingCommandValidator : AbstractValidator<GenerateBillingCommand>
{
    public GenerateBillingCommandValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThan(2000).WithMessage("El año debe ser mayor a 2000.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).When(x => x.Month.HasValue)
            .WithMessage("El mes debe estar entre 1 y 12.");
    }
}
