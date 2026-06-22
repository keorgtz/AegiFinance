using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Commands.DeleteBankAccount;

public class DeleteBankAccountCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteBankAccountCommand() { }

    public DeleteBankAccountCommand(Guid id)
    {
        Id = id;
    }
}
