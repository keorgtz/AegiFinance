using MediatR;

namespace AegiFinance.Application.Features.Services.Commands.DeactivateService;

public class DeactivateServiceCommand : IRequest
{
    public Guid Id { get; set; }

    public DeactivateServiceCommand() { }

    public DeactivateServiceCommand(Guid id)
    {
        Id = id;
    }
}
