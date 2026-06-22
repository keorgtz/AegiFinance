using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommandHandler : IRequestHandler<CreateExchangeRateCommand, ExchangeRateDto>
{
    private readonly IApplicationDbContext _context;

    public CreateExchangeRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRateDto> Handle(CreateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            CurrencyCode = request.CurrencyCode,
            RateToMXN = request.RateToMXN,
            RateFromMXN = request.RateFromMXN,
            EffectiveDate = request.EffectiveDate,
            Source = ExchangeRateSource.Manual,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ExchangeRates.Add(rate);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExchangeRateDto
        {
            Id = rate.Id,
            CurrencyCode = rate.CurrencyCode,
            RateToMXN = rate.RateToMXN,
            RateFromMXN = rate.RateFromMXN,
            EffectiveDate = rate.EffectiveDate,
            Source = rate.Source.ToString()
        };
    }
}
