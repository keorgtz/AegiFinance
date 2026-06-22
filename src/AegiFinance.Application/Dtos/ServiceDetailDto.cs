namespace AegiFinance.Application.Dtos;

public class ServiceDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string BillingType { get; set; } = string.Empty;
    public decimal DefaultPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public List<ServicePriceHistoryDto> PriceHistory { get; set; } = new List<ServicePriceHistoryDto>();
}
