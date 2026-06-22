using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.GenerateBillingForSubscription;

public class GenerateBillingForSubscriptionCommandValidator : AbstractValidator<GenerateBillingForSubscriptionCommand>
{
    public GenerateBillingForSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("La suscripción es obligatoria.");
    }
}
