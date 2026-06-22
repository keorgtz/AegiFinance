using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class BillingCycle : AuditableEntity
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BillingCycleStatus Status { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
}
