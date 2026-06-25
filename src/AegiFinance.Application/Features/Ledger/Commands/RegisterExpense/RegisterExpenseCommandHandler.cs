using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Ledger.Commands.RegisterExpense;

public class RegisterExpenseCommandHandler : IRequestHandler<RegisterExpenseCommand, LedgerEntryDto>
{
    private readonly ILedgerService _ledgerService;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RegisterExpenseCommandHandler(ILedgerService ledgerService, IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _ledgerService = ledgerService;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LedgerEntryDto> Handle(RegisterExpenseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para registrar egresos.");
        }

        var entry = await _ledgerService.RegisterExpenseAsync(
            new RegisterExpenseRequest(
                request.BankAccountId,
                request.Amount,
                request.Currency,
                request.Date,
                request.Description,
                request.Reference),
            cancellationToken);

        return await LoadLedgerEntryDtoAsync(entry.Id, cancellationToken);
    }

    private async Task<LedgerEntryDto> LoadLedgerEntryDtoAsync(Guid entryId, CancellationToken cancellationToken)
    {
        var entry = await _context.LedgerEntries
            .AsNoTracking()
            .Include(le => le.BankAccount)
            .Include(le => le.Client)
            .Include(le => le.BillingItem)
            .FirstAsync(le => le.Id == entryId, cancellationToken);

        return new LedgerEntryDto
        {
            Id = entry.Id,
            BankAccountId = entry.BankAccountId,
            BankAccountName = entry.BankAccount.Name,
            EntryType = entry.EntryType.ToString(),
            Amount = entry.Amount,
            Currency = entry.Currency,
            Date = entry.Date,
            Description = entry.Description,
            Reference = entry.Reference,
            ClientId = entry.ClientId,
            ClientName = entry.Client?.Name,
            BillingItemId = entry.BillingItemId,
            BillingItemDescription = entry.BillingItem?.Description,
            IsReconciled = entry.IsReconciled,
            ReconciledAt = entry.ReconciledAt
        };
    }
}
