using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccounts;

public class GetBankAccountsQuery : IRequest<PaginatedList<BankAccountListDto>>
{
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
