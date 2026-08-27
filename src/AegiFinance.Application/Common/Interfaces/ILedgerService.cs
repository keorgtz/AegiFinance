using AegiFinance.Domain.Entities;

namespace AegiFinance.Application.Common.Interfaces;

public interface ILedgerService
{
    Task<LedgerEntry> RegisterIncomeAsync(RegisterIncomeRequest request, CancellationToken cancellationToken = default);
    Task<LedgerEntry> RegisterExpenseAsync(RegisterExpenseRequest request, CancellationToken cancellationToken = default);
    Task<TransferGroup> RegisterTransferAsync(RegisterTransferRequest request, CancellationToken cancellationToken = default);
    Task<LedgerEntry> RegisterAdjustmentAsync(RegisterAdjustmentRequest request, CancellationToken cancellationToken = default);
}

public record RegisterIncomeRequest(Guid BankAccountId, decimal Amount, string Currency, DateTime Date, string Description, string? Reference, Guid? ClientId, Guid? BillingItemId);
public record RegisterExpenseRequest(Guid BankAccountId, decimal Amount, string Currency, DateTime Date, string Description, string? Reference);
public record RegisterTransferRequest(Guid FromBankAccountId, Guid ToBankAccountId, decimal Amount, string Currency, DateTime Date, string? Description);
public record RegisterAdjustmentRequest(Guid BankAccountId, decimal Amount, string Currency, DateTime Date, string Description, string Reason);
