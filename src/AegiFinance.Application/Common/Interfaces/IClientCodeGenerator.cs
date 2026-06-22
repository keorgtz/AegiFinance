namespace AegiFinance.Application.Common.Interfaces;

public interface IClientCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
