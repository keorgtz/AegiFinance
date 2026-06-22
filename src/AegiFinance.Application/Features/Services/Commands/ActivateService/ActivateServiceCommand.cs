using MediatR;

namespace AegiFinance.Application.Features.Services.Commands.ActivateService;

public class ActivateServiceCommand : IRequest
{
    public Guid Id { get; set; }

    public ActivateServiceCommand() { }

    public ActivateServiceCommand(Guid id)
    {
        Id = id;
    }
}
