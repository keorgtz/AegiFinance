namespace AegiFinance.Domain.Entities;

public sealed class BankImportProfile : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string AdapterCode { get; set; } = "generic";
    public string DateColumn { get; set; } = string.Empty;
    public string DescriptionColumn { get; set; } = string.Empty;
    public string? ReferenceColumn { get; set; }
    public string? AmountColumn { get; set; }
    public string? DebitColumn { get; set; }
    public string? CreditColumn { get; set; }
    public string? CurrencyColumn { get; set; }
    public string? BalanceColumn { get; set; }
    public string? DateFormat { get; set; }
    public string Delimiter { get; set; } = ",";
    public int HeaderRow { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
