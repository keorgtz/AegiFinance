using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class ReportRun : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid ReportScheduleId { get; set; }
    public ReportSchedule ReportSchedule { get; set; } = null!;
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public ReportRunStatus Status { get; set; } = ReportRunStatus.Pending;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string FilterJson { get; set; } = "{}";
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public string? Content { get; set; }
    public string? ResultHash { get; set; }
    public int RowCount { get; set; }
    public string? Error { get; set; }
}
