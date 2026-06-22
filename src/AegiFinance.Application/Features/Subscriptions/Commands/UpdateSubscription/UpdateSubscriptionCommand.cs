using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.UpdateSubscription;

public class UpdateSubscriptionCommand : IRequest<SubscriptionDto>
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid ServiceId { get; set; }
    public BillingType BillingType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int BillingDay { get; set; }
    public bool AutoRenew { get; set; }
    public string? Notes { get; set; }
}
