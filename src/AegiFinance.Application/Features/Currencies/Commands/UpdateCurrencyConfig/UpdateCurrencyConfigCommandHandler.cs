using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Currencies.Commands.UpdateCurrencyConfig;

public class UpdateCurrencyConfigCommandHandler : IRequestHandler<UpdateCurrencyConfigCommand, CurrencyConfigDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCurrencyConfigCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyConfigDto> Handle(UpdateCurrencyConfigCommand request, CancellationToken cancellationToken)
    {
        var config = await _context.CurrencyConfigs
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (config is null)
        {
            throw new InvalidOperationException("La moneda no existe.");
        }

        if (config.IsDefault && !request.IsDefault)
        {
            throw new InvalidOperationException("Debe existir siempre una moneda por defecto. Asigne otra moneda como predeterminada primero.");
        }

        if (request.IsDefault && !config.IsDefault)
        {
            await _context.CurrencyConfigs
                .Where(c => c.IsDefault && c.Id != config.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDefault, false), cancellationToken);
        }

        config.Name = request.Name;
        config.Symbol = request.Symbol;
        config.IsActive = request.IsActive;
        config.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync(cancellationToken);

        return new CurrencyConfigDto
        {
            Id = config.Id,
            Code = config.Code,
            Name = config.Name,
            Symbol = config.Symbol,
            IsActive = config.IsActive,
            IsDefault = config.IsDefault
        };
    }
}
