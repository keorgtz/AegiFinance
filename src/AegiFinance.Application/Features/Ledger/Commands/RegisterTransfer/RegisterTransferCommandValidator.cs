using FluentValidation;

namespace AegiFinance.Application.Features.Ledger.Commands.RegisterTransfer;

public class RegisterTransferCommandValidator : AbstractValidator<RegisterTransferCommand>
{
    public RegisterTransferCommandValidator()
    {
        RuleFor(x => x.FromBankAccountId)
            .NotEmpty().WithMessage("La cuenta de origen es obligatoria.");

        RuleFor(x => x.ToBankAccountId)
            .NotEmpty().WithMessage("La cuenta de destino es obligatoria.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(3).WithMessage("La moneda no puede exceder 3 caracteres.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}
