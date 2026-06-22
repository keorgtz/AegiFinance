using MediatR;

namespace AegiFinance.Application.Features.ClientTags.Commands.DeleteClientTag;

public class DeleteClientTagCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteClientTagCommand() { }

    public DeleteClientTagCommand(Guid id)
    {
        Id = id;
    }
}
