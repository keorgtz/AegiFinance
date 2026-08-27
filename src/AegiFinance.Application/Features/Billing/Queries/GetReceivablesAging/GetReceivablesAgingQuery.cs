using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Queries.GetReceivablesAging;

public class GetReceivablesAgingQuery : IRequest<ReceivablesAgingDto>
{
    public DateTime? AsOfDate { get; set; }
    public Guid? ClientId { get; set; }
    public string Currency { get; set; } = "MXN";
}
