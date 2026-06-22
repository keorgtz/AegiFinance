using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.AddClientNote;

public class AddClientNoteCommand : IRequest<ClientNoteDto>
{
    public Guid ClientId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
}
