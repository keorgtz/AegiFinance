using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.CancelBillingItem;

public class CancelBillingItemCommand : IRequest
{
    public Guid BillingItemId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
