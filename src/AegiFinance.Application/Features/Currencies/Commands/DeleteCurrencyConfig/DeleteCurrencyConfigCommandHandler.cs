using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Currencies.Commands.DeleteCurrencyConfig;

public class DeleteCurrencyConfigCommandHandler : IRequestHandler<DeleteCurrencyConfigCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCurrencyConfigCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCurrencyConfigCommand request, CancellationToken cancellationToken)
    {
        var config = await _context.CurrencyConfigs
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (config is null)
        {
            throw new InvalidOperationException("La moneda no existe.");
        }

        if (config.IsDefault)
        {
            throw new InvalidOperationException("No se puede eliminar la moneda predeterminada.");
        }

        var hasExchangeRates = await _context.ExchangeRates
            .AsNoTracking()
            .AnyAsync(r => r.CurrencyCode == config.Code, cancellationToken);

        if (hasExchangeRates)
        {
            throw new InvalidOperationException("No se puede eliminar una moneda con tipos de cambio registrados.");
        }

        _context.CurrencyConfigs.Remove(config);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
