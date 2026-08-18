using FluentValidation;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ChangeSubscriptionPlan;

public sealed class ChangeSubscriptionPlanCommandValidator : AbstractValidator<ChangeSubscriptionPlanCommand>
{
    public ChangeSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ServiceVersionId).NotEmpty();
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0).When(x => x.BasePrice.HasValue);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100).When(x => x.DiscountPercent.HasValue);
        RuleFor(x => x.TaxPercent).InclusiveBetween(0, 100).When(x => x.TaxPercent.HasValue);
        RuleFor(x => x.BillingDay).InclusiveBetween(1, 31).When(x => x.BillingDay.HasValue);
        RuleFor(x => x.Terms).MaximumLength(4000);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
