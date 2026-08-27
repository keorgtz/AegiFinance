using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.AddBillingAdjustment;

public class AddBillingAdjustmentCommand : IRequest<BillingAdjustmentDto>
{
    public Guid BillingItemId { get; set; }
    public BillingAdjustmentType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Reason { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
}
