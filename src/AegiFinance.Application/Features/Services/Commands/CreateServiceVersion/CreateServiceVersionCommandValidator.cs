using AegiFinance.Domain.Enums;
using FluentValidation;

namespace AegiFinance.Application.Features.Services.Commands.CreateServiceVersion;

public sealed class CreateServiceVersionCommandValidator : AbstractValidator<CreateServiceVersionCommand>
{
    public CreateServiceVersionCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).Length(3);
        RuleFor(x => x.DefaultDiscountPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.DefaultTaxPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.CustomIntervalDays).NotNull().InclusiveBetween(1, 3660).When(x => x.BillingType == BillingType.Custom);
        RuleForEach(x => x.Concepts).ChildRules(concept =>
        {
            concept.RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            concept.RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            concept.RuleFor(x => x.Quantity).GreaterThan(0);
            concept.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            concept.RuleFor(x => x.TaxPercent).InclusiveBetween(0, 100);
        });
    }
}
