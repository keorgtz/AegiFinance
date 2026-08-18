using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class GeneralLedgerAccount : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public GeneralLedgerAccountType AccountType { get; set; }
    public GeneralLedgerAccountPurpose Purpose { get; set; }
    public Guid? ParentAccountId { get; set; }
    public GeneralLedgerAccount? ParentAccount { get; set; }
    public Guid? BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }
    public string Currency { get; set; } = "MXN";
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
}
