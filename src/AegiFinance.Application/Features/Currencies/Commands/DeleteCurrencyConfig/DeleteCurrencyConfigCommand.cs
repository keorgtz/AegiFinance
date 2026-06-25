using MediatR;

namespace AegiFinance.Application.Features.Currencies.Commands.DeleteCurrencyConfig;

public class DeleteCurrencyConfigCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteCurrencyConfigCommand() { }

    public DeleteCurrencyConfigCommand(Guid id)
    {
        Id = id;
    }
}
