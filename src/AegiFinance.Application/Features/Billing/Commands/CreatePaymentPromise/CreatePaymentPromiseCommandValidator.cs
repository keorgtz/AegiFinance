using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.CreatePaymentPromise;

public class CreatePaymentPromiseCommandValidator : AbstractValidator<CreatePaymentPromiseCommand>
{
    public CreatePaymentPromiseCommandValidator()
    {
        RuleFor(x => x.BillingItemId).NotEmpty();
        RuleFor(x => x.PromisedAmount).GreaterThan(0);
        RuleFor(x => x.PromiseDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
