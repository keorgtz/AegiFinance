namespace AegiFinance.Application.Dtos;

public class AccountStatementItemDto
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public decimal OriginalAmountMXN { get; set; }
    public decimal? ExchangeRateUsed { get; set; }
}
