using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.CreateManualCharge;

public class CreateManualChargeCommand : IRequest<BillingItemListDto>
{
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public string Description { get; set; } = null!;
    public DateTime ChargeDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string IdempotencyKey { get; set; } = null!;
}
