using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.CloseBillingCycle;

public class CloseBillingCycleCommandValidator : AbstractValidator<CloseBillingCycleCommand>
{
    public CloseBillingCycleCommandValidator()
    {
        RuleFor(x => x.BillingCycleId)
            .NotEmpty().WithMessage("El ciclo de facturación es obligatorio.");
    }
}
