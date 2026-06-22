using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountBalance;

public class GetBankAccountBalanceQuery : IRequest<decimal>
{
    public Guid BankAccountId { get; set; }
    public DateTime? AsOfDate { get; set; }

    public GetBankAccountBalanceQuery() { }

    public GetBankAccountBalanceQuery(Guid bankAccountId, DateTime? asOfDate = null)
    {
        BankAccountId = bankAccountId;
        AsOfDate = asOfDate;
    }
}
