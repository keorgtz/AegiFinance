using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Commands.RecordBankBalance;

public sealed class RecordBankBalanceCommand : IRequest<BankAccountBalancesDto>
{
    public Guid BankAccountId { get; set; }
    public DateTime AsOfDate { get; set; }
    public decimal Balance { get; set; }
}
