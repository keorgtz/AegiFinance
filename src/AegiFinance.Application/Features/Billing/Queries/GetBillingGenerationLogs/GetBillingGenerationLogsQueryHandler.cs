using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Queries.GetBillingGenerationLogs;

public class GetBillingGenerationLogsQueryHandler : IRequestHandler<GetBillingGenerationLogsQuery, List<BillingGenerationLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBillingGenerationLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BillingGenerationLogDto>> Handle(GetBillingGenerationLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BillingGenerationLogs
            .AsNoTracking()
            .Include(log => log.BillingCycle)
            .AsQueryable();

        if (request.BillingCycleId.HasValue)
        {
            query = query.Where(log => log.BillingCycleId == request.BillingCycleId.Value);
        }

        if (request.Year.HasValue)
        {
            query = query.Where(log => log.BillingCycle != null && log.BillingCycle.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            query = query.Where(log => log.BillingCycle != null && log.BillingCycle.Month == request.Month.Value);
        }

        return await query
            .OrderByDescending(log => log.StartedAt)
            .Take(request.Limit)
            .Select(log => new BillingGenerationLogDto
            {
                Id = log.Id,
                BillingCycleId = log.BillingCycleId,
                BillingCycleLabel = log.BillingCycle != null
                    ? $"{log.BillingCycle.Year}-{(log.BillingCycle.Month.HasValue ? log.BillingCycle.Month.Value.ToString("D2") : "**")}"
                    : null,
                StartedAt = log.StartedAt,
                FinishedAt = log.FinishedAt,
                Status = log.Status,
                ItemsGenerated = log.ItemsGenerated,
                Errors = log.Errors,
                TriggeredBy = log.TriggeredBy
            })
            .ToListAsync(cancellationToken);
    }
}
