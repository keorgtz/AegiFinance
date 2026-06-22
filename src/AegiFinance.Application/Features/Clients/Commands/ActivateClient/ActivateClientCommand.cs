using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.ActivateClient;

public class ActivateClientCommand : IRequest
{
    public Guid Id { get; set; }

    public ActivateClientCommand() { }

    public ActivateClientCommand(Guid id)
    {
        Id = id;
    }
}
