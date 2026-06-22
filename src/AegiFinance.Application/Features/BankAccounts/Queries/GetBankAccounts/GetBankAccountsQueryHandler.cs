using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccounts;

public class GetBankAccountsQueryHandler : IRequestHandler<GetBankAccountsQuery, PaginatedList<BankAccountListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBankAccountsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<BankAccountListDto>> Handle(GetBankAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BankAccounts
            .AsNoTracking()
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(ba => ba.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(ba =>
                EF.Functions.Like(ba.Name.ToLower(), $"%{search}%") ||
                (ba.BankName != null && EF.Functions.Like(ba.BankName.ToLower(), $"%{search}%")));
        }

        query = query.OrderBy(ba => ba.Name);

        var projected = query.Select(ba => new BankAccountListDto
        {
            Id = ba.Id,
            Name = ba.Name,
            BankName = ba.BankName,
            Currency = ba.Currency,
            OpeningBalance = ba.OpeningBalance,
            IsActive = ba.IsActive
        });

        return await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
