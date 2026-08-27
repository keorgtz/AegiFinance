using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.UpdatePaymentPromiseStatus;

public class UpdatePaymentPromiseStatusCommand : IRequest
{
    public Guid PaymentPromiseId { get; set; }
    public PaymentPromiseStatus Status { get; set; }
}
