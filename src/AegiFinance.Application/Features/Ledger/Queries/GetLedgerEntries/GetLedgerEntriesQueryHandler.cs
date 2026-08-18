using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Dashboard;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Ledger.Queries.GetLedgerEntries;

public class GetLedgerEntriesQueryHandler : IRequestHandler<GetLedgerEntriesQuery, PaginatedList<LedgerEntryListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLedgerEntriesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<LedgerEntryListDto>> Handle(GetLedgerEntriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LedgerEntries
            .AsNoTracking()
            .Include(le => le.BankAccount)
            .Include(le => le.Client)
            .AsQueryable();

        Guid? clientId = request.ClientId;
        if (_currentUserService.IsClientUser())
        {
            if (!_currentUserService.ClientId.HasValue)
            {
                return new PaginatedList<LedgerEntryListDto>(new List<LedgerEntryListDto>(), 0, request.PageNumber, request.PageSize);
            }

            clientId = _currentUserService.ClientId.Value;
        }

        query = query.ApplyOperationalScope(clientId, request.BankAccountId, request.DateFrom, request.DateTo, request.Currency?.ToUpperInvariant());

        if (request.EntryType.HasValue)
        {
            query = query.Where(le => le.EntryType == request.EntryType.Value);
        }

        if (request.IsReconciled.HasValue) query = query.Where(item => item.IsReconciled == request.IsReconciled.Value);
        if (request.HasUnappliedBalance == true)
        {
            query = query.Where(item => item.EntryType == AegiFinance.Domain.Enums.LedgerEntryType.Income &&
                (_context.SubscriptionAllocations.Where(allocation => allocation.LedgerEntryId == item.Id)
                    .Sum(allocation => (decimal?)allocation.Amount) ?? 0) < item.Amount);
        }

        query = query.OrderByDescending(le => le.Date).ThenByDescending(le => le.CreatedAt);

        var projected = query.Select(le => new LedgerEntryListDto
        {
            Id = le.Id,
            BankAccountId = le.BankAccountId,
            BankAccountName = le.BankAccount.Name,
            EntryType = le.EntryType.ToString(),
            Amount = le.Amount,
            Currency = le.Currency,
            Date = le.Date,
            Description = le.Description,
            Reference = le.Reference,
            ClientId = le.ClientId,
            ClientName = le.Client != null ? le.Client.Name : null,
            IsReconciled = le.IsReconciled
        });

        return await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
