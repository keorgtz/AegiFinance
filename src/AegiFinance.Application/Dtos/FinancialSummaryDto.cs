namespace AegiFinance.Application.Dtos;

public class FinancialSummaryDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string DisplayCurrency { get; set; } = "MXN";
    public decimal? ExchangeRateUsed { get; set; }
    public decimal TotalCharges { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalAdjustments { get; set; }
    public decimal CurrentBalance { get; set; }
}
