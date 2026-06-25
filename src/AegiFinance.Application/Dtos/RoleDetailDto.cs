namespace AegiFinance.Application.Dtos;

public class RoleDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? UserType { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}
