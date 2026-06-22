using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ChangeSubscriptionPrice;

public class ChangeSubscriptionPriceCommand : IRequest<SubscriptionDto>
{
    public Guid Id { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
}
