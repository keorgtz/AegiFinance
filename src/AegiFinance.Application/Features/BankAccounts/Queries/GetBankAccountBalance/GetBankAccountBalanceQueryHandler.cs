using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountBalance;

public class GetBankAccountBalanceQueryHandler : IRequestHandler<GetBankAccountBalanceQuery, decimal>
{
    private readonly IAccountBalanceCalculator _balanceCalculator;

    public GetBankAccountBalanceQueryHandler(IAccountBalanceCalculator balanceCalculator)
    {
        _balanceCalculator = balanceCalculator;
    }

    public async Task<decimal> Handle(GetBankAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        return await _balanceCalculator.CalculateBalanceAsync(request.BankAccountId, request.AsOfDate, cancellationToken);
    }
}
