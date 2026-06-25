using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.DeleteClientContact;

public class DeleteClientContactCommand : IRequest
{
    public Guid ClientId { get; set; }
    public Guid ContactId { get; set; }

    public DeleteClientContactCommand() { }

    public DeleteClientContactCommand(Guid clientId, Guid contactId)
    {
        ClientId = clientId;
        ContactId = contactId;
    }
}
