using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountById;

public class GetBankAccountByIdQuery : IRequest<BankAccountDto>
{
    public Guid Id { get; set; }

    public GetBankAccountByIdQuery() { }

    public GetBankAccountByIdQuery(Guid id)
    {
        Id = id;
    }
}
