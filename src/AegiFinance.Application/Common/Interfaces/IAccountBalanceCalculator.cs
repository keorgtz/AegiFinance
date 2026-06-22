namespace AegiFinance.Application.Common.Interfaces;

public interface IAccountBalanceCalculator
{
    Task<decimal> CalculateBalanceAsync(Guid bankAccountId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
}
