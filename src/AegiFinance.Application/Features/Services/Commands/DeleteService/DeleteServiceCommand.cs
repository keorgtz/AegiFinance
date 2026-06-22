using MediatR;

namespace AegiFinance.Application.Features.Services.Commands.DeleteService;

public class DeleteServiceCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteServiceCommand() { }

    public DeleteServiceCommand(Guid id)
    {
        Id = id;
    }
}
