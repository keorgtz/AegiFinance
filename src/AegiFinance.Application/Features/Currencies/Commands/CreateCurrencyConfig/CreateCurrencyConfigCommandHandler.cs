using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Currencies.Commands.CreateCurrencyConfig;

public class CreateCurrencyConfigCommandHandler : IRequestHandler<CreateCurrencyConfigCommand, CurrencyConfigDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCurrencyConfigCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyConfigDto> Handle(CreateCurrencyConfigCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.CurrencyConfigs
            .AsNoTracking()
            .AnyAsync(c => c.Code == request.Code, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Ya existe una moneda configurada con el código '{request.Code}'.");
        }

        if (request.IsDefault)
        {
            await _context.CurrencyConfigs
                .Where(c => c.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDefault, false), cancellationToken);
        }

        var config = new CurrencyConfig
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Symbol = request.Symbol,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CurrencyConfigs.Add(config);
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
