using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountBalance;

public class GetBankAccountBalanceQueryHandler : IRequestHandler<GetBankAccountBalanceQuery, decimal>
{
    private readonly IAccountBalanceCalculator _balanceCalculator;
    private readonly ICurrentUserService _currentUserService;

    public GetBankAccountBalanceQueryHandler(IAccountBalanceCalculator balanceCalculator, ICurrentUserService currentUserService)
    {
        _balanceCalculator = balanceCalculator;
        _currentUserService = currentUserService;
    }

    public async Task<decimal> Handle(GetBankAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar el saldo de cuentas bancarias.");
        }

        return await _balanceCalculator.CalculateBalanceAsync(request.BankAccountId, request.AsOfDate, cancellationToken);
    }
}
