using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class PaymentPromise : AuditableEntity
{
    public Guid BillingItemId { get; set; }
    public BillingItem BillingItem { get; set; } = null!;
    public decimal PromisedAmount { get; set; }
    public DateTime PromiseDate { get; set; }
    public PaymentPromiseStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
}
