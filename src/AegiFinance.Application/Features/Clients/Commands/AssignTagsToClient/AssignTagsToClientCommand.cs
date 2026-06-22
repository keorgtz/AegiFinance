using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.AssignTagsToClient;

public class AssignTagsToClientCommand : IRequest
{
    public Guid ClientId { get; set; }
    public List<Guid> TagIds { get; set; } = new List<Guid>();
}
