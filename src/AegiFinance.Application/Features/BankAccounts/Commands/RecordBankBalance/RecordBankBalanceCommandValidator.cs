using FluentValidation;

namespace AegiFinance.Application.Features.BankAccounts.Commands.RecordBankBalance;

public sealed class RecordBankBalanceCommandValidator : AbstractValidator<RecordBankBalanceCommand>
{
    public RecordBankBalanceCommandValidator()
    {
        RuleFor(command => command.BankAccountId).NotEmpty();
        RuleFor(command => command.AsOfDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("La fecha del saldo bancario no puede estar en el futuro.");
    }
}
