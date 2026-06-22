using FluentValidation;

namespace AegiFinance.Application.Features.Ledger.Commands.RegisterExpense;

public class RegisterExpenseCommandValidator : AbstractValidator<RegisterExpenseCommand>
{
    public RegisterExpenseCommandValidator()
    {
        RuleFor(x => x.BankAccountId)
            .NotEmpty().WithMessage("La cuenta bancaria es obligatoria.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(3).WithMessage("La moneda no puede exceder 3 caracteres.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");

        RuleFor(x => x.Reference)
            .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Reference))
            .WithMessage("La referencia no puede exceder 200 caracteres.");
    }
}
