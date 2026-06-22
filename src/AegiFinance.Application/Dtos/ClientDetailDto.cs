namespace AegiFinance.Application.Dtos;

public class ClientDetailDto
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
    public List<ClientContactDto> Contacts { get; set; } = new List<ClientContactDto>();
    public List<ClientNoteDto> NotesList { get; set; } = new List<ClientNoteDto>();
}
