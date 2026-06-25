using MediatR;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;

public class DeleteExchangeRateCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteExchangeRateCommand() { }

    public DeleteExchangeRateCommand(Guid id)
    {
        Id = id;
    }
}
