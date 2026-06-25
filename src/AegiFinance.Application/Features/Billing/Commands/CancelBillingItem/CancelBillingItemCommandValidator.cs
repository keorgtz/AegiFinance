using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.CancelBillingItem;

public class CancelBillingItemCommandValidator : AbstractValidator<CancelBillingItemCommand>
{
    public CancelBillingItemCommandValidator()
    {
        RuleFor(x => x.BillingItemId)
            .NotEmpty().WithMessage("El cargo es obligatorio.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo de cancelación es obligatorio.")
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres.");
    }
}
