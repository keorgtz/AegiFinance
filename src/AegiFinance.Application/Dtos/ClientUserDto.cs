namespace AegiFinance.Application.Dtos;

public class ClientUserDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool HasPin { get; set; }
}
