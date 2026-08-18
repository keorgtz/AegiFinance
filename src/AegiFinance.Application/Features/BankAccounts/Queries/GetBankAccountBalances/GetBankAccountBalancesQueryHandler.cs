using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountBalances;

public sealed class GetBankAccountBalancesQueryHandler
    : IRequestHandler<GetBankAccountBalancesQuery, BankAccountBalancesDto>
{
    private readonly IAccountBalanceCalculator _calculator;
    private readonly ICurrentUserService _currentUser;

    public GetBankAccountBalancesQueryHandler(IAccountBalanceCalculator calculator, ICurrentUserService currentUser)
    {
        _calculator = calculator;
        _currentUser = currentUser;
    }

    public Task<BankAccountBalancesDto> Handle(GetBankAccountBalancesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser())
            throw new UnauthorizedAccessException("No tiene permiso para consultar saldos bancarios.");

        return _calculator.CalculateBalancesAsync(request.BankAccountId, request.AsOfDate, cancellationToken);
    }
}
