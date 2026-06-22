namespace AegiFinance.Domain.Entities;

public class BankAccount : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal OpeningBalance { get; set; }
    public DateTime OpeningDate { get; set; }
    public bool IsActive { get; set; }
}
