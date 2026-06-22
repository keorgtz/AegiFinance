namespace AegiFinance.Application.Dtos;

public class ClientListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string? TaxId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? PrimaryContactName { get; set; }
    public List<ClientTagDto> Tags { get; set; } = new List<ClientTagDto>();
}
