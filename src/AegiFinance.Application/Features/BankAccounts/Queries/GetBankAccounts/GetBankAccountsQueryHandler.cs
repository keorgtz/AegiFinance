using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccounts;

public class GetBankAccountsQueryHandler : IRequestHandler<GetBankAccountsQuery, PaginatedList<BankAccountListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountNumberProtector _accountNumberProtector;

    public GetBankAccountsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IAccountNumberProtector accountNumberProtector)
    {
        _context = context;
        _currentUserService = currentUserService;
        _accountNumberProtector = accountNumberProtector;
    }

    public async Task<PaginatedList<BankAccountListDto>> Handle(GetBankAccountsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar cuentas bancarias.");
        }

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
            MaskedAccountNumber = ba.AccountNumber,
            Currency = ba.Currency,
            OpeningBalance = ba.OpeningBalance,
            IsActive = ba.IsActive
        });

        var result = await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);

        foreach (var item in result.Items)
        {
            item.MaskedAccountNumber = _accountNumberProtector.MaskFromProtected(item.MaskedAccountNumber);
        }

        return result;
    }
}
