using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Queries.GetBillingItems;

public class GetBillingItemsQuery : IRequest<PaginatedList<BillingItemListDto>>
{
    public Guid? BillingCycleId { get; set; }
    public Guid? ClientId { get; set; }
    public BillingItemStatus? Status { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? Currency { get; set; }
    public bool OutstandingOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
