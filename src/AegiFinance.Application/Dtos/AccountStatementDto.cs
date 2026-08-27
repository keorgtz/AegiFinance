namespace AegiFinance.Application.Dtos;

public class AccountStatementDto
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public DateTime StatementDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string DisplayCurrency { get; set; } = "MXN";
    public Guid? SubscriptionId { get; set; }
    public string? SubscriptionCode { get; set; }
    public string? ServiceName { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal TotalCharges { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalAdjustments { get; set; }
    public decimal FinalBalance { get; set; }
    public decimal OverdueBalance { get; set; }
    public bool IsSubledgerView { get; set; }
    public string ScopeDescription { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
    public bool FormulaValid { get; set; }
    public List<AccountStatementItemDto> Items { get; set; } = new();
}
