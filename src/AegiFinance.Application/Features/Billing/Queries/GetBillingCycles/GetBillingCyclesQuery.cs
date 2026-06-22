using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Queries.GetBillingCycles;

public class GetBillingCyclesQuery : IRequest<List<BillingCycleDto>>
{
    public int? Year { get; set; }
    public int? Month { get; set; }
}
