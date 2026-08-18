using AegiFinance.Domain.Entities;

namespace AegiFinance.Domain.Accounting;

public static class BankAccountRules
{
    public static void ValidateMovement(BankAccount account, string currency)
    {
        if (!account.IsActive)
            throw new InvalidOperationException("La cuenta bancaria está inactiva y no acepta nuevos movimientos.");

        if (!string.Equals(account.Currency, currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"La moneda del movimiento debe ser {account.Currency}.");
    }

    public static void ValidateTransfer(BankAccount source, BankAccount destination, decimal amount, string currency)
    {
        if (source.Id == destination.Id)
            throw new InvalidOperationException("La cuenta de origen y destino deben ser diferentes.");
        if (amount <= 0)
            throw new InvalidOperationException("El monto de la transferencia debe ser mayor a cero.");

        ValidateMovement(source, currency);
        ValidateMovement(destination, currency);
        if (!string.Equals(source.Currency, destination.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Las transferencias directas requieren cuentas de la misma moneda.");
    }
}
