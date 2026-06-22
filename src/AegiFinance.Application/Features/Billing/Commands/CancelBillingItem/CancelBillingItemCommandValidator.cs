using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.CancelBillingItem;

public class CancelBillingItemCommandValidator : AbstractValidator<CancelBillingItemCommand>
{
    public CancelBillingItemCommandValidator()
    {
        RuleFor(x => x.BillingItemId)
            .NotEmpty().WithMessage("El cargo es obligatorio.");
    }
}
