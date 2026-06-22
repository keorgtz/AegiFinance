using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Ledger.Commands.RegisterTransfer;

public class RegisterTransferCommandHandler : IRequestHandler<RegisterTransferCommand, TransferGroupDto>
{
    private readonly ILedgerService _ledgerService;
    private readonly IApplicationDbContext _context;

    public RegisterTransferCommandHandler(ILedgerService ledgerService, IApplicationDbContext context)
    {
        _ledgerService = ledgerService;
        _context = context;
    }

    public async Task<TransferGroupDto> Handle(RegisterTransferCommand request, CancellationToken cancellationToken)
    {
        var transferGroup = await _ledgerService.RegisterTransferAsync(
            new RegisterTransferRequest(
                request.FromBankAccountId,
                request.ToBankAccountId,
                request.Amount,
                request.Currency,
                request.Date,
                request.Description),
            cancellationToken);

        var result = await _context.TransferGroups
            .AsNoTracking()
            .Include(tg => tg.FromEntry)
            .ThenInclude(e => e.BankAccount)
            .Include(tg => tg.ToEntry)
            .ThenInclude(e => e.BankAccount)
            .FirstAsync(tg => tg.Id == transferGroup.Id, cancellationToken);

        return new TransferGroupDto
        {
            Id = result.Id,
            FromEntryId = result.FromEntryId,
            FromBankAccountId = result.FromEntry.BankAccountId,
            FromBankAccountName = result.FromEntry.BankAccount.Name,
            ToEntryId = result.ToEntryId,
            ToBankAccountId = result.ToEntry.BankAccountId,
            ToBankAccountName = result.ToEntry.BankAccount.Name,
            Amount = result.Amount,
            Date = result.Date,
            Description = result.Description
        };
    }
}
