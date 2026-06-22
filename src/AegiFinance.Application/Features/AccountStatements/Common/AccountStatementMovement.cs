namespace AegiFinance.Application.Features.AccountStatements.Common;

internal sealed class AccountStatementMovement
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public decimal DebitMXN { get; set; }
    public decimal CreditMXN { get; set; }
}
