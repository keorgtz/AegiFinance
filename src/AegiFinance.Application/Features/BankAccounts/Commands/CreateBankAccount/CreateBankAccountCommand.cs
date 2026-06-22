using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.BankAccounts.Commands.CreateBankAccount;

public class CreateBankAccountCommand : IRequest<BankAccountDto>
{
    public string Name { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal OpeningBalance { get; set; }
    public DateTime OpeningDate { get; set; }
    public bool IsActive { get; set; } = true;
}
