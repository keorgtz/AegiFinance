namespace AegiFinance.Application.Dtos;

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public bool IsSystemGenerated { get; set; }
    public bool IsActive { get; set; }
}
