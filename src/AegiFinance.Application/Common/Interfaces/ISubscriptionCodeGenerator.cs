namespace AegiFinance.Application.Common.Interfaces;

public interface ISubscriptionCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
