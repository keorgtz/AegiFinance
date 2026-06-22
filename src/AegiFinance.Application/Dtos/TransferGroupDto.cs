namespace AegiFinance.Application.Dtos;

public class TransferGroupDto
{
    public Guid Id { get; set; }
    public Guid FromEntryId { get; set; }
    public Guid FromBankAccountId { get; set; }
    public string FromBankAccountName { get; set; } = string.Empty;
    public Guid ToEntryId { get; set; }
    public Guid ToBankAccountId { get; set; }
    public string ToBankAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
