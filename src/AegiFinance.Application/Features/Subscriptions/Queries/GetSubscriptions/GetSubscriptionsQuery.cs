using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptions;

public class GetSubscriptionsQuery : IRequest<PaginatedList<SubscriptionListDto>>
{
    public Guid? ClientId { get; set; }
    public Guid? ServiceId { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
