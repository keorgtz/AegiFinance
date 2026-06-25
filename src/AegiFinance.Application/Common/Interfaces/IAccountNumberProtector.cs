namespace AegiFinance.Application.Common.Interfaces;

public interface IAccountNumberProtector
{
    string Protect(string accountNumber);
    string? MaskFromProtected(string? protectedAccountNumber);
}
