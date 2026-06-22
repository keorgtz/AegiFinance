using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Queries.GetBillingGenerationLogs;

public class GetBillingGenerationLogsQuery : IRequest<List<BillingGenerationLogDto>>
{
    public Guid? BillingCycleId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int Limit { get; set; } = 100;
}
