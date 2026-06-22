using AegiFinance.Application.Common.Interfaces;

namespace AegiFinance.Infrastructure.Services;

public class PinHasher : IPinHasher
{
    public string HashPin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length is < 4 or > 6)
        {
            throw new ArgumentException("El PIN debe tener entre 4 y 6 dígitos.", nameof(pin));
        }

        if (!pin.All(char.IsDigit))
        {
            throw new ArgumentException("El PIN debe ser numérico.", nameof(pin));
        }

        return BCrypt.Net.BCrypt.HashPassword(pin);
    }

    public bool VerifyPin(string pin, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(pin, hash);
    }
}
