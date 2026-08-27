namespace AegiFinance.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? OrganizationId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
}
