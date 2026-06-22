using FluentValidation;

namespace AegiFinance.Application.Features.Allocations.Commands.ManualAllocate;

public class ManualAllocateCommandValidator : AbstractValidator<ManualAllocateCommand>
{
    public ManualAllocateCommandValidator()
    {
        RuleFor(x => x.LedgerEntryId).NotEmpty();
        RuleFor(x => x.Allocations).NotEmpty();
        RuleForEach(x => x.Allocations).ChildRules(child =>
        {
            child.RuleFor(x => x.BillingItemId).NotEmpty();
            child.RuleFor(x => x.Amount).GreaterThan(0);
        });
        RuleFor(x => x.Allocations)
            .Must(x => x.Select(a => a.BillingItemId).Distinct().Count() == x.Count)
            .WithMessage("No se pueden repetir BillingItemId en la misma asignación manual.");
    }
}
