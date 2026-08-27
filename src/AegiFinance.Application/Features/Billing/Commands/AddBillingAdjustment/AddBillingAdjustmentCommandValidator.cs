using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.AddBillingAdjustment;

public class AddBillingAdjustmentCommandValidator : AbstractValidator<AddBillingAdjustmentCommand>
{
    public AddBillingAdjustmentCommandValidator()
    {
        RuleFor(x => x.BillingItemId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(120);
    }
}
