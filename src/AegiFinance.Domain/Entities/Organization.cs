namespace AegiFinance.Domain.Entities;

public class Organization : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<ServiceCategory> ServiceCategories { get; set; } = new List<ServiceCategory>();
    public ICollection<AccountingPeriod> AccountingPeriods { get; set; } = new List<AccountingPeriod>();
    public ICollection<AccountingPeriodReopenRequest> AccountingPeriodReopenRequests { get; set; } = new List<AccountingPeriodReopenRequest>();
    public ICollection<ReportSchedule> ReportSchedules { get; set; } = new List<ReportSchedule>();
    public ICollection<ReportRun> ReportRuns { get; set; } = new List<ReportRun>();
}
