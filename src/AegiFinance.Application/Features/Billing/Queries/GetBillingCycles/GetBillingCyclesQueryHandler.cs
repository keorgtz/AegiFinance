using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Queries.GetBillingCycles;

public class GetBillingCyclesQueryHandler : IRequestHandler<GetBillingCyclesQuery, List<BillingCycleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBillingCyclesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<BillingCycleDto>> Handle(GetBillingCyclesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar los ciclos de facturación.");
        }

        var query = _context.BillingCycles
            .AsNoTracking()
            .AsQueryable();

        if (request.Year.HasValue)
        {
            query = query.Where(c => c.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            query = query.Where(c => c.Month == request.Month.Value);
        }

        var itemsCountQuery = _context.BillingItems
            .AsNoTracking()
            .GroupBy(bi => bi.BillingCycleId)
            .Select(g => new { BillingCycleId = g.Key, Count = g.Count() });

        var cycles = await query
            .OrderByDescending(c => c.Year)
            .ThenByDescending(c => c.Month)
            .Select(c => new BillingCycleDto
            {
                Id = c.Id,
                Year = c.Year,
                Month = c.Month,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString(),
                ClosedAt = c.ClosedAt,
                ClosedBy = c.ClosedBy
            })
            .ToListAsync(cancellationToken);

        var counts = await itemsCountQuery.ToDictionaryAsync(x => x.BillingCycleId, x => x.Count, cancellationToken);

        foreach (var cycle in cycles)
        {
            cycle.ItemsCount = counts.TryGetValue(cycle.Id, out var count) ? count : 0;
        }

        return cycles;
    }
}
