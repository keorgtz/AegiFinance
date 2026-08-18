using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountBalances;

public sealed record GetBankAccountBalancesQuery(Guid BankAccountId, DateTime? AsOfDate) : IRequest<BankAccountBalancesDto>;
