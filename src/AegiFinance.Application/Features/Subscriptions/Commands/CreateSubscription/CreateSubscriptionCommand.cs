using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.CreateSubscription;

public class CreateSubscriptionCommand : IRequest<SubscriptionDto>
{
    public Guid ClientId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? ServiceVersionId { get; set; }
    public BillingType BillingType { get; set; } = BillingType.Monthly;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int BillingDay { get; set; }
    public int? CustomIntervalDays { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public ProrationPolicy ProrationPolicy { get; set; }
    public string? ContractTerms { get; set; }
    public bool AutoRenew { get; set; }
    public string? Notes { get; set; }
}
