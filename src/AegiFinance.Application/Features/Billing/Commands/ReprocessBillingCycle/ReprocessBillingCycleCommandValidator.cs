using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.ReprocessBillingCycle;

public class ReprocessBillingCycleCommandValidator : AbstractValidator<ReprocessBillingCycleCommand>
{
    public ReprocessBillingCycleCommandValidator()
    {
        RuleFor(x => x.BillingCycleId)
            .NotEmpty().WithMessage("El ciclo de facturación es obligatorio.");
    }
}
