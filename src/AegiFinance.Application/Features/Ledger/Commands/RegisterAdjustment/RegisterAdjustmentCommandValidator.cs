using FluentValidation;

namespace AegiFinance.Application.Features.Ledger.Commands.RegisterAdjustment;

public class RegisterAdjustmentCommandValidator : AbstractValidator<RegisterAdjustmentCommand>
{
    public RegisterAdjustmentCommandValidator()
    {
        RuleFor(x => x.BankAccountId)
            .NotEmpty().WithMessage("La cuenta bancaria es obligatoria.");

        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("El monto no puede ser cero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(3).WithMessage("La moneda no puede exceder 3 caracteres.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo del ajuste es obligatorio.")
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres.");
    }
}
