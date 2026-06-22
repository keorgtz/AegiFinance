using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.AddClientContact;

public class AddClientContactCommand : IRequest<ClientContactDto>
{
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public bool IsPrimary { get; set; }
}
