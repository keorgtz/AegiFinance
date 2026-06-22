using FluentValidation;

namespace AegiFinance.Application.Features.BankAccounts.Commands.CreateBankAccount;

public class CreateBankAccountCommandValidator : AbstractValidator<CreateBankAccountCommand>
{
    public CreateBankAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la cuenta es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(3).WithMessage("La moneda no puede exceder 3 caracteres.");

        RuleFor(x => x.OpeningDate)
            .NotEmpty().WithMessage("La fecha de apertura es obligatoria.");
    }
}
