namespace AegiFinance.Application.Dtos;

public class ClientCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
