using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ChangeSubscriptionPlan;

public sealed class ChangeSubscriptionPlanCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid ServiceVersionId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? TaxPercent { get; set; }
    public int? BillingDay { get; set; }
    public int? CustomIntervalDays { get; set; }
    public ProrationPolicy? ProrationPolicy { get; set; }
    public string? Terms { get; set; }
    public string? Reason { get; set; }
}
