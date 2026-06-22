using FluentValidation;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ChangeSubscriptionPrice;

public class ChangeSubscriptionPriceCommandValidator : AbstractValidator<ChangeSubscriptionPriceCommand>
{
    public ChangeSubscriptionPriceCommandValidator()
    {
        RuleFor(x => x.NewPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El nuevo precio debe ser mayor o igual a cero.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("La fecha efectiva es obligatoria.");
    }
}
