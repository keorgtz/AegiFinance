using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.SetPrimaryContact;

public class SetPrimaryContactCommand : IRequest
{
    public Guid ClientId { get; set; }
    public Guid ContactId { get; set; }

    public SetPrimaryContactCommand() { }

    public SetPrimaryContactCommand(Guid clientId, Guid contactId)
    {
        ClientId = clientId;
        ContactId = contactId;
    }
}
