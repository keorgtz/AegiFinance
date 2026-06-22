namespace AegiFinance.Application.Dtos;

public class ClientNoteDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
}
