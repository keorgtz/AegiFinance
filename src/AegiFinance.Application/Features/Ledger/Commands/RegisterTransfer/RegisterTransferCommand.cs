using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.RegisterTransfer;

public class RegisterTransferCommand : IRequest<TransferGroupDto>
{
    public Guid FromBankAccountId { get; set; }
    public Guid ToBankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
