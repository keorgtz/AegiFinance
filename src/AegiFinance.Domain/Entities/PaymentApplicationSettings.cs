using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class PaymentApplicationSettings : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public PaymentApplicationPriority DefaultPriority { get; set; } = PaymentApplicationPriority.DueDate;
}
