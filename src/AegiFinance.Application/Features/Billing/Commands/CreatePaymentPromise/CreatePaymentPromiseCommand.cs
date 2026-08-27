using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.CreatePaymentPromise;

public class CreatePaymentPromiseCommand : IRequest<PaymentPromiseDto>
{
    public Guid BillingItemId { get; set; }
    public decimal PromisedAmount { get; set; }
    public DateTime PromiseDate { get; set; }
    public string? Notes { get; set; }
}
