using System.Security.Cryptography;
using AegiFinance.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace AegiFinance.Infrastructure.Services;

public class AccountNumberProtector : IAccountNumberProtector
{
    private const string Purpose = "AegiFinance.BankAccount.AccountNumber";
    private readonly IDataProtector _protector;

    public AccountNumberProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(Purpose);
    }

    public string Protect(string accountNumber)
    {
        return _protector.Protect(accountNumber);
    }

    public string? MaskFromProtected(string? protectedAccountNumber)
    {
        if (string.IsNullOrWhiteSpace(protectedAccountNumber))
        {
            return null;
        }

        string plain;
        try
        {
            plain = _protector.Unprotect(protectedAccountNumber);
        }
        catch (CryptographicException)
        {
            return "****";
        }

        if (plain.Length <= 4)
        {
            return new string('*', plain.Length);
        }

        return new string('*', plain.Length - 4) + plain[^4..];
    }
}
