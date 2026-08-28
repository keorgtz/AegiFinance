using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class ReportSchedule : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public FinancialReportKind ReportKind { get; set; }
    public ReportScheduleFrequency Frequency { get; set; }
    public string Currency { get; set; } = "MXN";
    public int RollingDays { get; set; } = 30;
    public Guid? AccountId { get; set; }
    public GeneralLedgerAccount? Account { get; set; }
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }
    public int RunAtMinuteUtc { get; set; }
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ICollection<ReportRun> Runs { get; set; } = new List<ReportRun>();
}
