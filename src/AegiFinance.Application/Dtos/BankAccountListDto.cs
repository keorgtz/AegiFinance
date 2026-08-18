namespace AegiFinance.Application.Dtos;

public class BankAccountListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? MaskedAccountNumber { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public DateTime OpeningDate { get; set; }
    public decimal LedgerBalance { get; set; }
    public decimal? BankBalance { get; set; }
    public DateTime? BankBalanceAsOfDate { get; set; }
    public decimal? ComparisonLedgerBalance { get; set; }
    public decimal? Difference { get; set; }
    public bool IsActive { get; set; }
}
