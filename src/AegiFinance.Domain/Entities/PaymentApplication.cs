using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class PaymentApplication : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public string ReceiptNumber { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
    public PaymentApplicationPriority Priority { get; set; }
    public PaymentApplicationOrigin Origin { get; set; }
    public PaymentApplicationStatus Status { get; set; } = PaymentApplicationStatus.Active;
    public Guid? PreferredServiceId { get; set; }
    public Service? PreferredService { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal UnappliedAmount { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime AppliedAt { get; set; }
    public Guid? AppliedBy { get; set; }
    public DateTime? ReversedAt { get; set; }
    public Guid? ReversedBy { get; set; }
    public string? ReversalReason { get; set; }
    public Guid? ReappliesPaymentApplicationId { get; set; }
    public PaymentApplication? ReappliesPaymentApplication { get; set; }
    public ICollection<PaymentApplicationPayment> Payments { get; set; } = new List<PaymentApplicationPayment>();
    public ICollection<SubscriptionAllocation> Allocations { get; set; } = new List<SubscriptionAllocation>();
}
