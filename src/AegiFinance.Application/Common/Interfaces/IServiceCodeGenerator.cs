namespace AegiFinance.Application.Common.Interfaces;

public interface IServiceCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
