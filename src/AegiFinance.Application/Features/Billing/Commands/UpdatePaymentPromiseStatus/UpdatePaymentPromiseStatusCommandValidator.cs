using AegiFinance.Domain.Enums;
using FluentValidation;

namespace AegiFinance.Application.Features.Billing.Commands.UpdatePaymentPromiseStatus;

public class UpdatePaymentPromiseStatusCommandValidator : AbstractValidator<UpdatePaymentPromiseStatusCommand>
{
    public UpdatePaymentPromiseStatusCommandValidator()
    {
        RuleFor(x => x.PaymentPromiseId).NotEmpty();
        RuleFor(x => x.Status).Must(x => x is PaymentPromiseStatus.Fulfilled or PaymentPromiseStatus.Broken or PaymentPromiseStatus.Cancelled);
    }
}
