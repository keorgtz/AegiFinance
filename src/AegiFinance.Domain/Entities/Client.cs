using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class Client : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? TradeName { get; set; }
    public string? TaxId { get; set; }
    public string? BillingEmail { get; set; }
    public string? BillingAddress { get; set; }
    public string? Phone { get; set; }
    public ClientStatus Status { get; set; }
    public string? Notes { get; set; }

    public Guid? CategoryId { get; set; }
    public ClientCategory? Category { get; set; }

    public ICollection<ClientTag> Tags { get; set; } = new List<ClientTag>();
    public ICollection<ClientNote> NotesList { get; set; } = new List<ClientNote>();
    public ICollection<ClientContact> Contacts { get; set; } = new List<ClientContact>();
}
