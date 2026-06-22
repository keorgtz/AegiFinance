using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Ledger.Queries.GetLedgerEntries;

public class GetLedgerEntriesQuery : IRequest<PaginatedList<LedgerEntryListDto>>
{
    public Guid? BankAccountId { get; set; }
    public LedgerEntryType? EntryType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? ClientId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
