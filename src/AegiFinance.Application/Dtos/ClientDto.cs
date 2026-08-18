namespace AegiFinance.Application.Dtos;

public class ClientDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string? TaxId { get; set; }
    public string? BillingEmail { get; set; }
    public string? BillingAddress { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<ClientTagDto> Tags { get; set; } = new List<ClientTagDto>();
    public string PresentationCurrency { get; set; } = "MXN";
    public int PaymentTermsDays { get; set; }
    public decimal CreditLimit { get; set; }
    public string? CommercialTerms { get; set; }
    public Guid? AccountManagerUserId { get; set; }
    public string? AccountManagerName { get; set; }
}
