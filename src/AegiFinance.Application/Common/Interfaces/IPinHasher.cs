namespace AegiFinance.Application.Common.Interfaces;

public interface IPinHasher
{
    string HashPin(string pin);
    bool VerifyPin(string pin, string hash);
}
