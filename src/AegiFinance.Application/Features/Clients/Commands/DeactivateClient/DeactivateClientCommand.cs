using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.DeactivateClient;

public class DeactivateClientCommand : IRequest
{
    public Guid Id { get; set; }

    public DeactivateClientCommand() { }

    public DeactivateClientCommand(Guid id)
    {
        Id = id;
    }
}
