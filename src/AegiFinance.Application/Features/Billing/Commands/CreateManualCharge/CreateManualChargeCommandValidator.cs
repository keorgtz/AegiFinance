using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.CreateManualCharge;

public class CreateManualChargeCommandValidator : AbstractValidator<CreateManualChargeCommand>
{
    public CreateManualChargeCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ChargeDate).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.ChargeDate);
        RuleFor(x => x.PeriodStart).NotEmpty();
        RuleFor(x => x.PeriodEnd).GreaterThanOrEqualTo(x => x.PeriodStart);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(120);
    }
}
