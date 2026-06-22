namespace AegiFinance.Application.Dtos;

public class AccountStatementDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime StatementDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string DisplayCurrency { get; set; } = "MXN";
    public decimal? ExchangeRateUsed { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal TotalCharges { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalAdjustments { get; set; }
    public decimal FinalBalance { get; set; }
    public List<AccountStatementItemDto> Items { get; set; } = new();
}
