using FluentValidation;

namespace AegiFinance.Application.Features.BankAccounts.Commands.UpdateBankAccount;

public class UpdateBankAccountCommandValidator : AbstractValidator<UpdateBankAccountCommand>
{
    public UpdateBankAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador de la cuenta es obligatorio.");

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
