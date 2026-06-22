using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.GenerateBilling;

public class GenerateBillingCommand : IRequest<BillingGenerationResult>
{
    public int Year { get; set; }
    public int? Month { get; set; }
}
