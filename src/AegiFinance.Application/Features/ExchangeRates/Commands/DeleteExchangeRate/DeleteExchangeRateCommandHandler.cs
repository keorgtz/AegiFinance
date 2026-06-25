using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;

public class DeleteExchangeRateCommandHandler : IRequestHandler<DeleteExchangeRateCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteExchangeRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _context.ExchangeRates
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rate is null)
        {
            throw new InvalidOperationException("El tipo de cambio no existe.");
        }

        _context.ExchangeRates.Remove(rate);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
