namespace AegiFinance.Application.Dtos;

public class LedgerEntryListDto
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public bool IsReconciled { get; set; }
}
