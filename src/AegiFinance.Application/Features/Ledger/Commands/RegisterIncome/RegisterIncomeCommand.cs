using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.RegisterIncome;

public class RegisterIncomeCommand : IRequest<LedgerEntryDto>
{
    public Guid BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? BillingItemId { get; set; }
}
