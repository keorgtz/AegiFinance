namespace AegiFinance.Application.Dtos;

public class BankAccountListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; }
}
