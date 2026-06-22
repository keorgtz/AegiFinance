namespace AegiFinance.Domain.Entities;

public class ClientPinCredential : BaseEntity
{
    public Guid UserId { get; set; }
    public string PinHash { get; set; } = string.Empty;
}
