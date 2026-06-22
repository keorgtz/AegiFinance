using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.DeleteClient;

public class DeleteClientCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteClientCommand() { }

    public DeleteClientCommand(Guid id)
    {
        Id = id;
    }
}
