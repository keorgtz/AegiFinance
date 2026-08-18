using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class Client : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? TradeName { get; set; }
    public string? TaxId { get; set; }
    public string? BillingEmail { get; set; }
    public string? BillingAddress { get; set; }
    public string? Phone { get; set; }
    public ClientStatus Status { get; set; }
    public string? Notes { get; set; }
    public string PresentationCurrency { get; set; } = "MXN";
    public int PaymentTermsDays { get; set; }
    public decimal CreditLimit { get; set; }
    public string? CommercialTerms { get; set; }
    public Guid? AccountManagerUserId { get; set; }
    public User? AccountManagerUser { get; set; }
    public string NormalizedName { get; set; } = string.Empty;
    public string? NormalizedTaxId { get; set; }
    public string? NormalizedBillingEmail { get; set; }

    public Guid? CategoryId { get; set; }
    public ClientCategory? Category { get; set; }

    public ICollection<ClientTag> Tags { get; set; } = new List<ClientTag>();
    public ICollection<ClientNote> NotesList { get; set; } = new List<ClientNote>();
    public ICollection<ClientContact> Contacts { get; set; } = new List<ClientContact>();
    public ICollection<ClientDocument> Documents { get; set; } = new List<ClientDocument>();
}
