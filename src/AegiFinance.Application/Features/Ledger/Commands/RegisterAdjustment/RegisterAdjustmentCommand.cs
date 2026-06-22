using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.RegisterAdjustment;

public class RegisterAdjustmentCommand : IRequest<LedgerEntryDto>
{
    public Guid BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
